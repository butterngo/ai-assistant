using Agent.Api.Models;
using Agent.Core.Abstractions.Services;
using Agent.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Agent.Api.Endpoints;

public static class ConnectionToolEndpoint
{
	public static IEndpointRouteBuilder MapConnectionToolEndpoints(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/api/connection-tools")
			.WithTags("Connection Tools");

		// CRUD Operations
		group.MapPost("/", CreateAsync)
			.WithName("CreateConnectionTool")
			.WithSummary("Create a new connection tool")
			.Produces<ConnectionToolEntity>(StatusCodes.Status201Created)
			.Produces(StatusCodes.Status400BadRequest);

		group.MapGet("/{id:guid}", GetByIdAsync)
			.WithName("GetConnectionToolById")
			.WithSummary("Get connection tool by ID")
			.Produces<ConnectionToolEntity>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapGet("/by-name/{name}", GetByNameAsync)
			.WithName("GetConnectionToolByName")
			.WithSummary("Get connection tool by name")
			.Produces<ConnectionToolEntity>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapGet("/", GetAllAsync)
			.WithName("GetAllConnectionTools")
			.WithSummary("Get all connection tools")
			.Produces<IEnumerable<ConnectionToolEntity>>(StatusCodes.Status200OK);

		group.MapGet("/active", GetActiveAsync)
			.WithName("GetActiveConnectionTools")
			.WithSummary("Get all active connection tools")
			.Produces<IEnumerable<ConnectionToolEntity>>(StatusCodes.Status200OK);

		group.MapGet("/by-type/{type}", GetByTypeAsync)
			.WithName("GetConnectionToolsByType")
			.WithSummary("Get connection tools by type")
			.Produces<IEnumerable<ConnectionToolEntity>>(StatusCodes.Status200OK);

		group.MapPut("/{id:guid}", UpdateAsync)
			.WithName("UpdateConnectionTool")
			.WithSummary("Update an existing connection tool")
			.Produces<ConnectionToolEntity>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapDelete("/{id:guid}", DeleteAsync)
			.WithName("DeleteConnectionTool")
			.WithSummary("Delete a connection tool")
			.Produces(StatusCodes.Status204NoContent)
			.Produces(StatusCodes.Status404NotFound);

		// Tool Discovery Operations
		group.MapPost("/{id:guid}/discover", DiscoverToolsAsync)
			.WithName("DiscoverTools")
			.WithSummary("Discover AI tools from a connection (fresh)")
			.Produces<IEnumerable<object>>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapGet("/{id:guid}/tools", GetToolsAsync)
			.WithName("GetConnectionTools")
			.WithSummary("Get AI tools for a connection (uses cache)")
			.Produces<IEnumerable<object>>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapPost("/{id:guid}/test", TestConnectionAsync)
			.WithName("TestConnection")
			.WithSummary("Test connection to verify it's working")
			.Produces<TestConnectionResponse>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		return endpoints;
	}

	private static async Task<IResult> CreateAsync(
		CreateConnectionToolRequest request,
		IConnectionToolService service,
		CancellationToken ct)
	{
		try
		{
			var result = await service.CreateAsync(
				request.Name,
				request.Type,
				request.Description,
				request.Endpoint,
				request.Command,
				request.Config,
				request.IsActive,
				ct);

			return Results.Created($"/api/connection-tools/{result.Id}", result);
		}
		catch (InvalidOperationException ex)
		{
			return Results.BadRequest(new { error = ex.Message });
		}
	}

	private static async Task<IResult> GetByIdAsync(
		Guid id,
		IConnectionToolService service,
		CancellationToken ct)
	{
		var result = await service.GetByIdAsync(id, ct);
		return result is null ? Results.NotFound() : Results.Ok(result);
	}

	private static async Task<IResult> GetByNameAsync(
		string name,
		IConnectionToolService service,
		CancellationToken ct)
	{
		var result = await service.GetByNameAsync(name, ct);
		return result is null ? Results.NotFound() : Results.Ok(result);
	}

	private static async Task<IResult> GetAllAsync(
		IConnectionToolService service,
		CancellationToken ct)
	{
		var result = await service.GetAllAsync(ct);
		return Results.Ok(result);
	}

	private static async Task<IResult> GetActiveAsync(
		IConnectionToolService service,
		CancellationToken ct)
	{
		var result = await service.GetActiveAsync(ct);
		return Results.Ok(result);
	}

	private static async Task<IResult> GetByTypeAsync(
		string type,
		IConnectionToolService service,
		CancellationToken ct)
	{
		var result = await service.GetByTypeAsync(type, ct);
		return Results.Ok(result);
	}

	private static async Task<IResult> UpdateAsync(
		Guid id,
		UpdateConnectionToolRequest request,
		IConnectionToolService service,
		CancellationToken ct)
	{
		try
		{
			var result = await service.UpdateAsync(
				id,
				request.Name,
				request.Type,
				request.Description,
				request.Endpoint,
				request.Command,
				request.Config,
				request.IsActive,
				ct);

			return Results.Ok(result);
		}
		catch (KeyNotFoundException ex)
		{
			return Results.NotFound(new { error = ex.Message });
		}
		catch (InvalidOperationException ex)
		{
			return Results.BadRequest(new { error = ex.Message });
		}
	}

	private static async Task<IResult> DeleteAsync(
		Guid id,
		IConnectionToolService service,
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

	private static async Task<IResult> DiscoverToolsAsync(
		Guid id,
		IConnectionToolService connectionToolService,
		IDiscoveredToolService discoveredToolService,
		CancellationToken ct)
	{
		try
		{
			var tools = await connectionToolService.DiscoverToolsAsync(id, ct);

			await discoveredToolService.SaveDiscoveredToolsAsync(id, tools, ct);

			// Convert AITool to simple object for JSON response
			var toolsResponse = tools.Select(t => new
			{
				name = t.Name,
				description = t.Description,
				additionalProperties = t.AdditionalProperties
			});

			return Results.Ok(toolsResponse);
		}
		catch (KeyNotFoundException ex)
		{
			return Results.NotFound(new { error = ex.Message });
		}
	}

	private static async Task<IResult> GetToolsAsync(
		Guid id,
		[FromQuery] bool useCache,
		IConnectionToolService service,
		CancellationToken ct)
	{
		try
		{
			var tools = await service.GetToolsAsync(id, useCache, ct);

			// Convert AITool to simple object for JSON response
			var toolsResponse = tools.Select(t => new
			{
				name = t.Name,
				description = t.Description,
				additionalProperties = t.AdditionalProperties
			});

			return Results.Ok(toolsResponse);
		}
		catch (KeyNotFoundException ex)
		{
			return Results.NotFound(new { error = ex.Message });
		}
	}

	private static async Task<IResult> TestConnectionAsync(
		Guid id,
		IConnectionToolService service,
		CancellationToken ct)
	{
		try
		{
			var isConnected = await service.TestConnectionAsync(id, ct);

			return Results.Ok(new TestConnectionResponse
			{
				IsConnected = isConnected,
				Message = isConnected ? "Connection successful" : "Connection failed"
			});
		}
		catch (KeyNotFoundException ex)
		{
			return Results.NotFound(new { error = ex.Message });
		}
	}
}