using Agent.Api.Models;
using Agent.Core.Entities;
using Agent.Core.VectorRecords;
using Microsoft.AspNetCore.Mvc;
using Agent.Core.Abstractions.Services;
using Agent.Core.Abstractions.Persistents;

namespace Agent.Api.Endpoints;

public static class SkillEndPoint
{
	public static IEndpointRouteBuilder MapSkillEndPoints(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/api/skills")
		   .WithTags("Skills");

		group.MapPost("/", CreateAsync)
			.WithName("CreateSkill")
			.WithSummary("Create a new skill")
			.Produces<SkillEntity>(StatusCodes.Status201Created)
			.Produces(StatusCodes.Status400BadRequest)
			.Produces(StatusCodes.Status404NotFound);

		group.MapGet("/{id:guid}", GetByIdAsync)
			.WithName("GetSkillById")
			.WithSummary("Get skill by ID")
			.Produces<SkillEntity>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapGet("/by-category/{categoryId:guid}", GetByCategoryAsync)
			.WithName("GetSkillsByCategory")
			.WithSummary("Get all skills by category")
			.Produces<IEnumerable<SkillEntity>>(StatusCodes.Status200OK);

		group.MapPut("/{id:guid}", UpdateAsync)
			.WithName("UpdateSkill")
			.WithSummary("Update an existing skill")
			.Produces<SkillEntity>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapDelete("/{id:guid}", DeleteAsync)
			.WithName("DeleteSkill")
			.WithSummary("Delete a skill")
			.Produces(StatusCodes.Status204NoContent)
			.Produces(StatusCodes.Status404NotFound);

		group.MapPost("/route", RouteAsync)
			.WithName("RouteToSkill")
			.WithSummary("Route a query to the best matching skill")
			.Produces<SkillEntity>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapPost("/vector-search", HandleVectorSearchAsync)
			.WithName("VectorSearch")
			.Produces<SkillEntity>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		return endpoints;
	}

	private static async Task<IResult> CreateAsync(
		CreateSkillRequest request,
		ISkillService service,
		CancellationToken ct)
	{
		var result = await service.CreateAsync(
			request.CategoryId,
			request.Code,
			request.Name,
			request.SystemPrompt,
			ct);
		return Results.Created($"/api/skills/{result.Id}", result);
	}

	private static async Task<IResult> UpdateAsync(
	Guid id,
	UpdateSkillRequest request,
	ISkillService service,
	CancellationToken ct)
	{
		var result = await service.UpdateAsync(
			id,
			request.Code,
			request.Name,
			request.SystemPrompt,
			ct);
		return Results.Ok(result);
	}

	private static async Task<IResult> GetByIdAsync(
		Guid id,
		ISkillService service,
		CancellationToken ct)
	{
		var result = await service.GetByIdAsync(id, ct);
		return result is null ? Results.NotFound() : Results.Ok(result);
	}

	private static async Task<IResult> GetByCategoryAsync(
		Guid categoryId,
		ISkillService service,
		CancellationToken ct)
	{
		var category = await service.GetByCategoryAsync(categoryId, ct);

		return Results.Ok(category);
	}

	private static async Task<IResult> DeleteAsync(
		Guid id,
		ISkillService service,
		CancellationToken ct)
	{
		await service.DeleteAsync(id, ct);
		return Results.NoContent();
	}

	private static async Task<IResult> RouteAsync(
		RouteSkillRequest request,
		ISkillService service,
		CancellationToken ct)
	{
		var result = await service.RouteAsync(request.Query, ct);
		return result is null ? Results.NotFound() : Results.Ok(result);
	}

	private static async Task<IResult> HandleVectorSearchAsync(
		[FromBody] ChatRequest req,
		IQdrantRepository<SkillRoutingRecord> qdrantRepository,
		CancellationToken ct) 
	{
		var results = await qdrantRepository.SearchAsync(
			query: req.Message,
			top: 5,
			similarityThreshold: 0,
			cancellationToken: ct);

		return Results.Ok(results.Select(x=>new { x.SkillCode, x.Score}));
	}
}