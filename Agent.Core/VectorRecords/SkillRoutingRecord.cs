using Agent.Core.Abstractions.Persistents;
using Microsoft.Extensions.VectorData;
using System.Text;
using System.Text.Json.Serialization;

namespace Agent.Core.VectorRecords
{
	public class SkillRoutingRecord : QdrantRecordBase, IVectorRecord
	{
		[VectorStoreData(IsIndexed = true, StorageName = "skill_code")]
		public required string SkillCode { get; set; }

		[VectorStoreData(IsIndexed = true, StorageName = "skill_name")]
		public required string SkillName { get; set; }

		[VectorStoreData(StorageName = "user_queries")]
		public required string UserQueries { get; set; }

		[VectorStoreVector(1536,
		DistanceFunction = DistanceFunction.CosineSimilarity,
		IndexKind = IndexKind.Hnsw,
		StorageName = "embedding")]
		[JsonIgnore]
		public ReadOnlyMemory<float>? Embedding { get; set; }

		public override string GetTextToEmbed()
		{	
			return NormalizeText(this.UserQueries);
		}
	}
}
