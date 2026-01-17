using Agent.Api.Models;
using System.Text.Json;
using Agent.Core.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace Agent.Api.Endpoints;

public static class ChatBotEndpoints
{
	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase
	};

	private const string XAgentId = "X-Agent-Id";
	private const string XSkillId = "X-Skill-Id";

	public static IEndpointRouteBuilder MapChatBotEndPoints(this IEndpointRouteBuilder endpoints)
	{
		endpoints.MapPost("/chat/stream", HandleChatStreamAsync)
			.WithTags("ChatBot")
			.WithSummary("Stream chat response with SSE")
			.Produces(StatusCodes.Status200OK, contentType: "text/event-stream")
			.Produces(StatusCodes.Status400BadRequest);

		return endpoints;
	}

	private static Guid? GetAgentId(HttpContext ctx) 
	{

		if (ctx.Request.Headers.ContainsKey(XAgentId)) 
		{
			return Guid.Parse(ctx.Request.Headers[XAgentId].ToString());
		}

		return null;
	}
	
	private static async Task HandleChatStreamAsync(
		HttpContext ctx,
		[FromBody] ChatRequest req,
		[FromServices] IAgentManager manager,
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
			var (agent, thread, isNewConversation) = await manager.GetOrCreateAsync(GetAgentId(ctx), req.threadId, req.Message, ct);

			var metadata = new ChatMetadata
			{
				IsNewConversation = isNewConversation,
				ThreadId = thread.Id,
				Title = thread.Title
			};

			await WriteEventAsync(ctx.Response, "metadata", metadata, ct);

			await foreach (var update in agent.RunStreamingAsync(req.Message).WithCancellation(ct))
			{
				if (!string.IsNullOrEmpty(update.Text))
				{
					var data = new ChatData
					{
						ThreadId = thread.Id,
						Text = update.Text
					};

					await WriteEventAsync(ctx.Response, "data", data, ct);
				}
			}

			var doneEvent = new ChatDone
			{
				IsNewConversation = isNewConversation,
				ThreadId = thread.Id,
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
