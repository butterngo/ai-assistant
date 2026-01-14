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
