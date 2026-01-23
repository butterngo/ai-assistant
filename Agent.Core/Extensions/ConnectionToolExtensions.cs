using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Agent.Core.Entities;
using Agent.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace Agent.Core.Extensions;

public static class ConnectionToolExtensions
{
	/// <summary>
	/// Convert ConnectionToolEntity to ConnectionTool model
	/// </summary>
	public static ConnectionTool ToConnectionTool(this ConnectionToolEntity entity)
	{
		var config = entity.Config != null
			? JsonSerializer.Deserialize<ToolConfig>(entity.Config.RootElement.GetRawText())
			: new ToolConfig();

		var toolType = entity.Type.ToLower() switch
		{
			"mcp_http" => ConnectionToolType.MCP_HTTP,
			"mcp_stdio" => ConnectionToolType.MCP_STDIO,
			"openapi" => ConnectionToolType.OpenApi,
			_ => throw new NotSupportedException($"Tool type '{entity.Type}' is not supported")
		};

		return new ConnectionTool
		{
			PluginName = entity.Name,
			ToolType = toolType,
			Endpoint = entity.Endpoint,
			Command = entity.Command ?? config?.Command,
			Arguments = config?.Arguments,
			EnvironmentVariables = config?.EnvironmentVariables,
			WorkingDirectory = config?.WorkingDirectory,
			ShutdownTimeout = config?.ShutdownTimeout != null
				? TimeSpan.FromSeconds(config.ShutdownTimeout.Value)
				: TimeSpan.FromSeconds(5)
		};
	}

	/// <summary>
	/// Get all AI tools for a skill (aggregated from all its connections)
	/// </summary>
	public static async Task<List<AITool>> GetToolsForSkillAsync(
		this DbContext context,
		Guid skillId,
		CancellationToken ct = default)
	{
		var connectionTools = await context.Set<SkillConnectionToolEntity>()
			.Where(sct => sct.SkillId == skillId)
			.Include(sct => sct.ConnectionTool)
			.Where(sct => sct.ConnectionTool.IsActive)
			.Select(sct => sct.ConnectionTool)
			.ToListAsync(ct);

		var allTools = new List<AITool>();

		foreach (var entity in connectionTools)
		{
			var connection = entity.ToConnectionTool();
			var tools = await connection.GetToolsAsync(ct);
			if (tools != null && tools.Any())
			{
				allTools.AddRange(tools);
			}
		}

		return allTools;
	}
}