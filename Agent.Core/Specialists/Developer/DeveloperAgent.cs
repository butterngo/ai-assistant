using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Implementations.Persistents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Agent.Core.Specialists
{
	internal class DeveloperAgent : BaseAgent<DeveloperAgent>
	{
		public DeveloperAgent(ILogger<DeveloperAgent> logger,
			PostgresChatMessageStoreFactory postgresChatMessageStoreFactory,
			ISemanticKernelBuilder semanticKernelBuilder, Func<JsonElement> func) 
			: base(logger, postgresChatMessageStoreFactory, semanticKernelBuilder, func)
		{
			Id = Guid.Parse("00000000-0000-0000-0000-000000000005");
		}

		protected override (ChatClientAgent agent, AgentThread thread) CreateAgent(ChatClientAgentOptions options, Func<JsonElement> func)
		{
			throw new NotImplementedException();
		}
	}
}
