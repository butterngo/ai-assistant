using System.Text.Json;
using Agent.Core.Entities;
using Agent.Core.Extensions;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Agent.Core.Abstractions.Services;

using Agent.Core.Implementations.Persistents;

namespace Agent.Core.Services;

public class ConnectionToolService : IConnectionToolService
{
	private readonly AgentDbContext _context;
	private readonly IDiscoveredToolService _discoveredToolService;
	private readonly ILogger<ConnectionToolService> _logger;

	public ConnectionToolService(
		IDbContextFactory<AgentDbContext> dbContextFactory,
		IDiscoveredToolService discoveredToolService,
		ILogger<ConnectionToolService> logger)
	{
		_context = dbContextFactory.CreateDbContext();
		_discoveredToolService = discoveredToolService;
		_logger = logger;
	}

	public async Task<ConnectionToolEntity> CreateAsync(
		string name,
		string type,
		string? description = null,
		string? endpoint = null,
		string? command = null,
		JsonDocument? config = null,
		bool isActive = true,
		CancellationToken ct = default)
	{
		// Check if name already exists
		var existing = await _context.ConnectionTools
			.FirstOrDefaultAsync(c => c.Name == name, ct);

		if (existing != null)
		{
			throw new InvalidOperationException($"Connection tool with name '{name}' already exists");
		}

		var entity = new ConnectionToolEntity
		{
			Id = Guid.NewGuid(),
			Name = name,
			Type = type,
			Description = description,
			Endpoint = endpoint,
			Command = command,
			Config = config,
			IsActive = isActive,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		_context.ConnectionTools.Add(entity);
		await _context.SaveChangesAsync(ct);

		_logger.LogInformation("Created connection tool: {Name} ({Type})", name, type);

		return entity;
	}

	public async Task<ConnectionToolEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
	{
		return await _context.ConnectionTools
			.FirstOrDefaultAsync(c => c.Id == id, ct);
	}

	public async Task<ConnectionToolEntity?> GetByNameAsync(string name, CancellationToken ct = default)
	{
		return await _context.ConnectionTools
			.FirstOrDefaultAsync(c => c.Name == name, ct);
	}

	public async Task<IEnumerable<ConnectionToolEntity>> GetAllAsync(CancellationToken ct = default)
	{
		return await _context.ConnectionTools
			.OrderBy(c => c.Name)
			.ToListAsync(ct);
	}

	public async Task<IEnumerable<ConnectionToolEntity>> GetActiveAsync(CancellationToken ct = default)
	{
		return await _context.ConnectionTools
			.Where(c => c.IsActive)
			.OrderBy(c => c.Name)
			.ToListAsync(ct);
	}

	public async Task<IEnumerable<ConnectionToolEntity>> GetByTypeAsync(string type, CancellationToken ct = default)
	{
		return await _context.ConnectionTools
			.Where(c => c.Type == type)
			.OrderBy(c => c.Name)
			.ToListAsync(ct);
	}

	public async Task<ConnectionToolEntity> UpdateAsync(
		Guid id,
		string? name = null,
		string? type = null,
		string? description = null,
		string? endpoint = null,
		string? command = null,
		JsonDocument? config = null,
		bool? isActive = null,
		CancellationToken ct = default)
	{
		var entity = await _context.ConnectionTools.FindAsync(new object[] { id }, ct);
		if (entity == null)
		{
			throw new KeyNotFoundException($"Connection tool with ID {id} not found");
		}

		// Check for name conflict if name is being changed
		if (name != null && name != entity.Name)
		{
			var existing = await _context.ConnectionTools
				.FirstOrDefaultAsync(c => c.Name == name, ct);
			if (existing != null)
			{
				throw new InvalidOperationException($"Connection tool with name '{name}' already exists");
			}
			entity.Name = name;
		}

		if (type != null) entity.Type = type;
		if (description != null) entity.Description = description;
		if (endpoint != null) entity.Endpoint = endpoint;
		if (command != null) entity.Command = command;
		if (config != null) entity.Config = config;
		if (isActive.HasValue) entity.IsActive = isActive.Value;

		entity.UpdatedAt = DateTime.UtcNow;

		await _context.SaveChangesAsync(ct);

		_logger.LogInformation("Updated connection tool: {Id} ({Name})", id, entity.Name);

		return entity;
	}

	public async Task DeleteAsync(Guid id, CancellationToken ct = default)
	{
		var entity = await _context.ConnectionTools.FindAsync(new object[] { id }, ct);
		if (entity == null)
		{
			throw new KeyNotFoundException($"Connection tool with ID {id} not found");
		}

		_context.ConnectionTools.Remove(entity);
		await _context.SaveChangesAsync(ct);

		_logger.LogInformation("Deleted connection tool: {Id} ({Name})", id, entity.Name);
	}

	public async Task<IEnumerable<AITool>> DiscoverToolsAsync(Guid id, CancellationToken ct = default)
	{
		var entity = await GetByIdAsync(id, ct);
		if (entity == null)
		{
			throw new KeyNotFoundException($"Connection tool with ID {id} not found");
		}

		_logger.LogInformation("Discovering tools from connection: {Name}", entity.Name);

		try
		{
			var connectionTool = entity.ToConnectionTool();
			var tools = await connectionTool.GetToolsAsync(ct);

			if (tools == null || !tools.Any())
			{
				_logger.LogWarning("No tools discovered from connection: {Name}", entity.Name);
				return Enumerable.Empty<AITool>();
			}

			_logger.LogInformation("Discovered {Count} tools from connection: {Name}",
				tools.Count, entity.Name);

			return tools;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error discovering tools from connection: {Name}", entity.Name);
			throw;
		}
	}

	public async Task<IEnumerable<AITool>> GetToolsAsync(
		Guid id,
		bool useCache = true,
		CancellationToken ct = default)
	{
		if (!useCache)
		{
			// Always discover fresh
			var freshTools = await DiscoverToolsAsync(id, ct);

			// Update cache in background (don't await)
			_ = _discoveredToolService.SaveDiscoveredToolsAsync(id, freshTools, ct);

			return freshTools;
		}

		// Check cache first
		var isCacheStale = await _discoveredToolService.IsCacheStaleAsync(
			id,
			TimeSpan.FromHours(24),
			ct);

		if (isCacheStale)
		{
			_logger.LogInformation("Cache is stale for connection {Id}, refreshing...", id);

			// Discover fresh and update cache
			var tools = await DiscoverToolsAsync(id, ct);
			await _discoveredToolService.SaveDiscoveredToolsAsync(id, tools, ct);

			return tools;
		}

		// Return from cache
		var cachedTools = await _discoveredToolService.GetAvailableByConnectionAsync(id, ct);

		if (cachedTools.Any())
		{
			_logger.LogInformation("Returning {Count} cached tools for connection {Id}",
				cachedTools.Count(), id);

			return cachedTools.Select(dt =>
				JsonSerializer.Deserialize<AITool>(dt.ToolSchema.RootElement.GetRawText()))
				.Where(t => t != null)
				.Cast<AITool>()
				.ToList();
		}

		// Cache is empty, discover
		_logger.LogInformation("Cache is empty for connection {Id}, discovering...", id);
		var discoveredTools = await DiscoverToolsAsync(id, ct);
		await _discoveredToolService.SaveDiscoveredToolsAsync(id, discoveredTools, ct);

		return discoveredTools;
	}

	public async Task<bool> TestConnectionAsync(Guid id, CancellationToken ct = default)
	{
		try
		{
			var tools = await DiscoverToolsAsync(id, ct);
			return tools != null && tools.Any();
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Connection test failed for {Id}", id);
			return false;
		}
	}
}