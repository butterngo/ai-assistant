using System.Text.Json;

namespace Agent.Api.Models;

public record CreateConnectionToolRequest
{
	public required string Name { get; init; }
	public required string Type { get; init; } // 'mcp_http', 'mcp_stdio', 'openapi'
	public string? Description { get; init; }
	public string? Endpoint { get; init; }
	public string? Command { get; init; }
	public JsonDocument? Config { get; init; }
	public bool IsActive { get; init; } = true;
}

public record UpdateConnectionToolRequest
{
	public string? Name { get; init; }
	public string? Type { get; init; }
	public string? Description { get; init; }
	public string? Endpoint { get; init; }
	public string? Command { get; init; }
	public JsonDocument? Config { get; init; }
	public bool? IsActive { get; init; }
}

public record TestConnectionResponse
{
	public bool IsConnected { get; init; }
	public string Message { get; init; } = string.Empty;
}