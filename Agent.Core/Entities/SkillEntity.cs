using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Core.Entities;

[Table("skills")]
public class SkillEntity
{
	[Key]
	[Column("id")]
	public Guid Id { get; set; }

	[Required]
	[Column("category_id")]
	public Guid CategoryId { get; set; }

	[Required]
	[MaxLength(100)]
	[Column("skill_code")]
	public string SkillCode { get; set; } = string.Empty;

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
	[ForeignKey(nameof(CategoryId))]
	public CategoryEntity Category { get; set; } = null!;

	public ICollection<ToolEntity> Tools { get; set; } = [];
}
