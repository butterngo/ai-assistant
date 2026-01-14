using Agent.Api.Models;
using Agent.Core.Abstractions;

namespace Agent.Api.Endpoints
{
	public static class SkillEndPoints
	{
		public static IEndpointRouteBuilder MapSkills(this IEndpointRouteBuilder endpoints)
		{
			var group = endpoints.MapGroup("/api/skills")
				.WithTags("Skills");

			// Category
			group.MapPost("/categories", CreateCategoryAsync);
			group.MapGet("/categories", GetAllCategoriesAsync);
			group.MapGet("/categories/{id:guid}", GetCategoryByIdAsync);
			group.MapPut("/categories/{id:guid}", UpdateCategoryAsync);
			group.MapDelete("/categories/{id:guid}", DeleteCategoryAsync);

			// Skill
			group.MapPost("/", CreateSkillAsync);
			group.MapGet("/by-category/{categoryId:guid}", GetSkillsByCategoryAsync);
			group.MapGet("/{id:guid}", GetSkillByIdAsync);
			group.MapPut("/{id:guid}", UpdateSkillAsync);
			group.MapDelete("/{id:guid}", DeleteSkillAsync);

			// Tool
			group.MapPost("/{skillId:guid}/tools", CreateToolAsync);
			group.MapGet("/{skillId:guid}/tools", GetToolsBySkillAsync);
			group.MapGet("/tools/{id:guid}", GetToolByIdAsync);
			group.MapPut("/tools/{id:guid}", UpdateToolAsync);
			group.MapDelete("/tools/{id:guid}", DeleteToolAsync);

			// Routing
			group.MapPost("/route", RouteToSkillAsync);

			return endpoints;
		}

		#region Category

		private static async Task<IResult> CreateCategoryAsync(
			CreateCategoryRequest request,
			ISkillService service,
			CancellationToken ct)
		{
			var result = await service.CreateCategoryAsync(request.Name, request.Description, ct);
			return Results.Created($"/api/skills/categories/{result.Id}", result);
		}

		private static async Task<IResult> GetAllCategoriesAsync(
			ISkillService service,
			CancellationToken ct)
		{
			var result = await service.GetAllCategoriesAsync(ct);
			return Results.Ok(result);
		}

		private static async Task<IResult> GetCategoryByIdAsync(
			Guid id,
			ISkillService service,
			CancellationToken ct)
		{
			var result = await service.GetCategoryByIdAsync(id, ct);
			return result is null ? Results.NotFound() : Results.Ok(result);
		}

		private static async Task<IResult> UpdateCategoryAsync(
			Guid id,
			UpdateCategoryRequest request,
			ISkillService service,
			CancellationToken ct)
		{
			var result = await service.UpdateCategoryAsync(id, request.Name, request.Description, ct);
			return Results.Ok(result);
		}

		private static async Task<IResult> DeleteCategoryAsync(
			Guid id,
			ISkillService service,
			CancellationToken ct)
		{
			await service.DeleteCategoryAsync(id, ct);
			return Results.NoContent();
		}

		#endregion

		#region Skill

		private static async Task<IResult> CreateSkillAsync(
			CreateSkillRequest request,
			ISkillService service,
			CancellationToken ct)
		{
			var result = await service.CreateSkillAsync(
				request.CategoryId,
				request.Name,
				request.SystemPrompt,
				request.Description,
				ct);
			return Results.Created($"/api/skills/{result.Id}", result);
		}

		private static async Task<IResult> GetSkillByIdAsync(
			Guid id,
			ISkillService service,
			CancellationToken ct)
		{
			var result = await service.GetSkillByIdAsync(id, ct);
			return result is null ? Results.NotFound() : Results.Ok(result);
		}

		private static async Task<IResult> GetSkillsByCategoryAsync(
			Guid categoryId,
			ISkillService service,
			CancellationToken ct)
		{
			var result = await service.GetSkillsByCategoryAsync(categoryId, ct);
			return Results.Ok(result);
		}

		private static async Task<IResult> UpdateSkillAsync(
			Guid id,
			UpdateSkillRequest request,
			ISkillService service,
			CancellationToken ct)
		{
			var result = await service.UpdateSkillAsync(
				id,
				request.Name,
				request.SystemPrompt,
				request.Description,
				ct);
			return Results.Ok(result);
		}

		private static async Task<IResult> DeleteSkillAsync(
			Guid id,
			ISkillService service,
			CancellationToken ct)
		{
			await service.DeleteSkillAsync(id, ct);
			return Results.NoContent();
		}

		#endregion

		#region Tool

		private static async Task<IResult> CreateToolAsync(
			Guid skillId,
			CreateToolRequest request,
			ISkillService service,
			CancellationToken ct)
		{
			var result = await service.CreateToolAsync(
				skillId,
				request.Name,
				request.Type,
				request.Endpoint,
				request.Description,
				request.Config,
				request.IsPrefetch,
				ct);
			return Results.Created($"/api/skills/tools/{result.Id}", result);
		}

		private static async Task<IResult> GetToolByIdAsync(
			Guid id,
			ISkillService service,
			CancellationToken ct)
		{
			var result = await service.GetToolByIdAsync(id, ct);
			return result is null ? Results.NotFound() : Results.Ok(result);
		}

		private static async Task<IResult> GetToolsBySkillAsync(
			Guid skillId,
			ISkillService service,
			CancellationToken ct)
		{
			var result = await service.GetToolsBySkillAsync(skillId, ct);
			return Results.Ok(result);
		}

		private static async Task<IResult> UpdateToolAsync(
			Guid id,
			UpdateToolRequest request,
			ISkillService service,
			CancellationToken ct)
		{
			var result = await service.UpdateToolAsync(
				id,
				request.Name,
				request.Type,
				request.Endpoint,
				request.Description,
				request.Config,
				request.IsPrefetch,
				ct);
			return Results.Ok(result);
		}

		private static async Task<IResult> DeleteToolAsync(
			Guid id,
			ISkillService service,
			CancellationToken ct)
		{
			await service.DeleteToolAsync(id, ct);
			return Results.NoContent();
		}

		#endregion

		#region Routing

		private static async Task<IResult> RouteToSkillAsync(
			RouteSkillRequest request,
			ISkillService service,
			CancellationToken ct)
		{
			var result = await service.RouteToSkillAsync(request.Query, ct);
			return result is null ? Results.NotFound() : Results.Ok(result);
		}

		#endregion
	}
}
