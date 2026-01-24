using System.Text.Json;
using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Core.Entities;

/// <summary>
/// Represents a connection to an external tool source (MCP server or OpenAPI endpoint)
/// </summary>
[Table("connection_tools")]
public class ConnectionToolEntity
{
	[Key]
	[Column("id")]
	public Guid Id { get; set; }

	/// <summary>
	/// Unique identifier for the connection (e.g., "github", "azure-devops")
	/// </summary>
	[Required]
	[MaxLength(100)]
	[Column("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Type of connection: 'mcp_http', 'mcp_stdio', 'openapi'
	/// </summary>
	[Required]
	[MaxLength(50)]
	[Column("type")]
	public string Type { get; set; } = string.Empty;

	[Column("description")]
	public string? Description { get; set; }

	/// <summary>
	/// HTTP endpoint (for MCP_HTTP and OpenAPI)
	/// </summary>
	[MaxLength(500)]
	[Column("endpoint")]
	public string? Endpoint { get; set; }

	/// <summary>
	/// Command to execute (for MCP_STDIO, e.g., "npx")
	/// </summary>
	[MaxLength(500)]
	[Column("command")]
	public string? Command { get; set; }

	/// <summary>
	/// JSON configuration containing arguments, environment variables, etc.
	/// </summary>
	[Column("config", TypeName = "jsonb")]
	public JsonDocument? Config { get; set; }

	/// <summary>
	/// Whether this connection is currently active
	/// </summary>
	[Column("is_active")]
	public bool IsActive { get; set; } = true;

	[Column("created_at")]
	public DateTime CreatedAt { get; set; }

	[Column("updated_at")]
	public DateTime UpdatedAt { get; set; }

	//// Navigation properties
	//[JsonIgnore]
	//public ICollection<SkillConnectionToolEntity> SkillConnectionTools { get; set; }
	//	= new List<SkillConnectionToolEntity>();

	//[JsonIgnore]
	//public ICollection<DiscoveredToolEntity> DiscoveredTools { get; set; }
	//	= new List<DiscoveredToolEntity>();
}