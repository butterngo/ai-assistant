using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Implementations.Persistents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Agent.Core.Specialists;

internal sealed class ProjectManagerAgent : BaseAgent<ProjectManagerAgent>
{
	public ProjectManagerAgent(ILogger<ProjectManagerAgent> logger, PostgresChatMessageStoreFactory postgresChatMessageStoreFactory, ISemanticKernelBuilder semanticKernelBuilder, Func<JsonElement> func) : base(logger, postgresChatMessageStoreFactory, semanticKernelBuilder, func)
	{
	}

	protected override (ChatClientAgent agent, AgentThread thread) CreateAgent(ChatClientAgentOptions options, Func<JsonElement> func)
	{
		throw new NotImplementedException();
	}
}
