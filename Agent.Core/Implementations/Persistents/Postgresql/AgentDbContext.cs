using Agent.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace Agent.Core.Implementations.Persistents;

public class AgentDbContext : DbContext
{
	public AgentDbContext(DbContextOptions<AgentDbContext> options) : base(options)
	{
		AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
		AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);
	}

	public DbSet<ChatMessageEntity> ChatMessages => Set<ChatMessageEntity>();
	public DbSet<ChatThreadEntity> ChatThreads => Set<ChatThreadEntity>();
	public DbSet<AgentEntity> Agents => Set<AgentEntity>();
	public DbSet<SkillEntity> Skills => Set<SkillEntity>();
	public DbSet<ConnectionToolEntity> ConnectionTools => Set<ConnectionToolEntity>();
	public DbSet<SkillConnectionToolEntity> SkillConnectionTools => Set<SkillConnectionToolEntity>();
	public DbSet<DiscoveredToolEntity> DiscoveredTools => Set<DiscoveredToolEntity>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		// Composite key for junction table
		modelBuilder.Entity<SkillConnectionToolEntity>()
			.HasKey(e => new { e.SkillId, e.ConnectionToolId });

		//// Unique indexes
		//modelBuilder.Entity<AgentEntity>()
		//	.HasIndex(e => e.Code)
		//	.IsUnique();

		//modelBuilder.Entity<SkillEntity>()
		//	.HasIndex(e => e.Code)
		//	.IsUnique();

		//modelBuilder.Entity<SkillEntity>()
		//	.HasIndex(e => new { e.AgentId, e.Code })
		//	.IsUnique();

		//modelBuilder.Entity<ConnectionToolEntity>()
		//	.HasIndex(e => e.Name)
		//	.IsUnique();

		//modelBuilder.Entity<DiscoveredToolEntity>()
		//	.HasIndex(e => new { e.ConnectionToolId, e.Name })
		//	.IsUnique();

		//// Additional indexes for performance
		//modelBuilder.Entity<ConnectionToolEntity>()
		//	.HasIndex(e => e.Type);

		//modelBuilder.Entity<ConnectionToolEntity>()
		//	.HasIndex(e => e.IsActive);

		//modelBuilder.Entity<DiscoveredToolEntity>()
		//	.HasIndex(e => e.ConnectionToolId);

		//modelBuilder.Entity<DiscoveredToolEntity>()
		//	.HasIndex(e => e.IsAvailable);

		//modelBuilder.Entity<SkillConnectionToolEntity>()
		//	.HasIndex(e => e.SkillId);

		//modelBuilder.Entity<SkillConnectionToolEntity>()
		//	.HasIndex(e => e.ConnectionToolId);
	}
}
