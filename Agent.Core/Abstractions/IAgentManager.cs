using Agent.Core.Domains;
using Agent.Core.Specialists;

namespace Agent.Core.Abstractions;

public interface IAgentManager
{
	Task<(GeneralAgent agent, ChatThreadEntity thread)> GetOrCreateAsync(string? conversationId,
		string userMessage,
		CancellationToken ct = default);
}
