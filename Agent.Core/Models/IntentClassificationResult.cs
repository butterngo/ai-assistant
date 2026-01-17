using Agent.Core.Enums;
using System.Text.Json.Serialization;

namespace Agent.Core.Models;

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
			"general" => Enums.Specialist.None,
			"pm" => Enums.Specialist.ProjectManager,
			"projectmanager" => Enums.Specialist.ProjectManager,
			"project manager" => Enums.Specialist.ProjectManager,
			"po" => Enums.Specialist.ProductOwner,
			"productowner" => Enums.Specialist.ProductOwner,
			"product owner" => Enums.Specialist.ProductOwner,
			"product" => Enums.Specialist.ProductOwner,
			"architect" =>	Enums.Specialist.SoftwareArchitect,
			"softwarearchitect" => Enums.Specialist.SoftwareArchitect,
			"software architect" =>	Enums.Specialist.SoftwareArchitect,
			"dev" =>	Enums.Specialist.Developer,
			"developer" =>	Enums.Specialist.Developer,
			"engineer" => Enums.Specialist.Developer,
			_ => Enums.Specialist.None
		};
	}
}
