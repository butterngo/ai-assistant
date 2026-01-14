using Agent.Core.Abstractions;
using Agent.Core.Abstractions.Persistents;
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
		ChatDbContext dbContext,
		IQdrantRepository<SkillRoutingRecord> skillRoutingRepo)
	{
		_dbContext = dbContext;
		_skillRoutingRepo = skillRoutingRepo;
	}

	#region Category

	public async Task<CategoryEntity> CreateCategoryAsync(
		string name,
		string? description = null,
		CancellationToken ct = default)
	{
		var entity = new CategoryEntity
		{
			Id = Guid.NewGuid(),
			Name = name,
			Description = description,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		_dbContext.Categories.Add(entity);
		await _dbContext.SaveChangesAsync(ct);

		return entity;
	}

	public async Task<CategoryEntity?> GetCategoryByIdAsync(Guid id, CancellationToken ct = default)
	{
		return await _dbContext.Categories
			.FirstOrDefaultAsync(c => c.Id == id, ct);
	}

	public async Task<IEnumerable<CategoryEntity>> GetAllCategoriesAsync(CancellationToken ct = default)
	{
		return await _dbContext.Categories
			.OrderBy(c => c.Name)
			.ToListAsync(ct);
	}

	public async Task<CategoryEntity> UpdateCategoryAsync(
		Guid id,
		string? name = null,
		string? description = null,
		CancellationToken ct = default)
	{
		var entity = await _dbContext.Categories
			.FirstOrDefaultAsync(c => c.Id == id, ct)
			?? throw new InvalidOperationException($"Category {id} not found");

		if (name is not null) entity.Name = name;
		if (description is not null) entity.Description = description;
		entity.UpdatedAt = DateTime.UtcNow;

		await _dbContext.SaveChangesAsync(ct);

		return entity;
	}

	public async Task DeleteCategoryAsync(Guid id, CancellationToken ct = default)
	{
		var entity = await _dbContext.Categories
			.FirstOrDefaultAsync(c => c.Id == id, ct)
			?? throw new InvalidOperationException($"Category {id} not found");

		_dbContext.Categories.Remove(entity);
		await _dbContext.SaveChangesAsync(ct);
	}

	#endregion

	#region Skill

	public async Task<SkillEntity> CreateSkillAsync(
		Guid categoryId,
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
			Name = name,
			SystemPrompt = systemPrompt,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		// Insert to PostgreSQL
		_dbContext.Skills.Add(entity);
		await _dbContext.SaveChangesAsync(ct);

		// Insert to Qdrant
		var record = new SkillRoutingRecord
		{
			SkillId = entity.Id,
			SkillName = entity.Name,
			CategoryId = category.Id,
			CategoryName = category.Name,
			Description = description
		};

		await _skillRoutingRepo.UpsertAsync(record, ct);

		return entity;
	}

	public async Task<SkillEntity?> GetSkillByIdAsync(Guid id, CancellationToken ct = default)
	{
		return await _dbContext.Skills
			.Include(s => s.Category)
			.Include(s => s.Tools)
			.FirstOrDefaultAsync(s => s.Id == id, ct);
	}

	public async Task<IEnumerable<SkillEntity>> GetSkillsByCategoryAsync(Guid categoryId, CancellationToken ct = default)
	{
		return await _dbContext.Skills
			.Include(s => s.Tools)
			.Where(s => s.CategoryId == categoryId)
			.OrderBy(s => s.Name)
			.ToListAsync(ct);
	}

	public async Task<SkillEntity> UpdateSkillAsync(
		Guid id,
		string? name = null,
		string? systemPrompt = null,
		string? description = null,
		CancellationToken ct = default)
	{
		var entity = await _dbContext.Skills
			.Include(s => s.Category)
			.FirstOrDefaultAsync(s => s.Id == id, ct)
			?? throw new InvalidOperationException($"Skill {id} not found");

		if (name is not null) entity.Name = name;
		if (systemPrompt is not null) entity.SystemPrompt = systemPrompt;
		entity.UpdatedAt = DateTime.UtcNow;

		await _dbContext.SaveChangesAsync(ct);

		// Update Qdrant if description changed
		if (description is not null)
		{
			var record = new SkillRoutingRecord
			{
				SkillId = entity.Id,
				SkillName = entity.Name,
				CategoryId = entity.CategoryId,
				CategoryName = entity.Category.Name,
				Description = description
			};

			await _skillRoutingRepo.UpsertAsync(record, ct);
		}

		return entity;
	}

	public async Task DeleteSkillAsync(Guid id, CancellationToken ct = default)
	{
		var entity = await _dbContext.Skills
			.FirstOrDefaultAsync(s => s.Id == id, ct)
			?? throw new InvalidOperationException($"Skill {id} not found");

		// Delete from Qdrant first
		await _skillRoutingRepo.DeleteAsync(id, ct);

		// Delete from PostgreSQL (cascade deletes tools)
		_dbContext.Skills.Remove(entity);
		await _dbContext.SaveChangesAsync(ct);
	}

	#endregion

	#region Tool

	public async Task<ToolEntity> CreateToolAsync(
		Guid skillId,
		string name,
		string type,
		string endpoint,
		string? description = null,
		JsonDocument? config = null,
		bool isPrefetch = false,
		CancellationToken ct = default)
	{
		var skillExists = await _dbContext.Skills.AnyAsync(s => s.Id == skillId, ct);
		if (!skillExists)
			throw new InvalidOperationException($"Skill {skillId} not found");

		var entity = new ToolEntity
		{
			Id = Guid.NewGuid(),
			SkillId = skillId,
			Name = name,
			Type = type,
			Endpoint = endpoint,
			Description = description,
			Config = config,
			IsPrefetch = isPrefetch,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		_dbContext.Tools.Add(entity);
		await _dbContext.SaveChangesAsync(ct);

		return entity;
	}

	public async Task<ToolEntity?> GetToolByIdAsync(Guid id, CancellationToken ct = default)
	{
		return await _dbContext.Tools
			.Include(t => t.Skill)
			.FirstOrDefaultAsync(t => t.Id == id, ct);
	}

	public async Task<IEnumerable<ToolEntity>> GetToolsBySkillAsync(Guid skillId, CancellationToken ct = default)
	{
		return await _dbContext.Tools
			.Where(t => t.SkillId == skillId)
			.OrderBy(t => t.Name)
			.ToListAsync(ct);
	}

	public async Task<ToolEntity> UpdateToolAsync(
		Guid id,
		string? name = null,
		string? type = null,
		string? endpoint = null,
		string? description = null,
		JsonDocument? config = null,
		bool? isPrefetch = null,
		CancellationToken ct = default)
	{
		var entity = await _dbContext.Tools
			.FirstOrDefaultAsync(t => t.Id == id, ct)
			?? throw new InvalidOperationException($"Tool {id} not found");

		if (name is not null) entity.Name = name;
		if (type is not null) entity.Type = type;
		if (endpoint is not null) entity.Endpoint = endpoint;
		if (description is not null) entity.Description = description;
		if (config is not null) entity.Config = config;
		if (isPrefetch.HasValue) entity.IsPrefetch = isPrefetch.Value;
		entity.UpdatedAt = DateTime.UtcNow;

		await _dbContext.SaveChangesAsync(ct);

		return entity;
	}

	public async Task DeleteToolAsync(Guid id, CancellationToken ct = default)
	{
		var entity = await _dbContext.Tools
			.FirstOrDefaultAsync(t => t.Id == id, ct)
			?? throw new InvalidOperationException($"Tool {id} not found");

		_dbContext.Tools.Remove(entity);
		await _dbContext.SaveChangesAsync(ct);
	}

	#endregion

	#region Routing

	public async Task<SkillEntity?> RouteToSkillAsync(string query, CancellationToken ct = default)
	{
		var results = await _skillRoutingRepo.SearchAsync(query, top: 1, cancellationToken: ct);
		var topResult = results.FirstOrDefault();

		if (topResult is null)
			return null;

		return await GetSkillByIdAsync(topResult.SkillId, ct);
	}

	#endregion
}
