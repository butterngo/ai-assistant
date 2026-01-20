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
	public DbSet<AgentEntity> Agents => Set<AgentEntity>();
	public DbSet<SkillEntity> Skills => Set<SkillEntity>();
	public DbSet<ToolEntity> Tools => Set<ToolEntity>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);
	}
}
