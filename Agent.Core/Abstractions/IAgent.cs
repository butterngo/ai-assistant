using Microsoft.Agents.AI;

namespace Agent.Core.Abstractions;

public interface IAgent
{
	public Guid Id { get; }
	public string Name { get; }
	IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(string userMessage, 
		CancellationToken cancellationToken = default);
	Task<AgentRunResponse> RunAsync(string userMessage,
		CancellationToken cancellationToken = default);
}
