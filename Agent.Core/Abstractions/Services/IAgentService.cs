using Agent.Core.Entities;

namespace Agent.Core.Abstractions.Services;

public interface IAgentService
{
	Task<AgentEntity> CreateAsync(string catCode, string name, string? description = null, CancellationToken ct = default);
	Task<AgentEntity> UpdateAsync(Guid id, string? catCode, string? name = null, string? description = null, CancellationToken ct = default);
	Task<AgentEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
	Task<IEnumerable<AgentEntity>> GetAllAsync(CancellationToken ct = default);
	Task DeleteAsync(Guid id, CancellationToken ct = default);
}
