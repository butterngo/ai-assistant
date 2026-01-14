using System.Text.Json;
using Agent.Core.Models;
using Microsoft.Extensions.VectorData;
using Agent.Core.Abstractions.Persistents;

namespace Agent.Core.VectorRecords;

public class IntentClassificationRecord : QdrantRecordBase, IVectorRecord
{
	public static IntentClassificationRecord Create(string userMessage, IntentClassificationResult intentClassificationResult)
	{
		return new IntentClassificationRecord
		{
			UserMessage = userMessage,
			Reason = intentClassificationResult.Reason,
			Payload = JsonSerializer.Serialize(intentClassificationResult)
		};
	}

	[VectorStoreData(IsIndexed = true, StorageName = "user_message")]
	public required string UserMessage { get; set; }

	[VectorStoreData(IsIndexed = true, StorageName = "reason")]
	public required string Reason { get; set; }

	[VectorStoreData(StorageName = "payload")]
	public string Payload { get; set; } = string.Empty;

	[VectorStoreVector(1536,
		DistanceFunction = DistanceFunction.CosineSimilarity,
		IndexKind = IndexKind.Hnsw,
		StorageName = "embedding")]

	public ReadOnlyMemory<float>? Embedding { get; set; }

	public IntentClassificationResult GetClassificationResult()
	{
		if (string.IsNullOrWhiteSpace(Payload))
		{
			return new IntentClassificationResult();
		}
		return JsonSerializer.Deserialize<IntentClassificationResult>(Payload)
			?? new IntentClassificationResult();
	}

	public override string GetTextToEmbed()
	{
		return this.UserMessage;
	}
}
