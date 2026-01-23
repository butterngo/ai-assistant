using Agent.Core.Entities;
using Microsoft.Extensions.AI;

namespace Agent.Core.Abstractions.Services;

/// <summary>
/// Service for managing the many-to-many relationship between Skills and ConnectionTools
/// </summary>
public interface ISkillConnectionToolService
{
	/// <summary>
	/// Add a connection tool to a skill
	/// </summary>
	Task<SkillConnectionToolEntity> CreateAsync(
		Guid skillId,
		Guid connectionToolId,
		CancellationToken ct = default);

	/// <summary>
	/// Check if a skill is using a connection tool
	/// </summary>
	Task<bool> ExistsAsync(
		Guid skillId,
		Guid connectionToolId,
		CancellationToken ct = default);

	/// <summary>
	/// Get all connection tools for a skill
	/// </summary>
	Task<IEnumerable<ConnectionToolEntity>> GetConnectionToolsBySkillAsync(
		Guid skillId,
		CancellationToken ct = default);

	/// <summary>
	/// Get all active connection tools for a skill
	/// </summary>
	Task<IEnumerable<ConnectionToolEntity>> GetActiveConnectionToolsBySkillAsync(
		Guid skillId,
		CancellationToken ct = default);

	/// <summary>
	/// Get all skills using a connection tool
	/// </summary>
	Task<IEnumerable<SkillEntity>> GetSkillsByConnectionToolAsync(
		Guid connectionToolId,
		CancellationToken ct = default);

	/// <summary>
	/// Remove a connection tool from a skill
	/// </summary>
	Task DeleteAsync(
		Guid skillId,
		Guid connectionToolId,
		CancellationToken ct = default);

	/// <summary>
	/// Remove all connection tools from a skill
	/// </summary>
	Task DeleteBySkillAsync(Guid skillId, CancellationToken ct = default);

	/// <summary>
	/// Remove a connection tool from all skills
	/// </summary>
	Task DeleteByConnectionToolAsync(Guid connectionToolId, CancellationToken ct = default);

	/// <summary>
	/// Get all AI tools for a skill (aggregated from all its active connections)
	/// </summary>
	Task<IEnumerable<AITool>> GetToolsForSkillAsync(
		Guid skillId,
		bool useCache = true,
		CancellationToken ct = default);

	/// <summary>
	/// Bulk add multiple connection tools to a skill
	/// </summary>
	Task<IEnumerable<SkillConnectionToolEntity>> BulkCreateAsync(
		Guid skillId,
		IEnumerable<Guid> connectionToolIds,
		CancellationToken ct = default);

	/// <summary>
	/// Sync connection tools for a skill (add new, remove old)
	/// </summary>
	Task SyncConnectionToolsAsync(
		Guid skillId,
		IEnumerable<Guid> connectionToolIds,
		CancellationToken ct = default);
}