using System.Text.Json;

namespace Agent.Api.Models;

// Category
public record CreateCategoryRequest(string Name, string? Description = null);
public record UpdateCategoryRequest(string? Name = null, string? Description = null);

// Skill
public record CreateSkillRequest(
	Guid CategoryId,
	string Name,
	string SystemPrompt,
	string Description);

public record UpdateSkillRequest(
	string? Name = null,
	string? SystemPrompt = null,
	string? Description = null);

// Tool
public record CreateToolRequest(
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

// Routing
public record RouteSkillRequest(string Query);
