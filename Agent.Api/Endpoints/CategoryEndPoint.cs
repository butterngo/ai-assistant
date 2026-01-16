using Agent.Api.Models;
using Agent.Core.Abstractions.Services;
using Agent.Core.Entities;

namespace Agent.Api.Endpoints;

public static class CategoryEndPoint
{
	public static IEndpointRouteBuilder MapCategoryEndPoints(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/api/categories")
			.WithTags("Categories");

		group.MapPost("/", CreateAsync)
			.WithName("CreateCategory")
			.WithSummary("Create a new category")
			.Produces<CategoryEntity>(StatusCodes.Status201Created)
			.Produces(StatusCodes.Status400BadRequest);

		group.MapGet("/", GetAllAsync)
			.WithName("GetAllCategories")
			.WithSummary("Get all categories")
			.Produces<IEnumerable<CategoryEntity>>(StatusCodes.Status200OK);

		group.MapGet("/{id:guid}", GetByIdAsync)
			.WithName("GetCategoryById")
			.WithSummary("Get category by ID")
			.Produces<CategoryEntity>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapPut("/{id:guid}", UpdateAsync)
			.WithName("UpdateCategory")
			.WithSummary("Update an existing category")
			.Produces<CategoryEntity>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapDelete("/{id:guid}", DeleteAsync)
			.WithName("DeleteCategory")
			.WithSummary("Delete a category")
			.Produces(StatusCodes.Status204NoContent)
			.Produces(StatusCodes.Status404NotFound);

		return endpoints;
	}

	private static async Task<IResult> CreateAsync(
		CreateCategoryRequest request,
		ICategoryService service,
		CancellationToken ct)
	{
		var result = await service.CreateAsync(request.Code, request.Name, request.Description, ct);
		return Results.Created($"/api/categories/{result.Id}", result);
	}

	private static async Task<IResult> UpdateAsync(
	Guid id,
	UpdateCategoryRequest request,
	ICategoryService service,
	CancellationToken ct)
	{
		var result = await service.UpdateAsync(id, request.Code, request.Name, request.Description, ct);
		return Results.Ok(result);
	}

	private static async Task<IResult> GetAllAsync(
		ICategoryService service,
		CancellationToken ct)
	{
		var result = await service.GetAllAsync(ct);
		return Results.Ok(result);
	}

	private static async Task<IResult> GetByIdAsync(
		Guid id,
		ICategoryService service,
		CancellationToken ct)
	{
		var result = await service.GetByIdAsync(id, ct);
		return result is null ? Results.NotFound() : Results.Ok(result);
	}

	private static async Task<IResult> DeleteAsync(
		Guid id,
		ICategoryService service,
		CancellationToken ct)
	{
		await service.DeleteAsync(id, ct);
		return Results.NoContent();
	}
}