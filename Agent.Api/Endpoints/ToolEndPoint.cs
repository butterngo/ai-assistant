using Agent.Api.Models;
using Agent.Core.Abstractions.Services;
using Agent.Core.Entities;
using Agent.Core.Models;

namespace Agent.Api.Endpoints;

public static class ToolEndPoint
{
	public static IEndpointRouteBuilder MapToolEndPoints(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/api/tools")
			.WithTags("Tools");

		group.MapPost("/", CreateAsync)
			.WithName("CreateTool")
			.WithSummary("Create a new tool for a skill")
			.Produces<ToolEntity>(StatusCodes.Status201Created)
			.Produces(StatusCodes.Status400BadRequest)
			.Produces(StatusCodes.Status404NotFound);

		group.MapGet("/{id:guid}", GetByIdAsync)
			.WithName("GetToolById")
			.WithSummary("Get tool by ID")
			.Produces<ToolEntity>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapGet("/by-skill/{skillId:guid}", GetBySkillAsync)
			.WithName("GetToolsBySkill")
			.WithSummary("Get all tools by skill")
			.Produces<IEnumerable<ToolEntity>>(StatusCodes.Status200OK);

		group.MapPut("/{id:guid}", UpdateAsync)
			.WithName("UpdateTool")
			.WithSummary("Update an existing tool")
			.Produces<ToolEntity>(StatusCodes.Status200OK)
			.Produces(StatusCodes.Status404NotFound);

		group.MapDelete("/{id:guid}", DeleteAsync)
			.WithName("DeleteTool")
			.WithSummary("Delete a tool")
			.Produces(StatusCodes.Status204NoContent)
			.Produces(StatusCodes.Status404NotFound);

		group.MapPost("/test-mcp", async (IConfiguration configuration) => 
		{
			var githubTool = new ConnectionTool
			{
				PluginName = "github",
				ToolType = ConnectionToolType.MCP_STDIO,
				Command = "npx",
				Arguments = new List<string> { "-y", "@modelcontextprotocol/server-github" },
				EnvironmentVariables = new Dictionary<string, string?>
				{
					["GITHUB_PERSONAL_ACCESS_TOKEN"] = configuration.GetValue<string>("Github_ApiKey")
				},
				OnStandardError = (line) => Console.WriteLine($"GitHub MCP: {line}")
			};

			var tools = await githubTool.GetToolsAsync();

			return Results.Ok(tools);
		});

		return endpoints;
	}

	private static async Task<IResult> CreateAsync(
		CreateToolRequest request,
		IToolService service,
		CancellationToken ct)
	{
		var result = await service.CreateAsync(
			request.SkillId,
			request.Name,
			request.Type,
			request.Endpoint,
			request.Description,
			request.Config,
			request.IsPrefetch,
			ct);
		return Results.Created($"/api/tools/{result.Id}", result);
	}

	private static async Task<IResult> GetByIdAsync(
		Guid id,
		IToolService service,
		CancellationToken ct)
	{
		var result = await service.GetByIdAsync(id, ct);
		return result is null ? Results.NotFound() : Results.Ok(result);
	}

	private static async Task<IResult> GetBySkillAsync(
		Guid skillId,
		IToolService service,
		CancellationToken ct)
	{
		var result = await service.GetBySkillAsync(skillId, ct);
		return Results.Ok(result);
	}

	private static async Task<IResult> UpdateAsync(
		Guid id,
		UpdateToolRequest request,
		IToolService service,
		CancellationToken ct)
	{
		var result = await service.UpdateAsync(
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

	private static async Task<IResult> DeleteAsync(
		Guid id,
		IToolService service,
		CancellationToken ct)
	{
		await service.DeleteAsync(id, ct);
		return Results.NoContent();
	}
}

