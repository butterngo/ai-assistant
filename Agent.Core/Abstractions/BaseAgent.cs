using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Agent.Core.Abstractions.LLM;
using Microsoft.Extensions.Logging;

namespace Agent.Core.Abstractions;

public abstract class BaseAgent<T> : IAgent
	where T : class, IAgent
{
	public Guid Id { get; protected set; } = Guid.NewGuid();

	public  string Name { get; protected set; }

	protected readonly ILogger<T> _logger;

	protected readonly ISemanticKernelBuilder _semanticKernelBuilder;

	protected readonly ChatMessageStore _chatMessageStore;

	protected string ConversationId { get; set; }

	public void SetConversationId(string conversationId)
	{
		ConversationId = conversationId;
	}

	protected readonly ChatClientAgent _agent;

	protected readonly AgentThread _thread;

	public BaseAgent(ILogger<T> logger,
		ChatMessageStore chatMessageStore,
		ISemanticKernelBuilder semanticKernelBuilder) 
	{
		Name = typeof(T).Name;
		_logger = logger;
		_chatMessageStore = chatMessageStore;
		_semanticKernelBuilder = semanticKernelBuilder;

		(_agent, _thread) = CreateAgent(new ChatClientAgentOptions
		{
			Name = Name
		});
	}

	protected abstract void SetAgentOptions(IChatClient chatClient, ChatClientAgentOptions options);
	
	protected virtual (ChatClientAgent agent, AgentThread thread) CreateAgent(ChatClientAgentOptions options)
	{
		var chatClient = _semanticKernelBuilder.Build(Options.LLMProviderType.AzureOpenAI);

		SetAgentOptions(chatClient, options);

		var agent = new ChatClientAgent(chatClient, options);

		var thread = agent.GetNewThread(_chatMessageStore);

		return (agent, thread);
	}

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
