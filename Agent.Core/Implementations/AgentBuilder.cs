using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Implementations.LLM;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;

namespace Agent.Core.Implementations;

internal class AgentBuilder
{
	private ILogger? _logger;
	private ISemanticKernelBuilder? _kernelBuilder;
	private ChatMessageStore? _messageStore;
	private AIContextProvider? _aIContextProvider;

	public AgentBuilder WithLogger<TAgent>(ILoggerFactory loggerFactory)
	{
		_logger = loggerFactory.CreateLogger<TAgent>();
		return this;
	}

	public AgentBuilder WithKernel(ISemanticKernelBuilder kernelBuilder)
	{
		_kernelBuilder = kernelBuilder;
		return this;
	}

	public AgentBuilder WithMessageStore(ChatMessageStore messageStore)
	{
		_messageStore = messageStore;
		return this;
	}

	public AgentBuilder WithAIContextProvider(AIContextProvider aIContextProvider)
	{
		_aIContextProvider = aIContextProvider;
		return this;
	}

	public TAgent Build<TAgent>() where TAgent : IAgent
	{
		if (_logger == null || _kernelBuilder == null ||
			_messageStore == null|| _aIContextProvider == null)
		{
			throw new InvalidOperationException("Agent builder not fully configured");
		}

		return (TAgent)Activator.CreateInstance(
			typeof(TAgent),
			_logger,
			_kernelBuilder,
			_messageStore,
			_aIContextProvider)!;
	}
}
