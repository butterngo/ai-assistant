using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agent.Core.Entities;

[Table("skills")]
public class SkillEntity
{
	[Key]
	[Column("id")]
	public Guid Id { get; set; }

	[Required]
	[Column("agent_id")]
	public Guid AgentId { get; set; }

	[Required]
	[MaxLength(100)]
	[Column("code")]
	public string Code { get; set; } = string.Empty;

	[Required]
	[MaxLength(100)]
	[Column("name")]
	public string Name { get; set; } = string.Empty;

	[Required]
	[Column("system_prompt")]
	public string SystemPrompt { get; set; } = string.Empty;

	[Column("created_at")]
	public DateTime CreatedAt { get; set; }

	[Column("updated_at")]
	public DateTime UpdatedAt { get; set; }

	// Navigation
	[ForeignKey(nameof(AgentId))]
	[JsonIgnore]
	public AgentEntity Agent { get; set; } = null!;

	public ICollection<ToolEntity> Tools { get; set; } = [];
}
