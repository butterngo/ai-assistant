namespace Agent.Api.Models;

public record CreateCategoryRequest(string Name, string? Description = null);
public record UpdateCategoryRequest(string? Name = null, string? Description = null);
