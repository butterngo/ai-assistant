namespace Agent.Api.Models;

// Skill
public record CreateSkillRequest(
	Guid CategoryId,
	string Code,
	string Name,
	string SystemPrompt);

public record UpdateSkillRequest(
	string? Code = null,
	string? Name = null,
	string? SystemPrompt = null);

// Routing
public record RouteSkillRequest(string Query);

public record CreateSkillRouterRequest(string SkillCode, string SkillName, string UserQueries);
public record UpdateSkillRouterRequest(string UserQueries);