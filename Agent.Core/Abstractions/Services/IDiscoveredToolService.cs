using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Agent.Core.Entities;
using Microsoft.Extensions.AI;

namespace Agent.Core.Abstractions.Services;

/// <summary>
/// Service for managing discovered/cached tools
/// </summary>
public interface IDiscoveredToolService
{
	/// <summary>
	/// Create/save a discovered tool
	/// </summary>
	Task<DiscoveredToolEntity> CreateAsync(
		Guid connectionToolId,
		string name,
		string? description = null,
		JsonDocument? toolSchema = null,
		bool isAvailable = true,
		CancellationToken ct = default);

	/// <summary>
	/// Get discovered tool by ID
	/// </summary>
	Task<DiscoveredToolEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

	/// <summary>
	/// Get discovered tool by connection and name
	/// </summary>
	Task<DiscoveredToolEntity?> GetByNameAsync(
		Guid connectionToolId,
		string name,
		CancellationToken ct = default);

	/// <summary>
	/// Get all discovered tools for a connection
	/// </summary>
	Task<IEnumerable<DiscoveredToolEntity>> GetByConnectionAsync(
		Guid connectionToolId,
		CancellationToken ct = default);

	/// <summary>
	/// Get available discovered tools for a connection (is_available = true)
	/// </summary>
	Task<IEnumerable<DiscoveredToolEntity>> GetAvailableByConnectionAsync(
		Guid connectionToolId,
		CancellationToken ct = default);

	/// <summary>
	/// Update an existing discovered tool
	/// </summary>
	Task<DiscoveredToolEntity> UpdateAsync(
		Guid id,
		string? name = null,
		string? description = null,
		JsonDocument? toolSchema = null,
		bool? isAvailable = null,
		CancellationToken ct = default);

	/// <summary>
	/// Delete a discovered tool
	/// </summary>
	Task DeleteAsync(Guid id, CancellationToken ct = default);

	/// <summary>
	/// Save/update discovered tools from AI tools list (bulk operation)
	/// </summary>
	Task SaveDiscoveredToolsAsync(
		Guid connectionToolId,
		IEnumerable<AITool> tools,
		CancellationToken ct = default);

	/// <summary>
	/// Clear all cached tools for a connection
	/// </summary>
	Task ClearCacheAsync(Guid connectionToolId, CancellationToken ct = default);

	/// <summary>
	/// Mark a tool as unavailable
	/// </summary>
	Task MarkAsUnavailableAsync(
		Guid connectionToolId,
		string toolName,
		CancellationToken ct = default);

	/// <summary>
	/// Mark a tool as available
	/// </summary>
	Task MarkAsAvailableAsync(
		Guid connectionToolId,
		string toolName,
		CancellationToken ct = default);

	/// <summary>
	/// Update last verified timestamp
	/// </summary>
	Task UpdateLastVerifiedAsync(Guid id, CancellationToken ct = default);

	/// <summary>
	/// Check if cache is stale (older than specified duration)
	/// </summary>
	Task<bool> IsCacheStaleAsync(
		Guid connectionToolId,
		TimeSpan maxAge,
		CancellationToken ct = default);
}