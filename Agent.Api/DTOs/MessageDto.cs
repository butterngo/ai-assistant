namespace Agent.Api.DTOs;

public class MessageDto
{
	public Guid Id { get; set; }
	public string MessageId { get; set; } = string.Empty;
	public string Role { get; set; } = string.Empty;
	public string Content { get; set; } = string.Empty;
	public DateTimeOffset CreatedAt { get; set; }
	public long SequenceNumber { get; set; }
}
