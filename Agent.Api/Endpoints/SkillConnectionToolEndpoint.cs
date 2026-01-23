using Agent.Api.Models;
using Agent.Core.Abstractions.Services;
using Agent.Core.Entities;

namespace Agent.Api.Endpoints;

public static class SkillConnectionToolEndpoint
{
	public static IEndpointRouteBuilder MapSkillConnectionToolEndpoints(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/api/skill-connection-tools")
			.WithTags("Skill Connection Tools");

		// Link/Unlink Operations
		group.MapPost("/", CreateAsync)
			.WithName("LinkConnectionToSkill")
			.WithSummary("Link a connection tool to a skill")
			.Produces<SkillConnectionToolEntity>(StatusCodes.Status201Created)
			.Produces(StatusCodes.Status400BadRequest);

		group.MapDelete("/", DeleteAsync)
			.WithName("UnlinkConnectionFromSkill")
			.WithSummary("Unlink a connection tool from a skill")
			.Produces(StatusCodes.Status204NoContent)
			.Produces(StatusCodes.Status404NotFound);

		group.MapDelete("/by-skill/{skillId:guid}", DeleteBySkillAsync)
			.WithName("UnlinkAllConnectionsFromSkill")
			.WithSummary("Unlink all connection tools from a skill")
			.Produces(StatusCodes.Status204NoContent);

		group.MapDelete("/by-connection/{connectionToolId:guid}", DeleteByConnectionToolAsync)
			.WithName("UnlinkConnectionFromAllSkills")
			.WithSummary("Unlink a connection tool from all skills")
			.Produces(StatusCodes.Status204NoContent);

		// Query Operations
		group.MapGet("/by-skill/{skillId:guid}/connections", GetConnectionsBySkillAsync)
			.WithName("GetConnectionToolsBySkill")
			.WithSummary("Get all connection tools for a skill")
			.Produces<IEnumerable<ConnectionToolEntity>>(StatusCodes.Status200OK);

		group.MapGet("/by-skill/{skillId:guid}/connections/active", GetActiveConnectionsBySkillAsync)
			.WithName("GetActiveConnectionToolsBySkill")
			.WithSummary("Get active connection tools for a skill")
			.Produces<IEnumerable<ConnectionToolEntity>>(StatusCodes.Status200OK);

		group.MapGet("/by-connection/{connectionToolId:guid}/skills", GetSkillsByConnectionAsync)
			.WithName("GetSkillsByConnectionTool")
			.WithSummary("Get all skills using a connection tool")
			.Produces<IEnumerable<SkillEntity>>(StatusCodes.Status200OK);

		group.MapGet("/exists", ExistsAsync)
			.WithName("CheckConnectionSkillLink")
			.WithSummary("Check if a skill is using a connection tool")
			.Produces<ExistsResponse>(StatusCodes.Status200OK);

		// Tool Aggregation
		group.MapGet("/by-skill/{skillId:guid}/tools", GetToolsForSkillAsync)
			.WithName("GetToolsForSkill")
			.WithSummary("Get all AI tools for a skill (aggregated from connections)")
			.Produces<IEnumerable<object>>(StatusCodes.Status200OK);

		// Bulk Operations
		group.MapPost("/by-skill/{skillId:guid}/bulk", BulkCreateAsync)
			.WithName("BulkLinkConnectionsToSkill")
			.WithSummary("Link multiple connection tools to a skill")
			.Produces<IEnumerable<SkillConnectionToolEntity>>(StatusCodes.Status201Created);

		group.MapPost("/by-skill/{skillId:guid}/sync", SyncConnectionToolsAsync)
			.WithName("SyncConnectionToolsForSkill")
			.WithSummary("Sync connection tools for a skill (add new, remove old)")
			.Produces(StatusCodes.Status204NoContent);

		return endpoints;
	}

