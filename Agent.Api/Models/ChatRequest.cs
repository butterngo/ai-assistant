namespace Agent.Api.Models;

public record ChatRequest(string Message, Guid? threadId);

/// <summary>
/// SSE event: metadata (sent first)
/// </summary>
public class ChatMetadata
{
	public bool IsNewConversation { get; set; }
	public Guid ThreadId { get; set; }
	public string? Title { get; set; }
}

/// <summary>
/// SSE event: data (streamed content)
/// </summary>
public class ChatData
{
	public bool IsStreaming { get; set; } = true;
	public Guid ThreadId { get; set; }
	public string Text { get; set; } = string.Empty;
}

/// <summary>
/// SSE event: done (sent last)
/// </summary>
public class ChatDone
{
	public bool IsStreaming { get; set; } = false;
	public bool IsNewConversation { get; set; }
	public Guid ThreadId { get; set; }
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
