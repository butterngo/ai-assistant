using Agent.Core.Entities;

namespace Agent.Core.Abstractions.Services;

public interface ISkillService
{
	Task<SkillEntity> CreateAsync(Guid agentId,
		string skillCode,
		string name,
		string systemPrompt,
		CancellationToken ct = default);

	Task<SkillEntity> UpdateAsync(Guid id,
		string? skillCode = null,
		string? name = null,
		string? systemPrompt = null,
		CancellationToken ct = default);

	Task<SkillEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
	Task<AgentEntity> GetByAgentAsync(Guid agentId, CancellationToken ct = default);
	Task DeleteAsync(Guid id, CancellationToken ct = default);
	Task<SkillEntity?> RouteAsync(string query, CancellationToken ct = default);
}
