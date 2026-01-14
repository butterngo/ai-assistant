using Microsoft.Extensions.VectorData;
using Agent.Core.Abstractions.Persistents;

namespace Agent.Core.VectorRecords;

public class CachedAnswerRecord : QdrantRecordBase, IVectorRecord
{
	[VectorStoreData(IsIndexed = true, StorageName = "skill_id")]
	public required Guid SkillId { get; set; }

	[VectorStoreData(StorageName = "question")]
	public required string Question { get; set; }

	[VectorStoreData(StorageName = "answer")]
	public required string Answer { get; set; }

	[VectorStoreData(StorageName = "hit_count")]
	public int HitCount { get; set; } = 0;


	[VectorStoreVector(1536,
	DistanceFunction = DistanceFunction.CosineSimilarity,
	IndexKind = IndexKind.Hnsw,
	StorageName = "embedding")]

	public ReadOnlyMemory<float>? Embedding { get; set; }

	public override string GetTextToEmbed() => this.Question;
	
}
