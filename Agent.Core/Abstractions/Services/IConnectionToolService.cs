using System.Text.Json;
using Agent.Core.Entities;
using Microsoft.Extensions.AI;

namespace Agent.Core.Abstractions.Services;

/// <summary>
/// Service for managing connection tools (MCP servers, OpenAPI endpoints)
/// </summary>
public interface IConnectionToolService
{
	/// <summary>
	/// Create a new connection tool
	/// </summary>
	Task<ConnectionToolEntity> CreateAsync(
		string name,
		string type,
		string? description = null,
		string? endpoint = null,
		string? command = null,
		JsonDocument? config = null,
		bool isActive = true,
		CancellationToken ct = default);

	/// <summary>
	/// Get connection tool by ID
	/// </summary>
	Task<ConnectionToolEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);

	/// <summary>
	/// Get connection tool by name
	/// </summary>
	Task<ConnectionToolEntity?> GetByNameAsync(string name, CancellationToken ct = default);

	/// <summary>
	/// Get all connection tools
	/// </summary>
	Task<IEnumerable<ConnectionToolEntity>> GetAllAsync(CancellationToken ct = default);

	/// <summary>
	/// Get all active connection tools
	/// </summary>
	Task<IEnumerable<ConnectionToolEntity>> GetActiveAsync(CancellationToken ct = default);

	/// <summary>
	/// Get connection tools by type (e.g., 'mcp_stdio', 'openapi')
	/// </summary>
	Task<IEnumerable<ConnectionToolEntity>> GetByTypeAsync(string type, CancellationToken ct = default);

	/// <summary>
	/// Update an existing connection tool
	/// </summary>
	Task<ConnectionToolEntity> UpdateAsync(
		Guid id,
		string? name = null,
		string? type = null,
		string? description = null,
		string? endpoint = null,
		string? command = null,
		JsonDocument? config = null,
		bool? isActive = null,
		CancellationToken ct = default);

	/// <summary>
	/// Delete a connection tool
	/// </summary>
	Task DeleteAsync(Guid id, CancellationToken ct = default);

	/// <summary>
	/// Get AI tools for a connection (uses cache if available, otherwise discovers)
	/// </summary>
	Task<IEnumerable<AITool>> GetToolsAsync(Guid id, bool useCache = true, CancellationToken ct = default);

	/// <summary>
	/// Test connection to verify it's working
	/// </summary>
	Task<bool> TestConnectionAsync(Guid id, CancellationToken ct = default);
}