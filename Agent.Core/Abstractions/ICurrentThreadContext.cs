using Agent.Core.VectorRecords;
using Microsoft.Extensions.AI;

namespace Agent.Core.Abstractions;

public interface ICurrentThreadContext
{
	Guid AgentId { get; set; }
	string AgentName { get; set; }
	Guid ThreadId { get; set; }
	string UserMessage { get; set; }
	float? SimilarityThreshold { get; set; }
	IEnumerable<SkillRoutingRecord> SkillRoutingRecords { get; set; }
	string Instructions { get; set; }
	IEnumerable<ChatMessage> RequestMessages { get; set; }
	IEnumerable<ChatMessage>? ResponseMessages { get; set; }
}
