using Agent.Core.Entities;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Agent.Core.Abstractions.Services;
using Agent.Core.Implementations.Persistents;

namespace Agent.Core.Services;

public class SkillConnectionToolService : ISkillConnectionToolService
{
	private readonly AgentDbContext _context;
	private readonly IConnectionToolService _connectionToolService;
	private readonly ILogger<SkillConnectionToolService> _logger;

	public SkillConnectionToolService(
		IDbContextFactory<AgentDbContext> dbContextFactory,
		IConnectionToolService connectionToolService,
		ILogger<SkillConnectionToolService> logger)
	{
		_context = dbContextFactory.CreateDbContext();
		_connectionToolService = connectionToolService;
		_logger = logger;
	}

	public async Task<SkillConnectionToolEntity> CreateAsync(
		Guid skillId,
		Guid connectionToolId,
		CancellationToken ct = default)
	{
		// Check if already exists
		var exists = await ExistsAsync(skillId, connectionToolId, ct);
		if (exists)
		{
			throw new InvalidOperationException(
				$"Skill {skillId} is already using connection tool {connectionToolId}");
		}

		var entity = new SkillConnectionToolEntity
		{
			SkillId = skillId,
			ConnectionToolId = connectionToolId,
			CreatedAt = DateTime.UtcNow
		};

		_context.SkillConnectionTools.Add(entity);
		await _context.SaveChangesAsync(ct);

		_logger.LogInformation("Linked skill {SkillId} to connection tool {ConnectionToolId}",
			skillId, connectionToolId);

		return entity;
	}

	public async Task<bool> ExistsAsync(
		Guid skillId,
		Guid connectionToolId,
		CancellationToken ct = default)
	{
		return await _context.SkillConnectionTools
			.AnyAsync(sct => sct.SkillId == skillId && sct.ConnectionToolId == connectionToolId, ct);
	}

	public async Task<IEnumerable<ConnectionToolEntity>> GetConnectionToolsBySkillAsync(
		Guid skillId,
		CancellationToken ct = default)
	{
		return await _context.SkillConnectionTools
			.Where(sct => sct.SkillId == skillId)
			.Include(sct => sct.ConnectionTool)
			.Select(sct => sct.ConnectionTool)
			.OrderBy(c => c.Name)
			.ToListAsync(ct);
	}

	public async Task<IEnumerable<ConnectionToolEntity>> GetActiveConnectionToolsBySkillAsync(
		Guid skillId,
		CancellationToken ct = default)
	{
		return await _context.SkillConnectionTools
			.Where(sct => sct.SkillId == skillId)
			.Include(sct => sct.ConnectionTool)
			.Where(sct => sct.ConnectionTool.IsActive)
			.Select(sct => sct.ConnectionTool)
			.OrderBy(c => c.Name)
			.ToListAsync(ct);
	}

	public async Task<IEnumerable<SkillEntity>> GetSkillsByConnectionToolAsync(
		Guid connectionToolId,
		CancellationToken ct = default)
	{
		return await _context.SkillConnectionTools
			.Where(sct => sct.ConnectionToolId == connectionToolId)
			.Include(sct => sct.Skill)
			.Select(sct => sct.Skill)
			.OrderBy(s => s.Name)
			.ToListAsync(ct);
	}

	public async Task DeleteAsync(
		Guid skillId,
		Guid connectionToolId,
		CancellationToken ct = default)
	{
		var entity = await _context.SkillConnectionTools
			.FirstOrDefaultAsync(sct => sct.SkillId == skillId && sct.ConnectionToolId == connectionToolId, ct);

		if (entity == null)
		{
			throw new KeyNotFoundException(
				$"Relationship between skill {skillId} and connection tool {connectionToolId} not found");
		}

		_context.SkillConnectionTools.Remove(entity);
		await _context.SaveChangesAsync(ct);

		_logger.LogInformation("Unlinked skill {SkillId} from connection tool {ConnectionToolId}",
			skillId, connectionToolId);
	}

