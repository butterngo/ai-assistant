using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agent.Core.Entities;

[Table("chat_messages")]
public class ChatMessageEntity
{
	[Key]
	[Column("id")]
	public Guid Id { get; set; }

	/// <summary>
	/// Groups messages belonging to the same conversation.
	/// </summary>
	[Required]
	[Column("thread_id")]
	public Guid ThreadId { get; set; }

	/// <summary>
	/// The role of the message sender (user, assistant, system, tool).
	/// </summary>
	[Required]
	[MaxLength(32)]
	[Column("role")]
	public string Role { get; set; } = string.Empty;

	/// <summary>
	/// Plain text content for quick querying/display.
	/// </summary>
	[Column("content", TypeName = "text")]
	public string Content { get; set; } = string.Empty;

	/// <summary>
	/// Full serialized ChatMessage for complete reconstruction.
	/// Stored as JSONB for efficient querying in PostgreSQL.
	/// </summary>
	[Required]
	[Column("serialized_message", TypeName = "jsonb")]
	public string SerializedMessage { get; set; } = string.Empty;

	/// <summary>
	/// Timestamp when the message was created.
	/// </summary>
	[Column("created_at")]
	public DateTimeOffset CreatedAt { get; set; }

	/// <summary>
	/// Ordering sequence within the thread.
	/// </summary>
	[Column("sequence_number")]
	public long SequenceNumber { get; set; }
}
