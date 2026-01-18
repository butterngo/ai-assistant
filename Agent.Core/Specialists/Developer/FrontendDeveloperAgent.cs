using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Microsoft.Extensions.Logging;

namespace Agent.Core.Specialists;

public sealed class FrontendDeveloperAgent : BaseAgent<FrontendDeveloperAgent>
{
	public FrontendDeveloperAgent(ILogger<FrontendDeveloperAgent> logger,
		ChatMessageStore chatMessageStore,
		AIContextProvider aIContextProvider,
		ISemanticKernelBuilder semanticKernelBuilder)
		: base(logger, chatMessageStore, aIContextProvider, semanticKernelBuilder)
	{
		Id = Guid.Parse("00000000-0000-0000-0000-000000000006");
	}

	protected override void SetAgentOptions(IChatClient chatClient, ChatClientAgentOptions options)
	{
		throw new NotImplementedException();
	}
}
