using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Agent.Core.Abstractions;

public abstract class BaseAgent<T> : IAgent
	where T : class, IAgent
{
	public string Id { get; protected set; }

	public  string Name { get; protected set; }

	protected readonly ILogger<T> _logger;

	protected readonly ISemanticKernelBuilder _semanticKernelBuilder;

	protected readonly IConnectionMultiplexer _redis;
	protected string ConversationId { get; set; }

	protected ChatClientAgentOptions Options { get; set; }

	public abstract AIAgent GetAIAgent();

	public BaseAgent(ILogger<T> logger,
		IConnectionMultiplexer redis,
		ISemanticKernelBuilder semanticKernelBuilder) 
	{
		Id = Guid.NewGuid().ToString("N");
		Name = typeof(T).Name;
		_logger = logger;
		_redis = redis;
		_semanticKernelBuilder = semanticKernelBuilder;
		Options = new ChatClientAgentOptions
		{
			Name = Name
		};
	}

	public abstract IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(string userMessage,
		CancellationToken cancellationToken = default);

	public abstract Task<AgentRunResponse> RunAsync(string userMessage,
		CancellationToken cancellationToken = default);

	public void SetConversationId(string conversationId)
	{
		ConversationId = conversationId;
	}
}
