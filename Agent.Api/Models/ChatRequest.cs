namespace Agent.Api.Models;

public record ChatRequest(string Message, string ConversationId = "")
{
	public string ConversationId { get; init; } =
		string.IsNullOrEmpty(ConversationId) ? Guid.NewGuid().ToString("N") : ConversationId;
}
