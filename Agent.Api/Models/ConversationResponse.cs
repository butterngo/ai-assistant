namespace Agent.Api.Models
{
	public class ConversationResponse
	{
		public Guid Id { get; set; }
		public string ThreadId { get; set; } = string.Empty;
		public string? Title { get; set; }
		public DateTimeOffset CreatedAt { get; set; }
		public DateTimeOffset UpdatedAt { get; set; }
		public string? UserId { get; set; }
	}

	public class MessageResponse
	{
		public Guid Id { get; set; }
		public string MessageId { get; set; } = string.Empty;
		public string Role { get; set; } = string.Empty;
		public string Content { get; set; } = string.Empty;
		public DateTimeOffset CreatedAt { get; set; }
		public long SequenceNumber { get; set; }
	}

	public class ConversationDetailResponse : ConversationResponse
	{
		public List<MessageResponse> Messages { get; set; } = new();
		public int MessageCount { get; set; }
	}

}
