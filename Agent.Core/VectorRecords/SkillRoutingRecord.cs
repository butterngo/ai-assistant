using Agent.Core.Abstractions.Persistents;
using Microsoft.Extensions.VectorData;

namespace Agent.Core.VectorRecords
{
	public class SkillRoutingRecord : QdrantRecordBase, IVectorRecord
	{
		[VectorStoreData(IsIndexed = true, StorageName = "skill_code")]
		public required string SkillCode { get; set; }

		[VectorStoreData(IsIndexed = true, StorageName = "skill_name")]
		public required string SkillName { get; set; }

		[VectorStoreData(IsIndexed = true, StorageName = "cat_code")]
		public required string CatCode { get; set; }

		[VectorStoreData(IsIndexed = true, StorageName = "category_name")]
		public required string CategoryName { get; set; }

		[VectorStoreData(StorageName = "description")]
		public string? Description { get; set; }

		[VectorStoreVector(1536,
		DistanceFunction = DistanceFunction.CosineSimilarity,
		IndexKind = IndexKind.Hnsw,
		StorageName = "embedding")]

		public ReadOnlyMemory<float>? Embedding { get; set; }

		public override string GetTextToEmbed()
		{
			return this.Description ?? this.SkillName;
		}
	}
}
