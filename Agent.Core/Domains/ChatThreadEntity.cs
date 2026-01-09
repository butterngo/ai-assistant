using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Core.Domains;


[Table("chat_threads")]
public class ChatThreadEntity
{
	[Key]
	[Column("id")]
	public Guid Id { get; set; }

	[Required]
	[MaxLength(64)]
	[Column("thread_id")]
	public string ThreadId { get; set; } = string.Empty;

	[MaxLength(256)]
	[Column("title")]
	public string? Title { get; set; }

	[Column("created_at")]
	public DateTimeOffset CreatedAt { get; set; }

	[Column("updated_at")]
	public DateTimeOffset UpdatedAt { get; set; }

	/// <summary>
	/// Serialized AgentThread state for resumption.
	/// </summary>
	[Column("serialized_thread_state", TypeName = "jsonb")]
	public string? SerializedThreadState { get; set; }

	/// <summary>
	/// Optional: User identifier for multi-tenant scenarios.
	/// </summary>
	[MaxLength(128)]
	[Column("user_id")]
	public string? UserId { get; set; }
}
