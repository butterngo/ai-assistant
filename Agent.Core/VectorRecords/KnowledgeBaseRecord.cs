using Agent.Core.Abstractions.Persistents;
using Microsoft.Extensions.VectorData;

namespace Agent.Core.VectorRecords;

public class KnowledgeBaseRecord : QdrantRecordBase, IVectorRecord
{
	[VectorStoreData(IsIndexed = true, StorageName = "skill_id")]
	public required Guid SkillId { get; set; }

	[VectorStoreData(IsIndexed = true, StorageName = "content_type")]
	public required string ContentType { get; set; }  // "example", "guide", "snippet"

	[VectorStoreData(StorageName = "title")]
	public required string Title { get; set; }

	[VectorStoreData(StorageName = "content")]
	public required string Content { get; set; }

	[VectorStoreVector(1536,
	DistanceFunction = DistanceFunction.CosineSimilarity,
	IndexKind = IndexKind.Hnsw,
	StorageName = "embedding")]

	public ReadOnlyMemory<float>? Embedding { get; set; }

	public override string GetTextToEmbed()
	{
		return this.Content;
	}
}
