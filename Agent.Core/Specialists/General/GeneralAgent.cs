using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Implementations.LLM;
using Agent.Core.Implementations.Persistents;
using Agent.Core.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Agent.Core.Specialists;

public sealed class GeneralAgent : BaseAgent<GeneralAgent>
{
	public GeneralAgent(ILogger<GeneralAgent> logger,
		ISemanticKernelBuilder semanticKernelBuilder,
		PostgresChatMessageStoreFactory postgresChatMessageStoreFactory,
		Func<JsonElement> func)
		: base(logger, postgresChatMessageStoreFactory, semanticKernelBuilder, func)
	{
	}

	protected override (ChatClientAgent agent, AgentThread thread) 
		CreateAgent(ChatClientAgentOptions options, Func<JsonElement> func) 
	{
		var chatClient = _semanticKernelBuilder.Build(Options.LLMProviderType.AzureOpenAI);

		var userInfo = new UserInfo
		{
			Name = "Vu Ngo",
			PreferredLanguage = "English",
			Interests = new List<string> { "technology", "travel", "music" },
			ConnectionTool = new ConnectionTool
			{
				PluginName = "ichibaApi",
				//Endpoint = "http://localhost:8050/swagger/bff-v1.0/swagger.json",
				Endpoint = "http://localhost:1234/sse",
				ToolType = ConnectionToolType.MCP,
			}
		};

		options.AIContextProviderFactory = (_) => new UserMemoryProvider(chatClient, userInfo);

		var agent = new ChatClientAgent(chatClient, options);

		var store = _postgresChatMessageStoreFactory.Create(func());

		var thread = agent.GetNewThread(store);

		return (agent, thread);
	}
}
