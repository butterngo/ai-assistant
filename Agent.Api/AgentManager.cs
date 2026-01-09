using Agent.Core.Abstractions;
using Agent.Core.Specialists;
using Microsoft.Agents.AI;
using StackExchange.Redis;
using System.Collections.Concurrent;

namespace Agent.Api;

public class AgentManager
{
	private readonly IConnectionMultiplexer _redis;
	private readonly ISemanticKernelBuilder _kernelBuilder;
	private readonly ConcurrentDictionary<string, GeneralAgent> _agents = new();

	public AgentManager(IConnectionMultiplexer redis, ISemanticKernelBuilder kernelBuilder)
	{
		_redis = redis;
		_kernelBuilder = kernelBuilder;
	}

	public AIAgent GetAIAgent()
	{
		var agent = new GeneralAgent(null, _redis, _kernelBuilder);

		return agent.GetAIAgent();
	}

	public GeneralAgent GetOrCreate(string conversationId)
	{
		return _agents.GetOrAdd(conversationId, id =>
		{
			var agent = new GeneralAgent(null, _redis, _kernelBuilder);
			agent.SetConversationId(id);
			return agent;
		});
	}

	public void Remove(string conversationId)
	{
		_agents.TryRemove(conversationId, out _);
	}
}
