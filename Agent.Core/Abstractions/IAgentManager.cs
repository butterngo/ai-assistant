using Agent.Core.Entities;

namespace Agent.Core.Abstractions;

public interface IAgentManager
{
	Task<(IAgent agent, ChatThreadEntity thread, bool isNewConversation)> GetOrCreateAsync(Guid? threadId,
		string userMessage,
		CancellationToken ct = default);

	Task<(IAgent agent, ChatThreadEntity thread, bool isNewConversation)> GetOrCreateAsync(Guid agentId, Guid? threadId,
		string userMessage,
		CancellationToken ct = default);

	Task<object> DryRunAsync(string userMessage, CancellationToken ct = default);
}
