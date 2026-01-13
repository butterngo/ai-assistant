using System.Text.Json.Serialization;

namespace Agent.Core.Models;

public enum Specialist
{
	None,//General,
	ProjectManager,//Specialist for Project-Manager,
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
			"architect" => Models.Specialist.SoftwareArchitect,
			"dev" => Models.Specialist.Developer,
			_ => Models.Specialist.None
		};
	}
}
