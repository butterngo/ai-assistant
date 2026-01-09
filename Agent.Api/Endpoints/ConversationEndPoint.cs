using Agent.Core.Domains;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Agent.Api.Endpoints;

public static class ConversationEndPoint
{
    // Simple in-memory store for conversations. Replace with a persistent repository if needed.
    private static readonly ConcurrentDictionary<string, Conversation> _conversations = new();

    public static IEndpointRouteBuilder MapConversations(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/conversations", GetAllConversations);
        endpoints.MapGet("/conversations/{id}", GetConversationById);
        endpoints.MapPost("/conversations", CreateConversation);
        endpoints.MapPut("/conversations/{id}", UpdateConversation);
        endpoints.MapDelete("/conversations/{id}", DeleteConversation);

        return endpoints;
    }

    private static IResult GetAllConversations()
    {
        return Results.Ok(_conversations.Values);
    }

    private static IResult GetConversationById(string id)
    {
        if (_conversations.TryGetValue(id, out var conv))
        {
            return Results.Ok(conv);
        }

        return Results.NotFound();
    }

    private static IResult CreateConversation([FromBody] Conversation conv)
    {
        if (string.IsNullOrWhiteSpace(conv.Name))
        {
            return Results.BadRequest(new { error = "Name is required" });
        }

        // Ensure id and createdAt
        var id = string.IsNullOrWhiteSpace(conv.Id) ? Guid.NewGuid().ToString("n") : conv.Id;
        conv.Id = id;
        conv.CreatedAt = conv.CreatedAt == default ? DateTime.UtcNow : conv.CreatedAt;

        if (!_conversations.TryAdd(id, conv))
        {
            return Results.Conflict(new { error = "Conversation with the same id already exists" });
        }

        return Results.Created($"/conversations/{id}", conv);
    }

    private static IResult UpdateConversation(string id, [FromBody] Conversation update)
    {
        if (!_conversations.TryGetValue(id, out var existing))
        {
            return Results.NotFound();
        }

        if (!string.IsNullOrWhiteSpace(update.Name))
        {
            existing.Name = update.Name;
        }

        // For now we don't allow updating CreatedAt or Id through this endpoint.
        _conversations[id] = existing;

        return Results.Ok(existing);
    }

    private static IResult DeleteConversation(string id, [FromServices] AgentManager? manager)
    {
        _conversations.TryRemove(id, out _);

        // Inform AgentManager to remove any in-memory agent for this conversation (if provided)
        try
        {
            manager?.Remove(id);
        }
        catch
        {
            // Swallow errors from manager removal to keep delete idempotent
        }

        return Results.NoContent();
    }
}
