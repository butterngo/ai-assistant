using Agent.Core.Abstractions.Services;
using Agent.Core.Entities;
using Agent.Core.Implementations.Persistents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Agent.Core.Services;

public class DiscoveredToolService : IDiscoveredToolService
{
	private readonly AgentDbContext _context;
	private readonly ILogger<DiscoveredToolService> _logger;

	public DiscoveredToolService(
		IDbContextFactory<AgentDbContext> dbContextFactory,
		ILogger<DiscoveredToolService> logger)
	{
		_context = dbContextFactory.CreateDbContext();
		_logger = logger;
	}

	public async Task<DiscoveredToolEntity> CreateAsync(
		Guid connectionToolId,
		string name,
		string? description = null,
		JsonDocument? toolSchema = null,
		bool isAvailable = true,
		CancellationToken ct = default)
	{
		var entity = new DiscoveredToolEntity
		{
			Id = Guid.NewGuid(),
			ConnectionToolId = connectionToolId,
			Name = name,
			Description = description,
			ToolSchema = toolSchema ?? JsonDocument.Parse("{}"),
			DiscoveredAt = DateTime.UtcNow,
			LastVerifiedAt = DateTime.UtcNow,
			IsAvailable = isAvailable
		};

		_context.DiscoveredTools.Add(entity);
		await _context.SaveChangesAsync(ct);

		return entity;
	}

	public async Task<DiscoveredToolEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
	{
		return await _context.DiscoveredTools
			.FirstOrDefaultAsync(d => d.Id == id, ct);
	}

	public async Task<DiscoveredToolEntity?> GetByNameAsync(
		Guid connectionToolId,
		string name,
		CancellationToken ct = default)
	{
		return await _context.DiscoveredTools
			.FirstOrDefaultAsync(d => d.ConnectionToolId == connectionToolId && d.Name == name, ct);
	}

	public async Task<IEnumerable<DiscoveredToolEntity>> GetByConnectionAsync(
		Guid connectionToolId,
		CancellationToken ct = default)
	{
		return await _context.DiscoveredTools
			.Where(d => d.ConnectionToolId == connectionToolId)
			.OrderBy(d => d.Name)
			.ToListAsync(ct);
	}

	public async Task<IEnumerable<DiscoveredToolEntity>> GetAvailableByConnectionAsync(
		Guid connectionToolId,
		CancellationToken ct = default)
	{
		return await _context.DiscoveredTools
			.Where(d => d.ConnectionToolId == connectionToolId && d.IsAvailable)
			.OrderBy(d => d.Name)
			.ToListAsync(ct);
	}

	public async Task<DiscoveredToolEntity> UpdateAsync(
		Guid id,
		string? name = null,
		string? description = null,
		JsonDocument? toolSchema = null,
		bool? isAvailable = null,
		CancellationToken ct = default)
	{
		var entity = await _context.DiscoveredTools.FindAsync(new object[] { id }, ct);
		if (entity == null)
		{
			throw new KeyNotFoundException($"Discovered tool with ID {id} not found");
		}

		if (name != null) entity.Name = name;
		if (description != null) entity.Description = description;
		if (toolSchema != null) entity.ToolSchema = toolSchema;
		if (isAvailable.HasValue) entity.IsAvailable = isAvailable.Value;

		await _context.SaveChangesAsync(ct);

		return entity;
	}

	public async Task DeleteAsync(Guid id, CancellationToken ct = default)
	{
		var entity = await _context.DiscoveredTools.FindAsync(new object[] { id }, ct);
		if (entity == null)
		{
			throw new KeyNotFoundException($"Discovered tool with ID {id} not found");
		}

		_context.DiscoveredTools.Remove(entity);
		await _context.SaveChangesAsync(ct);
	}

	public async Task SaveDiscoveredToolsAsync(
		Guid connectionToolId,
		IEnumerable<AITool> tools,
		CancellationToken ct = default)
	{
		if (tools == null || !tools.Any())
		{
			_logger.LogWarning("No tools to save for connection {ConnectionToolId}", connectionToolId);
			return;
		}

		_logger.LogInformation("Saving {Count} discovered tools for connection {ConnectionToolId}",
			tools.Count(), connectionToolId);

		// Clear old cache
		await ClearCacheAsync(connectionToolId, ct);

		// Save new tools
		foreach (var tool in tools)
		{
			var toolJson = JsonSerializer.Serialize(tool);
			var toolSchema = JsonDocument.Parse(toolJson);

			var entity = new DiscoveredToolEntity
			{
				Id = Guid.NewGuid(),
				ConnectionToolId = connectionToolId,
				Name = tool.Name ?? "unknown",
				Description = tool.Description,
				ToolSchema = toolSchema,
				DiscoveredAt = DateTime.UtcNow,
				LastVerifiedAt = DateTime.UtcNow,
				IsAvailable = true
			};

			_context.DiscoveredTools.Add(entity);
		}

		await _context.SaveChangesAsync(ct);

		_logger.LogInformation("Successfully saved {Count} discovered tools", tools.Count());
	}

	public async Task ClearCacheAsync(Guid connectionToolId, CancellationToken ct = default)
	{
		var tools = await _context.DiscoveredTools
			.Where(d => d.ConnectionToolId == connectionToolId)
			.ToListAsync(ct);

		if (tools.Any())
		{
			_context.DiscoveredTools.RemoveRange(tools);
			await _context.SaveChangesAsync(ct);

			_logger.LogInformation("Cleared {Count} cached tools for connection {ConnectionToolId}",
				tools.Count, connectionToolId);
		}
	}

	public async Task MarkAsUnavailableAsync(
		Guid connectionToolId,
		string toolName,
		CancellationToken ct = default)
	{
		var tool = await GetByNameAsync(connectionToolId, toolName, ct);
		if (tool != null)
		{
			tool.IsAvailable = false;
			await _context.SaveChangesAsync(ct);
		}
	}

	public async Task MarkAsAvailableAsync(
		Guid connectionToolId,
		string toolName,
		CancellationToken ct = default)
	{
		var tool = await GetByNameAsync(connectionToolId, toolName, ct);
		if (tool != null)
		{
			tool.IsAvailable = true;
			tool.LastVerifiedAt = DateTime.UtcNow;
			await _context.SaveChangesAsync(ct);
		}
	}

	public async Task UpdateLastVerifiedAsync(Guid id, CancellationToken ct = default)
	{
		var entity = await _context.DiscoveredTools.FindAsync(new object[] { id }, ct);
		if (entity != null)
		{
			entity.LastVerifiedAt = DateTime.UtcNow;
			await _context.SaveChangesAsync(ct);
		}
	}

	public async Task<bool> IsCacheStaleAsync(
		Guid connectionToolId,
		TimeSpan maxAge,
		CancellationToken ct = default)
	{
		var latestTool = await _context.DiscoveredTools
			.Where(d => d.ConnectionToolId == connectionToolId)
			.OrderByDescending(d => d.LastVerifiedAt)
			.FirstOrDefaultAsync(ct);

		if (latestTool == null)
		{
			// No cache exists
			return true;
		}

		var age = DateTime.UtcNow - latestTool.LastVerifiedAt;
		return age > maxAge;
	}
}