	public async Task DeleteBySkillAsync(Guid skillId, CancellationToken ct = default)
	{
		var entities = await _context.SkillConnectionTools
			.Where(sct => sct.SkillId == skillId)
			.ToListAsync(ct);

		if (entities.Any())
		{
			_context.SkillConnectionTools.RemoveRange(entities);
			await _context.SaveChangesAsync(ct);

			_logger.LogInformation("Removed {Count} connection tools from skill {SkillId}",
				entities.Count, skillId);
		}
	}

	public async Task DeleteByConnectionToolAsync(Guid connectionToolId, CancellationToken ct = default)
	{
		var entities = await _context.SkillConnectionTools
			.Where(sct => sct.ConnectionToolId == connectionToolId)
			.ToListAsync(ct);

		if (entities.Any())
		{
			_context.SkillConnectionTools.RemoveRange(entities);
			await _context.SaveChangesAsync(ct);

			_logger.LogInformation("Removed connection tool {ConnectionToolId} from {Count} skills",
				connectionToolId, entities.Count);
		}
	}

	public async Task<IEnumerable<AITool>> GetToolsForSkillAsync(
		Guid skillId,
		bool useCache = true,
		CancellationToken ct = default)
	{
		var connections = await GetActiveConnectionToolsBySkillAsync(skillId, ct);

		if (!connections.Any())
		{
			_logger.LogWarning("No active connection tools found for skill {SkillId}", skillId);
			return Enumerable.Empty<AITool>();
		}

		var allTools = new List<AITool>();

		foreach (var connection in connections)
		{
			try
			{
				var tools = await _connectionToolService.GetToolsAsync(connection.Id, useCache, ct);
				if (tools != null && tools.Any())
				{
					allTools.AddRange(tools);
				}
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error getting tools from connection {ConnectionId} for skill {SkillId}",
					connection.Id, skillId);
				// Continue with other connections
			}
		}

		_logger.LogInformation("Retrieved {Count} total tools for skill {SkillId}",
			allTools.Count, skillId);

		return allTools;
	}

	public async Task<IEnumerable<SkillConnectionToolEntity>> BulkCreateAsync(
		Guid skillId,
		IEnumerable<Guid> connectionToolIds,
		CancellationToken ct = default)
	{
		var entities = new List<SkillConnectionToolEntity>();

		foreach (var connectionToolId in connectionToolIds)
		{
			// Check if already exists
			var exists = await ExistsAsync(skillId, connectionToolId, ct);
			if (!exists)
			{
				var entity = new SkillConnectionToolEntity
				{
					SkillId = skillId,
					ConnectionToolId = connectionToolId,
					CreatedAt = DateTime.UtcNow
				};

				entities.Add(entity);
			}
		}

		if (entities.Any())
		{
			_context.SkillConnectionTools.AddRange(entities);
			await _context.SaveChangesAsync(ct);

			_logger.LogInformation("Bulk linked {Count} connection tools to skill {SkillId}",
				entities.Count, skillId);
		}

		return entities;
	}

	public async Task SyncConnectionToolsAsync(
		Guid skillId,
		IEnumerable<Guid> connectionToolIds,
		CancellationToken ct = default)
	{
		var currentConnections = await _context.SkillConnectionTools
			.Where(sct => sct.SkillId == skillId)
			.ToListAsync(ct);

		var currentIds = currentConnections.Select(c => c.ConnectionToolId).ToHashSet();
		var newIds = connectionToolIds.ToHashSet();

		// Remove connections that are no longer in the list
		var toRemove = currentConnections
			.Where(c => !newIds.Contains(c.ConnectionToolId))
			.ToList();

		if (toRemove.Any())
		{
			_context.SkillConnectionTools.RemoveRange(toRemove);
		}

		// Add new connections
		var toAdd = newIds
			.Where(id => !currentIds.Contains(id))
			.Select(id => new SkillConnectionToolEntity
			{
				SkillId = skillId,
				ConnectionToolId = id,
				CreatedAt = DateTime.UtcNow
			})
			.ToList();

		if (toAdd.Any())
		{
			_context.SkillConnectionTools.AddRange(toAdd);
		}

		await _context.SaveChangesAsync(ct);

		_logger.LogInformation("Synced connection tools for skill {SkillId}: removed {RemovedCount}, added {AddedCount}",
			skillId, toRemove.Count, toAdd.Count);
	}
}