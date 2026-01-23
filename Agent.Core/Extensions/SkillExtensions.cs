using Agent.Core.Entities;
using Microsoft.Extensions.AI;
using Agent.Core.Abstractions.Services;

namespace Agent.Core.Extensions;

public static class SkillExtensions
{
	/// <summary>
	/// Get all AI tools for this skill (convenience method)
	/// </summary>
	public static async Task<List<AITool>> GetToolsAsync(
		this SkillEntity skill,
		ISkillConnectionToolService skillConnectionToolService,
		bool useCache = true,
		CancellationToken ct = default)
	{
		var tools = await skillConnectionToolService.GetToolsForSkillAsync(
			skill.Id,
			useCache,
			ct);

		return tools.ToList();
	}

	/// <summary>
	/// Check if skill has any connection tools
	/// </summary>
	public static async Task<bool> HasConnectionToolsAsync(
		this SkillEntity skill,
		ISkillConnectionToolService skillConnectionToolService,
		CancellationToken ct = default)
	{
		var connections = await skillConnectionToolService.GetConnectionToolsBySkillAsync(
			skill.Id,
			ct);

		return connections.Any();
	}
}