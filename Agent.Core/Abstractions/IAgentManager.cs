using Agent.Core.Entities;
using Agent.Core.Specialists;

namespace Agent.Core.Abstractions;

public interface IAgentManager
{
	Task<(GeneralAgent agent, ChatThreadEntity thread, bool isNewConversation)> GetOrCreateAsync(Guid? threadId,
		string userMessage,
		CancellationToken ct = default);

	Task<object> DryRunAsync(string userMessage, CancellationToken ct = default);
}
