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
	}
}
