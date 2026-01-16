using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Core.Entities;

[Table("categories")]
public class CategoryEntity
{
	[Key]
	[Column("id")]
	public Guid Id { get; set; }

	[Required]
	[MaxLength(100)]
	[Column("cat_code")]
	public string CatCode { get; set; } = string.Empty;

	[Required]
	[MaxLength(100)]
	[Column("name")]
	public string Name { get; set; } = string.Empty;

	[Column("description")]
	public string? Description { get; set; }

	[Column("created_at")]
	public DateTime CreatedAt { get; set; }

	[Column("updated_at")]
	public DateTime UpdatedAt { get; set; }

	// Navigation
	public ICollection<SkillEntity> Skills { get; set; } = [];
}
