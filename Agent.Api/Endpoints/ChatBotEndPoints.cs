using Agent.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Agent.Api.Endpoints;

public static class ChatBotEndPoints
{
	public static IEndpointRouteBuilder MapChatBot(this IEndpointRouteBuilder endpoints) 
	{
		endpoints.MapPost("/chat/stream", HandleChatStreamAsync);
		
		return endpoints;
	}

	private static async Task HandleChatStreamAsync(HttpContext ctx,
		[FromBody] ChatRequest req,
		[FromServices] AgentManager manager,
		CancellationToken ct)
	{
		// Validate
		if (string.IsNullOrWhiteSpace(req.Message))
		{
			ctx.Response.StatusCode = 400;
			await ctx.Response.WriteAsJsonAsync(new { error = "Message required" });
			return;
		}

		// SSE headers
		ctx.Response.ContentType = "text/event-stream";
		ctx.Response.Headers.CacheControl = "no-cache";
		ctx.Response.Headers.Connection = "keep-alive";

		try
		{
			var agent = manager.GetOrCreate(req.ConversationId);

			await foreach (var update in agent.RunStreamingAsync(req.Message).WithCancellation(ct))
			{
				if (!string.IsNullOrEmpty(update.Text))
				{
					
					await ctx.Response.WriteAsync($"data: {JsonSerializer.Serialize(new { text = update.Text })}\n\n", ct);
					await ctx.Response.Body.FlushAsync(ct);
				}
			}

			await ctx.Response.WriteAsync("data: [DONE]\n\n", ct);
		}
		catch (OperationCanceledException) { /* Client disconnected */ }
		catch (Exception ex)
		{
			await ctx.Response.WriteAsync($"data: {JsonSerializer.Serialize(new { error = ex.Message })}\n\n", ct);
		}
	}
}
