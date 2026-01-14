using Agent.Core.Entities;
using System.Text.Json;

namespace Agent.Core.Abstractions;

public interface ISkillService
{
	Task<CategoryEntity> CreateCategoryAsync(string name, string? description = null, CancellationToken ct = default);
	Task<CategoryEntity?> GetCategoryByIdAsync(Guid id, CancellationToken ct = default);
	Task<IEnumerable<CategoryEntity>> GetAllCategoriesAsync(CancellationToken ct = default);
	Task<CategoryEntity> UpdateCategoryAsync(Guid id, string? name = null, string? description = null, CancellationToken ct = default);
	Task DeleteCategoryAsync(Guid id, CancellationToken ct = default);

	// Skill
	Task<SkillEntity> CreateSkillAsync(Guid categoryId, string name, string systemPrompt, string description, CancellationToken ct = default);
	Task<SkillEntity?> GetSkillByIdAsync(Guid id, CancellationToken ct = default);
	Task<IEnumerable<SkillEntity>> GetSkillsByCategoryAsync(Guid categoryId, CancellationToken ct = default);
	Task<SkillEntity> UpdateSkillAsync(Guid id, string? name = null, string? systemPrompt = null, string? description = null, CancellationToken ct = default);
	Task DeleteSkillAsync(Guid id, CancellationToken ct = default);

	// Tool
	Task<ToolEntity> CreateToolAsync(Guid skillId, string name, string type, string endpoint, string? description = null, JsonDocument? config = null, bool isPrefetch = false, CancellationToken ct = default);
	Task<ToolEntity?> GetToolByIdAsync(Guid id, CancellationToken ct = default);
	Task<IEnumerable<ToolEntity>> GetToolsBySkillAsync(Guid skillId, CancellationToken ct = default);
	Task<ToolEntity> UpdateToolAsync(Guid id, string? name = null, string? type = null, string? endpoint = null, string? description = null, JsonDocument? config = null, bool? isPrefetch = null, CancellationToken ct = default);
	Task DeleteToolAsync(Guid id, CancellationToken ct = default);

	// Routing (Qdrant search)
	Task<SkillEntity?> RouteToSkillAsync(string query, CancellationToken ct = default);
}
