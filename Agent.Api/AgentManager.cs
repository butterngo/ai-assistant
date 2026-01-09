using Agent.Core.Abstractions;
using Agent.Core.Implementations;
using Agent.Core.Specialists;
using Microsoft.Agents.AI;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace Agent.Api;

public class AgentManager
{
	private readonly IConnectionMultiplexer _redis;
	private readonly ISemanticKernelBuilder _kernelBuilder;
	private readonly PostgresChatMessageStoreFactory _postgresChatMessageStoreFactory;
	private readonly ConcurrentDictionary<string, GeneralAgent> _agents = new();
	private readonly ILoggerFactory _loggerFactory;
	public AgentManager(IConnectionMultiplexer redis,
		ILoggerFactory loggerFactory,
		ISemanticKernelBuilder kernelBuilder,
		PostgresChatMessageStoreFactory postgresChatMessageStoreFactory)
	{
		_redis = redis;
		_loggerFactory = loggerFactory;
		_kernelBuilder = kernelBuilder;
		_postgresChatMessageStoreFactory = postgresChatMessageStoreFactory;
	}

	public GeneralAgent GetOrCreate(string conversationId)
	{
		return _agents.GetOrAdd(conversationId, id =>
		{
			var agent = new GeneralAgent(_loggerFactory.CreateLogger<GeneralAgent>(), _redis, _kernelBuilder, _postgresChatMessageStoreFactory);
			agent.SetConversationId(id);
			return agent;
		});
	}

	public void Remove(string conversationId)
	{
		_agents.TryRemove(conversationId, out _);
	}
}
