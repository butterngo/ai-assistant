using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Agent.Core.Entities;

[Table("tools")]
public class ToolEntity
{
	[Key]
	[Column("id")]
	public Guid Id { get; set; }

	[Required]
	[Column("skill_id")]
	public Guid SkillId { get; set; }

	[Required]
	[MaxLength(100)]
	[Column("name")]
	public string Name { get; set; } = string.Empty;

	[Required]
	[MaxLength(50)]
	[Column("type")]
	public string Type { get; set; } = string.Empty;

	[Required]
	[MaxLength(500)]
	[Column("endpoint")]
	public string Endpoint { get; set; } = string.Empty;

	[Column("description")]
	public string? Description { get; set; }

	[Column("config", TypeName = "jsonb")]
	public JsonDocument? Config { get; set; }

	[Column("is_prefetch")]
	public bool IsPrefetch { get; set; }

	[Column("created_at")]
	public DateTime CreatedAt { get; set; }

	[Column("updated_at")]
	public DateTime UpdatedAt { get; set; }

	// Navigation
	[ForeignKey(nameof(SkillId))]
	public SkillEntity Skill { get; set; } = null!;
}
