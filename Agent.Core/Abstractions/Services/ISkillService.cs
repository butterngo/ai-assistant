using Agent.Core.Entities;

namespace Agent.Core.Abstractions.Services;

public interface ISkillService
{
	Task<SkillEntity> CreateAsync(Guid categoryId, string name, string systemPrompt, string description, CancellationToken ct = default);
	Task<SkillEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
	Task<IEnumerable<SkillEntity>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);
	Task<SkillEntity> UpdateAsync(Guid id, string? name = null, string? systemPrompt = null, string? description = null, CancellationToken ct = default);
	Task DeleteAsync(Guid id, CancellationToken ct = default);
	Task<SkillEntity?> RouteAsync(string query, CancellationToken ct = default);
}
