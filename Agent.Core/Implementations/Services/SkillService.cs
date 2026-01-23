using Agent.Core.Abstractions.Persistents;
using Agent.Core.Abstractions.Services;
using Agent.Core.Entities;
using Agent.Core.Implementations.Persistents;
using Agent.Core.VectorRecords;
using Microsoft.EntityFrameworkCore;

namespace Agent.Core.Implementations.Services;

public class SkillService : ISkillService
{
	private readonly AgentDbContext _dbContext;
	private readonly IQdrantRepository<SkillRoutingRecord> _skillRoutingRepo;

	public SkillService(
		IDbContextFactory<AgentDbContext> dbContextFactory,
		IQdrantRepository<SkillRoutingRecord> skillRoutingRepo)
	{
		_dbContext = dbContextFactory.CreateDbContext();
		_skillRoutingRepo = skillRoutingRepo;
	}

	public async Task<SkillEntity> CreateAsync(
		Guid agentId,
		string skillCode,
		string name,
		string systemPrompt,
		CancellationToken ct = default)
	{
		var agent = await _dbContext.Agents
			.FirstOrDefaultAsync(c => c.Id == agentId, ct)
			?? throw new InvalidOperationException($"Agent {agentId} not found");

		var entity = new SkillEntity
		{
			Id = Guid.NewGuid(),
			AgentId = agentId,
			Code = skillCode,
			Name = name,
			SystemPrompt = systemPrompt,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		_dbContext.Skills.Add(entity);
		await _dbContext.SaveChangesAsync(ct);

		return entity;
	}

	public async Task<SkillEntity> UpdateAsync(
	Guid id,
	string? skillCode = null,
	string? name = null,
	string? systemPrompt = null,
	CancellationToken ct = default)
	{
		var entity = await _dbContext.Skills
			.FirstOrDefaultAsync(s => s.Id == id, ct)
			?? throw new InvalidOperationException($"Skill {id} not found");

		if (skillCode is not null) entity.Code = skillCode;
		if (name is not null) entity.Name = name;
		if (systemPrompt is not null) entity.SystemPrompt = systemPrompt;

		entity.UpdatedAt = DateTime.UtcNow;

		_dbContext.Skills.Update(entity);

		await _dbContext.SaveChangesAsync(ct);

		return entity;
	}

	public async Task<SkillEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
	{
		return await _dbContext.Skills
			.Include(s => s.Agent)
			.Include(s => s.Tools)
			.FirstOrDefaultAsync(s => s.Id == id, ct);
	}

	public async Task<AgentEntity> GetByAgentAsync(Guid agentId, CancellationToken ct = default)
	{
		var agent = await _dbContext.Agents.Include(x=>x.Skills)
			.FirstOrDefaultAsync(c => c.Id == agentId, ct)
			?? throw new InvalidOperationException($"Agent {agentId} not found");

		return agent;
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