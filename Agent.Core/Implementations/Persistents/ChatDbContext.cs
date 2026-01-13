using Agent.Core.Domains;
using Microsoft.EntityFrameworkCore;

namespace Agent.Core.Implementations.Persistents;

public class ChatDbContext : DbContext
{
	public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
	{
	}

	public DbSet<ChatMessageEntity> ChatMessages => Set<ChatMessageEntity>();
	public DbSet<ChatThreadEntity> ChatThreads => Set<ChatThreadEntity>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Configure ChatMessageEntity
		modelBuilder.Entity<ChatMessageEntity>(entity =>
		{
			// Composite index for efficient thread-based queries
			entity.HasIndex(e => new { e.ThreadId, e.SequenceNumber })
				  .HasDatabaseName("ix_chat_messages_thread_sequence");

			// Index for thread lookup
			entity.HasIndex(e => e.ThreadId)
				  .HasDatabaseName("ix_chat_messages_thread_id");

			// Index for timestamp-based queries
			entity.HasIndex(e => e.CreatedAt)
				  .HasDatabaseName("ix_chat_messages_created_at");

			// Configure JSONB column for PostgreSQL
			entity.Property(e => e.SerializedMessage)
				  .HasColumnType("jsonb");
		});

		// Configure ChatThreadEntity
		modelBuilder.Entity<ChatThreadEntity>(entity =>
		{
			// Unique index on ThreadId
			entity.HasIndex(e => e.ThreadId)
				  .IsUnique()
				  .HasDatabaseName("ix_chat_threads_thread_id");

			// Index for user lookup (multi-tenant)
			entity.HasIndex(e => e.UserId)
				  .HasDatabaseName("ix_chat_threads_user_id");

			// Index for recent threads
			entity.HasIndex(e => e.UpdatedAt)
				  .HasDatabaseName("ix_chat_threads_updated_at");

			// Configure JSONB column
			entity.Property(e => e.SerializedThreadState)
				  .HasColumnType("jsonb");
		});
	}
}
