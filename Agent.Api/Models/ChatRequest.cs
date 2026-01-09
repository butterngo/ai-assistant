namespace Agent.Api.Models;

public record ChatRequest(string Message, string ConversationId = "")
{
	public string ConversationId { get; init; } =
		string.IsNullOrEmpty(ConversationId) ? Guid.NewGuid().ToString("N") : ConversationId;
}

/// <summary>
/// SSE event: metadata (sent first)
/// </summary>
public class ChatMetadata
{
	public string ConversationId { get; set; } = string.Empty;
	public string? Title { get; set; }
}

/// <summary>
/// SSE event: data (streamed content)
/// </summary>
public class ChatData
{
	public string ConversationId { get; set; } = string.Empty;
	public string Text { get; set; } = string.Empty;
}

/// <summary>
/// SSE event: done (sent last)
/// </summary>
public class ChatDone
{
	public string ConversationId { get; set; } = string.Empty;
	public string? Title { get; set; }
}

/// <summary>
/// SSE event: error
/// </summary>
public class ChatError
{
	public string Error { get; set; } = string.Empty;
	public string Code { get; set; } = string.Empty;
}
