using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Core.Entities;

[Table("agents")]
public class AgentEntity
{
	[Key]
	[Column("id")]
	public Guid Id { get; set; }

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

	[Column("description")]
	public string? Description { get; set; }


	[Column("created_at")]
	public DateTime CreatedAt { get; set; }

	[Column("updated_at")]
	public DateTime UpdatedAt { get; set; }

	// Navigation
	public ICollection<SkillEntity> Skills { get; set; } = [];

	public int SkillCount => Skills.Count;
}
