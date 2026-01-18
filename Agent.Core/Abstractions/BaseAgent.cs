using Agent.Core.Abstractions.LLM;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Agent.Core.Abstractions;

public abstract class BaseAgent<T> : IAgent
	where T : class, IAgent
{
	public Guid Id { get; protected set; } = Guid.NewGuid();

	public  string Name { get; protected set; }

	protected readonly ILogger<T> _logger;

	protected readonly ISemanticKernelBuilder _semanticKernelBuilder;

	protected readonly ChatMessageStore _chatMessageStore;

	protected readonly AIContextProvider _aIContextProvider;

	protected readonly ChatClientAgent _agent;

	protected readonly AgentThread _thread;

	public BaseAgent(ILogger<T> logger,
		ChatMessageStore chatMessageStore,
		AIContextProvider aIContextProvider,
		ISemanticKernelBuilder semanticKernelBuilder) 
	{
		Name = typeof(T).Name;
		_logger = logger;
		_chatMessageStore = chatMessageStore;
		_aIContextProvider = aIContextProvider;
		_semanticKernelBuilder = semanticKernelBuilder;

		(_agent, _thread) = CreateAgent(new ChatClientAgentOptions
		{
			Name = Name,
			AIContextProviderFactory = (_) => aIContextProvider
		});
	}

	protected virtual void SetAgentOptions(IChatClient chatClient, ChatClientAgentOptions options) 
	{
		// Override in derived classes to set specific options
	}

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
