using System.Text.Json;

namespace Agent.Api.Models;

public record UpdateDiscoveredToolRequest
{
	public string? Name { get; init; }
	public string? Description { get; init; }
	public JsonDocument? ToolSchema { get; init; }
	public bool? IsAvailable { get; init; }
}

public record CacheStatusResponse
{
	public bool IsStale { get; init; }
	public TimeSpan MaxAge { get; init; }
	public string Message { get; init; } = string.Empty;
}