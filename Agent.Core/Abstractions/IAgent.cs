using Microsoft.Agents.AI;

namespace Agent.Core.Abstractions;

public interface IAgent
{
	public string Id { get; }
	public string Name { get; }
	void SetConversationId(string conversationId);
	IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(string userMessage, 
		CancellationToken cancellationToken = default);
	Task<AgentRunResponse> RunAsync(string userMessage,
		CancellationToken cancellationToken = default);
}