	private static async Task<IResult> CreateAsync(
		LinkConnectionToolRequest request,
		ISkillConnectionToolService service,
		CancellationToken ct)
	{
		try
		{
			var result = await service.CreateAsync(
				request.SkillId,
				request.ConnectionToolId,
				ct);

			return Results.Created(
				$"/api/skill-connection-tools?skillId={request.SkillId}&connectionToolId={request.ConnectionToolId}",
				result);
		}
		catch (InvalidOperationException ex)
		{
			return Results.BadRequest(new { error = ex.Message });
		}
	}

	private static async Task<IResult> DeleteAsync(
		Guid skillId,
		Guid connectionToolId,
		ISkillConnectionToolService service,
		CancellationToken ct)
	{
		try
		{
			await service.DeleteAsync(skillId, connectionToolId, ct);
			return Results.NoContent();
		}
		catch (KeyNotFoundException ex)
		{
			return Results.NotFound(new { error = ex.Message });
		}
	}

	private static async Task<IResult> DeleteBySkillAsync(
		Guid skillId,
		ISkillConnectionToolService service,
		CancellationToken ct)
	{
		await service.DeleteBySkillAsync(skillId, ct);
		return Results.NoContent();
	}

	private static async Task<IResult> DeleteByConnectionToolAsync(
		Guid connectionToolId,
		ISkillConnectionToolService service,
		CancellationToken ct)
	{
		await service.DeleteByConnectionToolAsync(connectionToolId, ct);
		return Results.NoContent();
	}

	private static async Task<IResult> GetConnectionsBySkillAsync(
		Guid skillId,
		ISkillConnectionToolService service,
		CancellationToken ct)
	{
		var result = await service.GetConnectionToolsBySkillAsync(skillId, ct);
		return Results.Ok(result);
	}

	private static async Task<IResult> GetActiveConnectionsBySkillAsync(
		Guid skillId,
		ISkillConnectionToolService service,
		CancellationToken ct)
	{
		var result = await service.GetActiveConnectionToolsBySkillAsync(skillId, ct);
		return Results.Ok(result);
	}

	private static async Task<IResult> GetSkillsByConnectionAsync(
		Guid connectionToolId,
		ISkillConnectionToolService service,
		CancellationToken ct)
	{
		var result = await service.GetSkillsByConnectionToolAsync(connectionToolId, ct);
		return Results.Ok(result);
	}

	private static async Task<IResult> ExistsAsync(
		Guid skillId,
		Guid connectionToolId,
		ISkillConnectionToolService service,
		CancellationToken ct)
	{
		var exists = await service.ExistsAsync(skillId, connectionToolId, ct);

		return Results.Ok(new ExistsResponse
		{
			Exists = exists,
			SkillId = skillId,
			ConnectionToolId = connectionToolId
		});
	}

	private static async Task<IResult> GetToolsForSkillAsync(
		Guid skillId,
		bool useCache,
		ISkillConnectionToolService service,
		CancellationToken ct)
	{
		var tools = await service.GetToolsForSkillAsync(skillId, useCache, ct);

		// Convert AITool to simple object for JSON response
		var toolsResponse = tools.Select(t => new
		{
			name = t.Name,
			description = t.Description,
			additionalProperties = t.AdditionalProperties
		});

		return Results.Ok(toolsResponse);
	}

	private static async Task<IResult> BulkCreateAsync(
		Guid skillId,
		BulkLinkConnectionToolsRequest request,
		ISkillConnectionToolService service,
		CancellationToken ct)
	{
		var result = await service.BulkCreateAsync(skillId, request.ConnectionToolIds, ct);
		return Results.Created($"/api/skill-connection-tools/by-skill/{skillId}/connections", result);
	}

	private static async Task<IResult> SyncConnectionToolsAsync(
		Guid skillId,
		SyncConnectionToolsRequest request,
		ISkillConnectionToolService service,
		CancellationToken ct)
	{
		await service.SyncConnectionToolsAsync(skillId, request.ConnectionToolIds, ct);
		return Results.NoContent();
	}
}