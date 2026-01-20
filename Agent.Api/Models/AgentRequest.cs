namespace Agent.Api.Models;

public record CreateAgentRequest(string Code, string Name, string? Description = null);
public record UpdateAgentRequest(string? Code, string? Name = null, string? Description = null);
