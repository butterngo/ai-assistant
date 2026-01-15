using Agent.Core.Entities;
using System.Text.Json;

namespace Agent.Core.Abstractions.Services;

public interface IToolService
{
	Task<ToolEntity> CreateAsync(Guid skillId, string name, string type, string endpoint, string? description = null, JsonDocument? config = null, bool isPrefetch = false, CancellationToken ct = default);
	Task<ToolEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
	Task<IEnumerable<ToolEntity>> GetBySkillAsync(Guid skillId, CancellationToken ct = default);
	Task<ToolEntity> UpdateAsync(Guid id, string? name = null, string? type = null, string? endpoint = null, string? description = null, JsonDocument? config = null, bool? isPrefetch = null, CancellationToken ct = default);
	Task DeleteAsync(Guid id, CancellationToken ct = default);
}
