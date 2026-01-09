namespace Agent.Api.DTOs;

public class ConversationDetailDto : ConversationDto
{
	public List<MessageDto> Messages { get; set; } = new();
	public int MessageCount { get; set; }
}
