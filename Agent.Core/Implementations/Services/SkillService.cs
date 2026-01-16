using Agent.Core.Abstractions.Persistents;
using Agent.Core.Abstractions.Services;
using Agent.Core.Entities;
using Agent.Core.Implementations.Persistents;
using Agent.Core.VectorRecords;
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
			SkillCode = skillCode,
			Name = name,
			SystemPrompt = systemPrompt,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		_dbContext.Skills.Add(entity);
		await _dbContext.SaveChangesAsync(ct);

		// Insert to Qdrant
		var record = new SkillRoutingRecord
		{
			SkillCode = entity.SkillCode,
			SkillName = entity.Name,
			CatCode = category.CatCode,
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

		if (skillCode is not null) entity.SkillCode = skillCode;
		if (name is not null) entity.Name = name;
		if (systemPrompt is not null) entity.SystemPrompt = systemPrompt;
		entity.UpdatedAt = DateTime.UtcNow;

		await _dbContext.SaveChangesAsync(ct);

		// Update Qdrant if description changed
		if (description is not null)
		{
			var record = new SkillRoutingRecord
			{
				SkillCode = entity.SkillCode,
				SkillName = entity.Name,
				CatCode = entity.Category.CatCode,
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

	public async Task<IEnumerable<SkillEntity>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default)
	{
		return await _dbContext.Skills
			.Include(s => s.Tools)
			.Where(s => s.CategoryId == categoryId)
			.OrderBy(s => s.Name)
			.ToListAsync();
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
		var results = await _skillRoutingRepo.SearchAsync(query, top: 1, cancellationToken: ct);
		var topResult = results.FirstOrDefault();

		if (topResult is null)
			return null;

		return await _dbContext.Skills.FirstOrDefaultAsync(s => s.SkillCode == topResult.SkillCode, ct);
	}
}