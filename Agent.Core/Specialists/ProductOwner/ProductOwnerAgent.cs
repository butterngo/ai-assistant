using Microsoft.Agents.AI;
using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Microsoft.Extensions.Logging;

namespace Agent.Core.Specialists;

public sealed class ProductOwnerAgent : BaseAgent<ProductOwnerAgent>
{
	public ProductOwnerAgent(ILogger<ProductOwnerAgent> logger,
		ISemanticKernelBuilder semanticKernelBuilder,
		ChatMessageStore chatMessageStore,
		AIContextProvider aIContextProvider)
		: base(logger, chatMessageStore, aIContextProvider, semanticKernelBuilder)
	{
		Id = Guid.Parse("00000000-0000-0000-0000-000000000003");
	}
}
