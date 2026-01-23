using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Agent.Core.Entities;

/// <summary>
/// Cached AI tools discovered from MCP/OpenAPI connections
/// </summary>
[Table("discovered_tools")]
public class DiscoveredToolEntity
{
	[Key]
	[Column("id")]
	public Guid Id { get; set; }

	[Required]
	[Column("connection_tool_id")]
	public Guid ConnectionToolId { get; set; }

	/// <summary>
	/// Tool name (e.g., "create_issue", "core_list_projects")
	/// </summary>
	[Required]
	[MaxLength(200)]
	[Column("name")]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Tool description
	/// </summary>
	[Column("description")]
	public string? Description { get; set; }

	/// <summary>
	/// Full AITool definition as JSON
	/// </summary>
	[Required]
	[Column("tool_schema", TypeName = "jsonb")]
	public JsonDocument ToolSchema { get; set; } = null!;

	/// <summary>
	/// When the tool was first discovered
	/// </summary>
	[Column("discovered_at")]
	public DateTime DiscoveredAt { get; set; }

	/// <summary>
	/// Last time the tool was verified to still exist
	/// </summary>
	[Column("last_verified_at")]
	public DateTime LastVerifiedAt { get; set; }

	/// <summary>
	/// Whether the tool is currently available
	/// </summary>
	[Column("is_available")]
	public bool IsAvailable { get; set; } = true;

	// Navigation properties
	[ForeignKey(nameof(ConnectionToolId))]
	[JsonIgnore]
	public ConnectionToolEntity ConnectionTool { get; set; } = null!;
}