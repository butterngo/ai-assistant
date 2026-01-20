using Agent.Api.Models;
using Agent.Core.Abstractions.Services;
using Agent.Core.Entities;

namespace Agent.Api.Endpoints;

public static class AgentEndPoint
{
	public static IEndpointRouteBuilder MapAgentEndPoints(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/api/agents")
			.WithTags("Agents");

		group.MapPost("/", CreateAsync)
			.WithName("CreateAgent")
			.WithSummary("Create a new agent")
			.Produces<AgentEntity>(StatusCodes.Status201Created)
			.Produces(StatusCodes.Status400BadRequest);

		group.MapGet("/", GetAllAsync)
			.WithName("GetAllAgents")
			.WithSummary("Get all agents")
			.Produces<IEnumerable<AgentEntity>>(StatusCodes.Status200OK);

		group.MapGet("/{id:guid}", GetByIdAsync)
			.WithName("GetAgentById")
			.WithSummary("Get agent by ID")
			.Produces<AgentEntity>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapPut("/{id:guid}", UpdateAsync)
			.WithName("UpdateAgent")
			.WithSummary("Update an existing agent")
			.Produces<AgentEntity>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapDelete("/{id:guid}", DeleteAsync)
			.WithName("DeleteAgent")
			.WithSummary("Delete an agent")
			.Produces(StatusCodes.Status204NoContent)
			.Produces(StatusCodes.Status404NotFound);

		return endpoints;
	}

	private static async Task<IResult> CreateAsync(
		CreateAgentRequest request,
		IAgentService service,
		CancellationToken ct)
	{
		var result = await service.CreateAsync(request.Code, request.Name, request.Description, ct);
		return Results.Created($"/api/agents/{result.Id}", result);
	}

	private static async Task<IResult> UpdateAsync(
	Guid id,
	UpdateAgentRequest request,
	IAgentService service,
	CancellationToken ct)
	{
		var result = await service.UpdateAsync(id, request.Code, request.Name, request.Description, ct);
		return Results.Ok(result);
	}

	private static async Task<IResult> GetAllAsync(
		IAgentService service,
		CancellationToken ct)
	{
		var result = await service.GetAllAsync(ct);
		return Results.Ok(result);
	}

	private static async Task<IResult> GetByIdAsync(
		Guid id,
		IAgentService service,
		CancellationToken ct)
	{
		var result = await service.GetByIdAsync(id, ct);

		return result is null ? Results.NotFound() : Results.Ok(result);
	}

	private static async Task<IResult> DeleteAsync(
		Guid id,
		IAgentService service,
		CancellationToken ct)
	{
		await service.DeleteAsync(id, ct);
		return Results.NoContent();
	}
}