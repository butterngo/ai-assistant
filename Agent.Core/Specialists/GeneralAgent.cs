using Agent.Core.Abstractions;
using Agent.Core.Implementations;
using Agent.Core.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Agent.Core.Specialists;

public class GeneralAgent : BaseAgent<GeneralAgent>
{
	
	private ChatClientAgent? _agent;
	
	private AgentThread _thread;

	private readonly PostgresChatMessageStoreFactory _postgresChatMessageStoreFactory;

	public GeneralAgent(ILogger<GeneralAgent> logger,
		IConnectionMultiplexer redis,
		ISemanticKernelBuilder semanticKernelBuilder,
		PostgresChatMessageStoreFactory postgresChatMessageStoreFactory)
		: base(logger, redis, semanticKernelBuilder)
	{
		_postgresChatMessageStoreFactory = postgresChatMessageStoreFactory;
	}

	private ChatClientAgent CreateAgent() 
	{
		if (_agent == null)
		{
			var chatClient = _semanticKernelBuilder.Build(Core.Options.LLMProviderType.AzureOpenAI);

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

			Options.AIContextProviderFactory = (_) => new UserMemoryProvider(chatClient, userInfo);

			_agent = new ChatClientAgent(chatClient, Options);

			//var store = new RedisChatMessageStore(redis: _redis, threadId: ConversationId, maxMessages: 100);

			var store = _postgresChatMessageStoreFactory.Create();

			_thread = _agent.GetNewThread(store);
		}
		
		return _agent;
	}

	public override AIAgent GetAIAgent() => _agent;

	public override Task<AgentRunResponse> RunAsync(string userMessage, CancellationToken cancellationToken = default)
	{
		var agent = CreateAgent();

		return agent.RunAsync(message: userMessage,
			thread: _thread,
			cancellationToken: cancellationToken);
	}

	public override IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(string userMessage, CancellationToken cancellationToken = default)
	{
		var agent = CreateAgent();

		return agent.RunStreamingAsync(message: userMessage,
			thread: _thread,
			cancellationToken: cancellationToken);
	}
}
