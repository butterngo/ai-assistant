using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Implementations.Persistents;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Agent.Core.Implementations;

internal class AgentBuilder
{
	private ILogger? _logger;
	private ISemanticKernelBuilder? _kernelBuilder;
	private PostgresChatMessageStoreFactory? _messageStoreFactory;
	private Func<JsonElement>? _metadataFactory;
	private Guid? _threadId;

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

	public AgentBuilder WithMessageStore(PostgresChatMessageStoreFactory factory)
	{
		_messageStoreFactory = factory;
		return this;
	}

	public AgentBuilder WithThreadId(Guid threadId)
	{
		_threadId = threadId;
		_metadataFactory = () => JsonSerializer.SerializeToElement(new { threadId });
		return this;
	}

	public TAgent Build<TAgent>() where TAgent : IAgent
	{
		if (_logger == null || _kernelBuilder == null ||
			_messageStoreFactory == null || _metadataFactory == null)
		{
			throw new InvalidOperationException("Agent builder not fully configured");
		}

		return (TAgent)Activator.CreateInstance(
			typeof(TAgent),
			_logger,
			_kernelBuilder,
			_messageStoreFactory,
			_metadataFactory)!;
	}
}
