using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Implementations.Persistents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;

namespace Agent.Core.Specialists;

internal sealed class ProjectManagerAgent : BaseAgent<ProjectManagerAgent>
{
	public ProjectManagerAgent(ILogger<ProjectManagerAgent> logger, PostgresChatMessageStoreFactory postgresChatMessageStoreFactory, ISemanticKernelBuilder semanticKernelBuilder)
		: base(logger, postgresChatMessageStoreFactory, semanticKernelBuilder)
	{
	}


	protected override (ChatClientAgent agent, AgentThread thread) CreateAgent(ChatClientAgentOptions options)
	{
		throw new NotImplementedException();
	}
}
