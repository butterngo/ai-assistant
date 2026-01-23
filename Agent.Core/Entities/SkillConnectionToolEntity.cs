using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agent.Core.Entities;

/// <summary>
/// Junction table: Many-to-many relationship between Skills and ConnectionTools
/// </summary>
[Table("skill_connection_tools")]
public class SkillConnectionToolEntity
{
	[Key]
	[Column("skill_id", Order = 0)]
	public Guid SkillId { get; set; }

	[Key]
	[Column("connection_tool_id", Order = 1)]
	public Guid ConnectionToolId { get; set; }

	[Column("created_at")]
	public DateTime CreatedAt { get; set; }

	// Navigation properties
	[ForeignKey(nameof(SkillId))]
	[JsonIgnore]
	public SkillEntity Skill { get; set; } = null!;

	[ForeignKey(nameof(ConnectionToolId))]
	[JsonIgnore]
	public ConnectionToolEntity ConnectionTool { get; set; } = null!;
}