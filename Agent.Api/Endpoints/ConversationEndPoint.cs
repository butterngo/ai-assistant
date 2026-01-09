using Agent.Core.Domains;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Agent.Api.Endpoints;

public static class ConversationEndPoint
{
    // Simple in-memory store for conversations. Replace with a persistent repository if needed.
    private static readonly ConcurrentDictionary<string, ChatThreadEntity> _conversations = new();

    public static IEndpointRouteBuilder MapConversations(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/conversations", GetAllConversations);
        endpoints.MapGet("/conversations/{id}", GetConversationById);

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
}
