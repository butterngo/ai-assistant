using System.Text.Json;

namespace Agent.Api.Models;

public record CreateToolRequest(
	Guid SkillId,
	string Name,
	string Type,
	string Endpoint,
	string? Description = null,
	JsonDocument? Config = null,
	bool IsPrefetch = false);

public record UpdateToolRequest(
	string? Name = null,
	string? Type = null,
	string? Endpoint = null,
	string? Description = null,
	JsonDocument? Config = null,
	bool? IsPrefetch = null);
