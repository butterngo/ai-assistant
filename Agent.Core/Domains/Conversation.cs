namespace Agent.Core.Domains;

public class Conversation
{
	public required string Id { get; set; } = Guid.NewGuid().ToString("n");
	public required string Name { get; set; }
	public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
