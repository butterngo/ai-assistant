using Agent.Api.Models;
using Agent.Core.Abstractions.Services;
using Agent.Core.Entities;

namespace Agent.Api.Endpoints;

public static class DiscoveredToolEndpoint
{
	public static IEndpointRouteBuilder MapDiscoveredToolEndpoints(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/api/discovered-tools")
			.WithTags("Discovered Tools");

		group.MapGet("/{id:guid}", GetByIdAsync)
			.WithName("GetDiscoveredToolById")
			.WithSummary("Get discovered tool by ID")
			.Produces<DiscoveredToolEntity>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapGet("/by-connection/{connectionToolId:guid}", GetByConnectionAsync)
			.WithName("GetDiscoveredToolsByConnection")
			.WithSummary("Get all discovered tools for a connection")
			.Produces<IEnumerable<DiscoveredToolEntity>>(StatusCodes.Status200OK);

		group.MapGet("/by-connection/{connectionToolId:guid}/available", GetAvailableByConnectionAsync)
			.WithName("GetAvailableDiscoveredToolsByConnection")
			.WithSummary("Get available discovered tools for a connection")
			.Produces<IEnumerable<DiscoveredToolEntity>>(StatusCodes.Status200OK);

		group.MapGet("/by-connection/{connectionToolId:guid}/by-name/{name}", GetByNameAsync)
			.WithName("GetDiscoveredToolByName")
			.WithSummary("Get discovered tool by connection and name")
			.Produces<DiscoveredToolEntity>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapPut("/{id:guid}", UpdateAsync)
			.WithName("UpdateDiscoveredTool")
			.WithSummary("Update a discovered tool")
			.Produces<DiscoveredToolEntity>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapDelete("/{id:guid}", DeleteAsync)
			.WithName("DeleteDiscoveredTool")
			.WithSummary("Delete a discovered tool")
			.Produces(StatusCodes.Status204NoContent)
			.Produces(StatusCodes.Status404NotFound);

		group.MapPost("/by-connection/{connectionToolId:guid}/clear-cache", ClearCacheAsync)
			.WithName("ClearDiscoveredToolCache")
			.WithSummary("Clear all cached tools for a connection")
			.Produces(StatusCodes.Status204NoContent);

		group.MapPut("/by-connection/{connectionToolId:guid}/by-name/{toolName}/mark-unavailable", MarkAsUnavailableAsync)
			.WithName("MarkToolAsUnavailable")
			.WithSummary("Mark a tool as unavailable")
			.Produces(StatusCodes.Status204NoContent);

		group.MapPut("/by-connection/{connectionToolId:guid}/by-name/{toolName}/mark-available", MarkAsAvailableAsync)
			.WithName("MarkToolAsAvailable")
			.WithSummary("Mark a tool as available")
			.Produces(StatusCodes.Status204NoContent);

		group.MapGet("/by-connection/{connectionToolId:guid}/cache-status", CheckCacheStatusAsync)
			.WithName("CheckCacheStatus")
			.WithSummary("Check if cache is stale")
			.Produces<CacheStatusResponse>(StatusCodes.Status200OK);

		return endpoints;
	}

	private static async Task<IResult> GetByIdAsync(
		Guid id,
		IDiscoveredToolService service,
		CancellationToken ct)
	{
		var result = await service.GetByIdAsync(id, ct);
		return result is null ? Results.NotFound() : Results.Ok(result);
	}

	private static async Task<IResult> GetByConnectionAsync(
		Guid connectionToolId,
		IDiscoveredToolService service,
		CancellationToken ct)
	{
		var result = await service.GetByConnectionAsync(connectionToolId, ct);
		return Results.Ok(result);
	}

	private static async Task<IResult> GetAvailableByConnectionAsync(
		Guid connectionToolId,
		IDiscoveredToolService service,
		CancellationToken ct)
	{
		var result = await service.GetAvailableByConnectionAsync(connectionToolId, ct);
		return Results.Ok(result);
	}

	private static async Task<IResult> GetByNameAsync(
		Guid connectionToolId,
		string name,
		IDiscoveredToolService service,
		CancellationToken ct)
	{
		var result = await service.GetByNameAsync(connectionToolId, name, ct);
		return result is null ? Results.NotFound() : Results.Ok(result);
	}

	private static async Task<IResult> UpdateAsync(
		Guid id,
		UpdateDiscoveredToolRequest request,
		IDiscoveredToolService service,
		CancellationToken ct)
	{
		try
		{
			var result = await service.UpdateAsync(
				id,
				request.Name,
				request.Description,
				request.ToolSchema,
				request.IsAvailable,
				ct);

			return Results.Ok(result);
		}
		catch (KeyNotFoundException ex)
		{
			return Results.NotFound(new { error = ex.Message });
		}
	}

	private static async Task<IResult> DeleteAsync(
		Guid id,
		IDiscoveredToolService service,
		CancellationToken ct)
	{
		try
		{
			await service.DeleteAsync(id, ct);
			return Results.NoContent();
		}
		catch (KeyNotFoundException ex)
		{
			return Results.NotFound(new { error = ex.Message });
		}
	}

	private static async Task<IResult> ClearCacheAsync(
		Guid connectionToolId,
		IDiscoveredToolService service,
		CancellationToken ct)
	{
		await service.ClearCacheAsync(connectionToolId, ct);
		return Results.NoContent();
	}

	private static async Task<IResult> MarkAsUnavailableAsync(
		Guid connectionToolId,
		string toolName,
		IDiscoveredToolService service,
		CancellationToken ct)
	{
		await service.MarkAsUnavailableAsync(connectionToolId, toolName, ct);
		return Results.NoContent();
	}

	private static async Task<IResult> MarkAsAvailableAsync(
		Guid connectionToolId,
		string toolName,
		IDiscoveredToolService service,
		CancellationToken ct)
	{
		await service.MarkAsAvailableAsync(connectionToolId, toolName, ct);
		return Results.NoContent();
	}

	private static async Task<IResult> CheckCacheStatusAsync(
		Guid connectionToolId,
		IDiscoveredToolService service,
		CancellationToken ct)
	{
		var maxAge = TimeSpan.FromHours(24);
		var isStale = await service.IsCacheStaleAsync(connectionToolId, maxAge, ct);

		return Results.Ok(new CacheStatusResponse
		{
			IsStale = isStale,
			MaxAge = maxAge,
			Message = isStale ? "Cache is stale, refresh recommended" : "Cache is fresh"
		});
	}
}