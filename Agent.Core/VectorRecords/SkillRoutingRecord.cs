using Agent.Core.Abstractions.Persistents;
using Microsoft.Extensions.VectorData;
using System.Text;

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

		public string UserQueries { get; set; } = @"Do you know ecommerce domain?
    Do you have ecommerce experience?
    knowledge of ecommerce domain
    do you know fintech?
    what is your domain expertise?";

		public string Keywords { get; set; } = @"ecommerce, e-commerce, domain knowledge, industry, fintech, business context, retail, banking";

		[VectorStoreData(StorageName = "description")]
		public string? Description { get; set; }

		[VectorStoreVector(1536,
		DistanceFunction = DistanceFunction.CosineSimilarity,
		IndexKind = IndexKind.Hnsw,
		StorageName = "embedding")]

		public ReadOnlyMemory<float>? Embedding { get; set; }

		public override string GetTextToEmbed()
		{
			var sb = new StringBuilder();

			// 1. Primary Signal: The name of the skill
			sb.Append($"Skill: {this.SkillName}. ");

			if (!string.IsNullOrEmpty(this.UserQueries))
			{
				// 2. Strong Signal: The exact questions user might ask
				sb.Append($"Triggers: {this.UserQueries} ");
			}

			if (!string.IsNullOrEmpty(this.Keywords))
			{
				// 3. Keyword Signal: Dense keywords
				sb.Append($"Keywords: {this.Keywords} ");
			}

			return sb.ToString();
		}
	}
}
