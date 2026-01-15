using System.Text.Json;
using Agent.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Agent.Core.Abstractions.Services;
using Agent.Core.Implementations.Persistents;

namespace Agent.Core.Implementations.Services;

public class ToolService : IToolService
{
	private readonly ChatDbContext _dbContext;

	public ToolService(IDbContextFactory<ChatDbContext> dbContextFactory)
	{
		_dbContext = dbContextFactory.CreateDbContext();
	}

	public async Task<ToolEntity> CreateAsync(
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

	public async Task<ToolEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
	{
		return await _dbContext.Tools
			.Include(t => t.Skill)
			.FirstOrDefaultAsync(t => t.Id == id, ct);
	}

	public async Task<IEnumerable<ToolEntity>> GetBySkillAsync(Guid skillId, CancellationToken ct = default)
	{
		return await _dbContext.Tools
			.Where(t => t.SkillId == skillId)
			.OrderBy(t => t.Name)
			.ToListAsync(ct);
	}

	public async Task<ToolEntity> UpdateAsync(
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

	public async Task DeleteAsync(Guid id, CancellationToken ct = default)
	{
		var entity = await _dbContext.Tools
			.FirstOrDefaultAsync(t => t.Id == id, ct)
			?? throw new InvalidOperationException($"Tool {id} not found");

		_dbContext.Tools.Remove(entity);
		await _dbContext.SaveChangesAsync(ct);
	}
}
