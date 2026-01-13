using Agent.Core.Abstractions.Persistents;
using Microsoft.Extensions.VectorData;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Agent.Core.Models;

public enum Specialist
{
	None,
	ProductOwner,
	ProjectManager,
	SoftwareArchitect,
	Developer,
}

public class IntentClassificationRecord : IVectorRecord
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

	[VectorStoreKey] public ulong Id { get; set; } = (ulong)DateTime.UtcNow.Ticks;

	[VectorStoreData(IsIndexed = true, StorageName = "user_message")]
	public required string UserMessage { get; set; }

	[VectorStoreData(IsIndexed = true, StorageName = "reason")]
	public required string Reason { get; set; }

	[VectorStoreData(StorageName = "payload")]
	public string Payload { get; set; } = string.Empty;

	public IntentClassificationResult GetClassificationResult() 
	{
		if (string.IsNullOrWhiteSpace(Payload))
		{
			return new IntentClassificationResult();
		}
		return JsonSerializer.Deserialize<IntentClassificationResult>(Payload) 
			?? new IntentClassificationResult();
	}

	[VectorStoreVector(1536,
		DistanceFunction = DistanceFunction.CosineSimilarity,
		IndexKind = IndexKind.Hnsw,
		StorageName = "embedding")]

	public ReadOnlyMemory<float>? Embedding { get; set; }
}

public class IntentClassificationResult
{
	[JsonPropertyName("specialist")]
	public string Specialist { get; set; } = "None";

	[JsonPropertyName("reason")]
	public string Reason { get; set; } = string.Empty;

	[JsonPropertyName("confidence")]
	public double Confidence { get; set; }

	public Specialist GetSpecialist()
	{
		if (Enum.TryParse<Specialist>(Specialist, ignoreCase: true, out var specialist))
		{
			return specialist;
		}

		// Fallback mapping for common variations
		return Specialist.ToLowerInvariant() switch
		{
			"general" => Models.Specialist.None,
			"pm" => Models.Specialist.ProjectManager,
			"projectmanager" => Models.Specialist.ProjectManager,
			"project manager" => Models.Specialist.ProjectManager,
			"po" => Models.Specialist.ProductOwner,
			"productowner" => Models.Specialist.ProductOwner,
			"product owner" => Models.Specialist.ProductOwner,
			"product" => Models.Specialist.ProductOwner,
			"architect" => Models.Specialist.SoftwareArchitect,
			"softwarearchitect" => Models.Specialist.SoftwareArchitect,
			"software architect" => Models.Specialist.SoftwareArchitect,
			"dev" => Models.Specialist.Developer,
			"developer" => Models.Specialist.Developer,
			"engineer" => Models.Specialist.Developer,
			_ => Models.Specialist.None
		};
	}
}
