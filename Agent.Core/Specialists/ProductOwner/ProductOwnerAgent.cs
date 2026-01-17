using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Microsoft.Extensions.Logging;

namespace Agent.Core.Specialists;

internal class ProductOwnerAgent : BaseAgent<ProductOwnerAgent>
{
	public ProductOwnerAgent(ILogger<ProductOwnerAgent> logger,
		ChatMessageStore chatMessageStore,
		ISemanticKernelBuilder semanticKernelBuilder) 
		: base(logger, chatMessageStore, semanticKernelBuilder)
	{
		Id = Guid.Parse("00000000-0000-0000-0000-000000000003");
	}

	protected override void SetAgentOptions(IChatClient chatClient, ChatClientAgentOptions options)
	{
		throw new NotImplementedException();
	}
}
