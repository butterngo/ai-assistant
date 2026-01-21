using Agent.Core.Abstractions;
using Agent.Core.VectorRecords;
using Microsoft.Extensions.AI;

namespace Agent.Core.Models;

internal class CurrentThreadContext : ICurrentThreadContext
{
	private Guid _agentId;
	private string _agentName = string.Empty;
	private Guid _threadId;
	private string _userMessage = string.Empty;
	private float? _similarityThreshold;
	private IEnumerable<SkillRoutingRecord> _skillRoutingRecords = new List<SkillRoutingRecord>();
	private string _instructions = string.Empty;
	private IEnumerable<ChatMessage> _requestMessages = new List<ChatMessage>();
	private IEnumerable<ChatMessage>? _responseMessages = new List<ChatMessage>();

	public CurrentThreadContext(Guid agentId, Guid threadId)
	{
		_agentId = agentId;
		_threadId = threadId;
		UpdatedAt = DateTime.UtcNow;
	}

	public Guid AgentId
	{
		get => _agentId;
		set { _agentId = value; UpdateTimestamp(); }
	}

	public string AgentName
	{
		get => _agentName;
		set { _agentName = value; UpdateTimestamp(); }
	}

	public Guid ThreadId
	{
		get => _threadId;
		set { _threadId = value; UpdateTimestamp(); }
	}

	public string UserMessage
	{
		get => _userMessage;
		set { _userMessage = value; UpdateTimestamp(); }
	}

	public float? SimilarityThreshold
	{
		get => _similarityThreshold;
		set { _similarityThreshold = value; UpdateTimestamp(); }
	}

	public IEnumerable<SkillRoutingRecord> SkillRoutingRecords
	{
		get => _skillRoutingRecords;
		set { _skillRoutingRecords = value; UpdateTimestamp(); }
	}

	public string Instructions
	{
		get => _instructions;
		set { _instructions = value; UpdateTimestamp(); }
	}

	public IEnumerable<ChatMessage> RequestMessages
	{
		get => _requestMessages;
		set { _requestMessages = value; UpdateTimestamp(); }
	}

	public IEnumerable<ChatMessage>? ResponseMessages
	{
		get => _responseMessages;
		set { _responseMessages = value; UpdateTimestamp(); }
	}

	public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;

	private void UpdateTimestamp()
	{
		UpdatedAt = DateTime.UtcNow;
	}
}