using Agent.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Agent.Core.Implementations.Persistents;

public class ChatDbContext : DbContext
{
	public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options)
	{
	}

	public DbSet<ChatMessageEntity> ChatMessages => Set<ChatMessageEntity>();
	public DbSet<ChatThreadEntity> ChatThreads => Set<ChatThreadEntity>();
	public DbSet<CategoryEntity> Categories => Set<CategoryEntity>();
	public DbSet<SkillEntity> Skills => Set<SkillEntity>();
	public DbSet<ToolEntity> Tools => Set<ToolEntity>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// ChatMessageEntity indexes
		modelBuilder.Entity<ChatMessageEntity>(entity =>
		{
			entity.HasIndex(e => new { e.ThreadId, e.SequenceNumber })
				  .HasDatabaseName("ix_chat_messages_thread_sequence");
			entity.HasIndex(e => e.ThreadId)
				  .HasDatabaseName("ix_chat_messages_thread_id");
			entity.HasIndex(e => e.CreatedAt)
				  .HasDatabaseName("ix_chat_messages_created_at");
		});

		// ChatThreadEntity indexes
		modelBuilder.Entity<ChatThreadEntity>(entity =>
		{
			entity.HasIndex(e => e.ThreadId)
				  .IsUnique()
				  .HasDatabaseName("ix_chat_threads_thread_id");
			entity.HasIndex(e => e.UserId)
				  .HasDatabaseName("ix_chat_threads_user_id");
			entity.HasIndex(e => e.UpdatedAt)
				  .HasDatabaseName("ix_chat_threads_updated_at");
		});

		// CategoryEntity indexes
		modelBuilder.Entity<CategoryEntity>(entity =>
		{
			entity.HasIndex(e => e.Name)
				  .IsUnique()
				  .HasDatabaseName("ix_categories_name");

			entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
			entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
		});

		// SkillEntity indexes
		modelBuilder.Entity<SkillEntity>(entity =>
		{
			entity.HasIndex(e => new { e.CategoryId, e.Name })
				  .IsUnique()
				  .HasDatabaseName("ix_skills_category_name");
			entity.HasIndex(e => e.CategoryId)
				  .HasDatabaseName("ix_skills_category_id");

			entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
			entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
		});

		// ToolEntity indexes
		modelBuilder.Entity<ToolEntity>(entity =>
		{
			entity.HasIndex(e => new { e.SkillId, e.Name })
				  .IsUnique()
				  .HasDatabaseName("ix_tools_skill_name");
			entity.HasIndex(e => e.SkillId)
				  .HasDatabaseName("ix_tools_skill_id");
			entity.HasIndex(e => e.Type)
				  .HasDatabaseName("ix_tools_type");

			entity.Property(e => e.IsPrefetch).HasDefaultValue(false);
			entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
			entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
		});
	}
}
