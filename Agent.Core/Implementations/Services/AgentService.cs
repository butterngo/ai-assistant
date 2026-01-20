using Agent.Core.Abstractions.Services;
using Agent.Core.Entities;
using Agent.Core.Implementations.Persistents;
using Microsoft.EntityFrameworkCore;

namespace Agent.Core.Implementations.Services;

public class AgentService : IAgentService
{
	private readonly ChatDbContext _dbContext;

	public AgentService(IDbContextFactory<ChatDbContext> dbContextFactory)
	{
		_dbContext = dbContextFactory.CreateDbContext();
	}

	public async Task<AgentEntity> CreateAsync(
		string catCode,
		string name,
		string? description = null,
		CancellationToken ct = default)
	{
		var entity = new AgentEntity
		{
			Id = Guid.NewGuid(),
			Code = catCode,
			Name = name,
			Description = description,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		_dbContext.Agents.Add(entity);
		await _dbContext.SaveChangesAsync(ct);

		return entity;
	}

	public async Task<AgentEntity> UpdateAsync(
		Guid id,
		string? catCode,
		string? name = null,
		string? description = null,
		CancellationToken ct = default)
	{
		var entity = await _dbContext.Agents
			.FirstOrDefaultAsync(c => c.Id == id, ct)
			?? throw new InvalidOperationException($"Agent {id} not found");

		if (catCode is not null) entity.Code = catCode;
		if (name is not null) entity.Name = name;
		if (description is not null) entity.Description = description;
		entity.UpdatedAt = DateTime.UtcNow;

		await _dbContext.SaveChangesAsync(ct);

		return entity;
	}

	public async Task<AgentEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
	{
		return await _dbContext.Agents
			.Include(x=>x.Skills)
			.FirstOrDefaultAsync(c => c.Id == id);
	}

	public async Task<IEnumerable<AgentEntity>> GetAllAsync(CancellationToken ct = default)
	{
		return await _dbContext.Agents
			.Include(x=>x.Skills)
			.OrderBy(c => c.CreatedAt)
			.ToListAsync(ct);
	}

	public async Task DeleteAsync(Guid id, CancellationToken ct = default)
	{
		var entity = await _dbContext.Agents
			.FirstOrDefaultAsync(c => c.Id == id, ct)
			?? throw new InvalidOperationException($"Agent {id} not found");

		_dbContext.Agents.Remove(entity);
		await _dbContext.SaveChangesAsync(ct);
	}
}
