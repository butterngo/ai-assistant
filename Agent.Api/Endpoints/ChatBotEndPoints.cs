using Agent.Api.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Agent.Api.Endpoints;

public static class ChatBotEndpoints
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	public static IEndpointRouteBuilder MapChatBot(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapPost("/chat/stream", HandleChatStreamAsync)
			.WithTags("Chat")
			.WithSummary("Stream chat response with SSE")
			.Produces(StatusCodes.Status200OK, contentType: "text/event-stream")
			.Produces(StatusCodes.Status400BadRequest);

		return endpoints;
	}

	private static async Task HandleChatStreamAsync(
		HttpContext ctx,
		[FromBody] ChatRequest req,
		[FromServices] AgentManager manager,
		CancellationToken ct)
	{
		if (string.IsNullOrWhiteSpace(req.Message))
		{
			ctx.Response.StatusCode = StatusCodes.Status400BadRequest;
			await ctx.Response.WriteAsJsonAsync(new { error = "Message required" }, ct);
			return;
		}

		// SSE headers
		ctx.Response.ContentType = "text/event-stream";
		ctx.Response.Headers.CacheControl = "no-cache";
		ctx.Response.Headers.Connection = "keep-alive";

		try
		{
			var (agent, thread) = await manager.GetOrCreateAsync(req.ConversationId, req.Message, ct);

			var metadata = new ChatMetadata
			{
				ConversationId = thread.ThreadId,
				Title = thread.Title
			};

			await WriteEventAsync(ctx.Response, "metadata", metadata, ct);

			await foreach (var update in agent.RunStreamingAsync(req.Message).WithCancellation(ct))
			{
				if (!string.IsNullOrEmpty(update.Text))
				{
					var data = new ChatData
					{
						ConversationId = thread.ThreadId,
						Text = update.Text
					};

					await WriteEventAsync(ctx.Response, "data", data, ct);
				}
			}

			var doneEvent = new ChatDone
			{
				ConversationId = thread.ThreadId,
				Title = thread.Title
			};

			await WriteEventAsync(ctx.Response, "done", doneEvent, ct);
		}
		catch (OperationCanceledException)
		{
			// Client disconnected - no action needed
		}
		catch (Exception ex)
		{
			var error = new ChatError
			{
				Error = ex.Message,
				Code = "INTERNAL_ERROR"
			};
			await WriteEventAsync(ctx.Response, "error", error, ct);
		}
	}

	private static async Task WriteEventAsync<T>(
		HttpResponse response,
		string eventType,
		T data,
		CancellationToken ct)
	{
		var json = JsonSerializer.Serialize(data, JsonOptions);
		await response.WriteAsync($"event: {eventType}\n", ct);
		await response.WriteAsync($"data: {json}\n\n", ct);
		await response.Body.FlushAsync(ct);
	}
}
