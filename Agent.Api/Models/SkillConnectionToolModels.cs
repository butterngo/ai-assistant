namespace Agent.Api.Models;

public record LinkConnectionToolRequest
{
	public required Guid SkillId { get; init; }
	public required Guid ConnectionToolId { get; init; }
}

public record BulkLinkConnectionToolsRequest
{
	public required IEnumerable<Guid> ConnectionToolIds { get; init; }
}

public record SyncConnectionToolsRequest
{
	public required IEnumerable<Guid> ConnectionToolIds { get; init; }
}

public record ExistsResponse
{
	public bool Exists { get; init; }
	public Guid SkillId { get; init; }
	public Guid ConnectionToolId { get; init; }
}