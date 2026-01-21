using Agent.Core.Entities;
using Agent.Core.Enums;

namespace Agent.Core.Abstractions;

public interface IAgentManager
{
	Task<(IAgent agent, ChatThreadEntity thread, bool isNewConversation)> GetOrCreateAsync(Guid? agentId, Guid? threadId,
		string userMessage,
		ChatMessageStoreEnum chatMessageStore = ChatMessageStoreEnum.Postgresql,
		CancellationToken ct = default);

	ICurrentThreadContext CurrentThreadContext { get; }

	Task<object> DryRunAsync(string userMessage, CancellationToken ct = default);
}
