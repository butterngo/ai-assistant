using Agent.Core.Abstractions.LLM;
using Agent.Core.Implementations.Persistents;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
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

	public void SetConversationId(string conversationId)
	{
		ConversationId = conversationId;
	}

	protected readonly ChatClientAgent _agent;

	protected readonly AgentThread _thread;

	public BaseAgent(ILogger<T> logger,
		PostgresChatMessageStoreFactory postgresChatMessageStoreFactory,
		ISemanticKernelBuilder semanticKernelBuilder,
		Func<JsonElement> func) 
	{
		Id = Guid.NewGuid().ToString("N");
		Name = typeof(T).Name;
		_logger = logger;
		_postgresChatMessageStoreFactory = postgresChatMessageStoreFactory;
		_semanticKernelBuilder = semanticKernelBuilder;

		(_agent, _thread) = CreateAgent(new ChatClientAgentOptions
		{
			Name = Name
		}, func);
	}

	protected abstract (ChatClientAgent agent, AgentThread thread) CreateAgent(ChatClientAgentOptions options, Func<JsonElement> func);

	public virtual Task<AgentRunResponse> RunAsync(string userMessage, CancellationToken cancellationToken = default)
	{
		return _agent.RunAsync(message: userMessage,
			thread: _thread,
			cancellationToken: cancellationToken);
	}

	public virtual IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(string userMessage, CancellationToken cancellationToken = default)
	{
		return _agent.RunStreamingAsync(message: userMessage,
			thread: _thread,
			cancellationToken: cancellationToken);
	}
}
