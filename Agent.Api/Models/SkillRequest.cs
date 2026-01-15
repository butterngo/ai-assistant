using System.Text.Json;

namespace Agent.Api.Models;

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

// Routing
public record RouteSkillRequest(string Query);
