using Agent.Core.Abstractions.Persistents;
using Agent.Core.Abstractions.Services;
using Agent.Core.Entities;
using Agent.Core.Implementations.Persistents;
using Agent.Core.VectorRecords;
using Anthropic.SDK.Messaging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Agent.Core.Implementations.Services;

public class SkillService : ISkillService
{
	private readonly ChatDbContext _dbContext;
	private readonly IQdrantRepository<SkillRoutingRecord> _skillRoutingRepo;

	public SkillService(
		IDbContextFactory<ChatDbContext> dbContextFactory,
		IQdrantRepository<SkillRoutingRecord> skillRoutingRepo)
	{
		_dbContext = dbContextFactory.CreateDbContext();
		_skillRoutingRepo = skillRoutingRepo;
	}

	public async Task<SkillEntity> CreateAsync(
		Guid categoryId,
		string skillCode,
		string name,
		string systemPrompt,
		string description,
		CancellationToken ct = default)
	{
		var category = await _dbContext.Categories
			.FirstOrDefaultAsync(c => c.Id == categoryId, ct)
			?? throw new InvalidOperationException($"Category {categoryId} not found");

		var entity = new SkillEntity
		{
			Id = Guid.NewGuid(),
			CategoryId = categoryId,
			Code = skillCode,
			Name = name,
			Description = description,
			SystemPrompt = systemPrompt,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		_dbContext.Skills.Add(entity);
		await _dbContext.SaveChangesAsync(ct);

		// Insert to Qdrant
		var record = new SkillRoutingRecord
		{
			Id = entity.Id,
			SkillCode = entity.Code,
			SkillName = entity.Name,
			CatCode = category.Code,
			CategoryName = category.Name,
			Description = description
		};

		await _skillRoutingRepo.UpsertAsync(record, ct);

		return entity;
	}

	public async Task<SkillEntity> UpdateAsync(
	Guid id,
	string? skillCode = null,
	string? name = null,
	string? systemPrompt = null,
	string? description = null,
	CancellationToken ct = default)
	{
		var entity = await _dbContext.Skills
			.Include(s => s.Category)
			.FirstOrDefaultAsync(s => s.Id == id, ct)
			?? throw new InvalidOperationException($"Skill {id} not found");

		if (skillCode is not null) entity.Code = skillCode;
		if (name is not null) entity.Name = name;
		if (systemPrompt is not null) entity.SystemPrompt = systemPrompt;
		if (description is not null) entity.Description = description;
		entity.UpdatedAt = DateTime.UtcNow;

		await _dbContext.SaveChangesAsync(ct);

		// Update Qdrant if description changed
		if (description is not null)
		{
			var record = new SkillRoutingRecord
			{
				Id = entity.Id,
				SkillCode = entity.Code,
				SkillName = entity.Name,
				CatCode = entity.Category.Code,
				CategoryName = entity.Category.Name,
				Description = description
			};

			await _skillRoutingRepo.UpsertAsync(record, ct);
		}

		return entity;
	}

	public async Task<SkillEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
	{
		return await _dbContext.Skills
			.Include(s => s.Category)
			.Include(s => s.Tools)
			.FirstOrDefaultAsync(s => s.Id == id, ct);
	}

	public async Task<CategoryEntity> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default)
	{
		var category = await _dbContext.Categories.Include(x=>x.Skills)
			.FirstOrDefaultAsync(c => c.Id == categoryId, ct)
			?? throw new InvalidOperationException($"Category {categoryId} not found");

		return category;
	}

	public async Task DeleteAsync(Guid id, CancellationToken ct = default)
	{
		var entity = await _dbContext.Skills
			.FirstOrDefaultAsync(s => s.Id == id, ct)
			?? throw new InvalidOperationException($"Skill {id} not found");

		// Delete from Qdrant first
		await _skillRoutingRepo.DeleteAsync(id, ct);

		// Delete from PostgreSQL
		_dbContext.Skills.Remove(entity);
		await _dbContext.SaveChangesAsync(ct);
	}

	public async Task<SkillEntity?> RouteAsync(string query, CancellationToken ct = default)
	{
		var results = await _skillRoutingRepo.SearchAsync(query, top: 1, 
			similarityThreshold: null,
			cancellationToken: ct);

		var topResult = results.FirstOrDefault();

		if (topResult is null)
			return null;

		return await _dbContext.Skills.FirstOrDefaultAsync(s => s.Code == topResult.SkillCode, ct);
	}
}