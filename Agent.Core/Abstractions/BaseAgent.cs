using Agent.Core.Implementations;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text.Json;

namespace Agent.Core.Abstractions;

public abstract class BaseAgent<T> : IAgent
	where T : class, IAgent
{
	public string Id { get; protected set; }

	public  string Name { get; protected set; }

	protected readonly ILogger<T> _logger;

	protected readonly ISemanticKernelBuilder _semanticKernelBuilder;

	protected readonly PostgresChatMessageStoreFactory _postgresChatMessageStoreFactory;

	protected string ConversationId { get; set; }

	private JsonElement? _state;

	protected JsonElement GetState() 
	{
		if (!_state.HasValue)
		{
			_state = JsonSerializer.SerializeToElement(new { threadId = ConversationId });
		}

		return _state.Value;
	}

	protected ChatClientAgentOptions Options { get; set; }

	public BaseAgent(ILogger<T> logger,
		PostgresChatMessageStoreFactory postgresChatMessageStoreFactory,
		ISemanticKernelBuilder semanticKernelBuilder) 
	{
		Id = Guid.NewGuid().ToString("N");
		Name = typeof(T).Name;
		_logger = logger;
		_postgresChatMessageStoreFactory = postgresChatMessageStoreFactory;
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
