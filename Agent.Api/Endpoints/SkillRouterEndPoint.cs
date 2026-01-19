using Agent.Api.Models;
using Agent.Core.VectorRecords;
using Microsoft.AspNetCore.Mvc;
using Agent.Core.Abstractions.Services;

namespace Agent.Core.EndPoints;

public static class SkillRouterEndPoint
{
	public static IEndpointRouteBuilder MapSkillRouterEndPoints(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/api/skill-routers")
			.WithTags("Skill Routers");

		group.MapGet("/", GetBySkillCodeAsync)
			.WithName("GetSkillRoutersBySkillCode")
			.WithSummary("Get all routing queries for a skill")
			.Produces<IEnumerable<SkillRoutingRecord>>(StatusCodes.Status200OK);

		group.MapGet("/{id:guid}", GetByIdAsync)
			.WithName("GetSkillRouterById")
			.WithSummary("Get routing query by ID")
			.Produces<SkillRoutingRecord>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapPost("/", CreateAsync)
			.WithName("CreateSkillRouter")
			.WithSummary("Create a new routing query")
			.Produces<SkillRoutingRecord>(StatusCodes.Status201Created)
			.Produces(StatusCodes.Status400BadRequest);

		group.MapPut("/{id:guid}", UpdateAsync)
			.WithName("UpdateSkillRouter")
			.WithSummary("Update a routing query")
			.Produces<SkillRoutingRecord>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapDelete("/{id:guid}", DeleteAsync)
			.WithName("DeleteSkillRouter")
			.WithSummary("Delete a routing query")
			.Produces(StatusCodes.Status204NoContent)
			.Produces(StatusCodes.Status404NotFound);

		return endpoints;
	}

	private static async Task<IResult> GetBySkillCodeAsync(
		[FromQuery] string skillCode,
		ISkillRouterService service,
		CancellationToken ct)
	{
		var results = await service.GetBySkillCodeAsync(skillCode, ct);
		return Results.Ok(results);
	}

	private static async Task<IResult> GetByIdAsync(
		Guid id,
		ISkillRouterService service,
		CancellationToken ct)
	{
		var result = await service.GetByIdAsync(id, ct);
		return result is null ? Results.NotFound() : Results.Ok(result);
	}

	private static async Task<IResult> CreateAsync(
		CreateSkillRouterRequest request,
		ISkillRouterService service,
		CancellationToken ct)
	{
		var result = await service.CreateAsync(request.SkillCode, request.SkillName, request.UserQueries, ct);
		return Results.Created($"/api/skill-routers/{result.Id}", result);
	}

	private static async Task<IResult> UpdateAsync(
		Guid id,
		UpdateSkillRouterRequest request,
		ISkillRouterService service,
		CancellationToken ct)
	{
		var result = await service.UpdateAsync(id, request.UserQueries, ct);
		return result is null ? Results.NotFound() : Results.Ok(result);
	}

	private static async Task<IResult> DeleteAsync(
		Guid id,
		ISkillRouterService service,
		CancellationToken ct)
	{
		await service.DeleteAsync(id, ct);
		return Results.NoContent();
	}
}