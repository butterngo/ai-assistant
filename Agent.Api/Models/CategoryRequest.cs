namespace Agent.Api.Models;

public record CreateCategoryRequest(string Code, string Name, string? Description = null);
public record UpdateCategoryRequest(string? Code, string? Name = null, string? Description = null);
