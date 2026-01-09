namespace Agent.Api.DTOs;

public class ConversationDto
{
	public Guid Id { get; set; }
	public string ThreadId { get; set; } = string.Empty;
	public string? Title { get; set; }
	public DateTimeOffset CreatedAt { get; set; }
	public DateTimeOffset UpdatedAt { get; set; }
	public string? UserId { get; set; }
}
