using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using ModelContextProtocol.Client;
using System.Text.Json;

namespace Agent.Core.Models;

/// <summary>
/// Types of tool connections supported
/// </summary>
public enum ConnectionToolType
{
	/// <summary>
	/// MCP server accessible via HTTP/SSE
	/// </summary>
	MCP_HTTP,

	/// <summary>
	/// MCP server running as a local process (stdin/stdout communication)
	/// </summary>
	MCP_STDIO,

	/// <summary>
	/// OpenAPI/REST API endpoint
	/// </summary>
	OpenApi,
}

/// <summary>
/// Represents a connection to an external tool (MCP server or OpenAPI endpoint)
/// that can dynamically discover and provide AI tools
/// </summary>
public class ConnectionTool
{
	#region Properties

	/// <summary>
	/// Unique identifier for the plugin (e.g., "github", "jira")
	/// </summary>
	public required string PluginName { get; set; }

	/// <summary>
	/// Type of connection (MCP_HTTP, MCP_STDIO, OpenApi)
	/// </summary>
	public required ConnectionToolType ToolType { get; set; }

	// --- For MCP_HTTP ---

	/// <summary>
	/// HTTP endpoint for MCP server (required for MCP_HTTP)
	/// Also used for OpenApi endpoint
	/// </summary>
	public string? Endpoint { get; set; }

	// --- For MCP_STDIO ---

	/// <summary>
	/// Command to execute for MCP stdio server (e.g., "npx", "node")
	/// Required for MCP_STDIO
	/// </summary>
	public string? Command { get; set; }

	/// <summary>
	/// Arguments to pass to the command (e.g., ["-y", "@modelcontextprotocol/server-github"])
	/// Optional for MCP_STDIO
	/// </summary>
	public IList<string>? Arguments { get; set; }

	/// <summary>
	/// Environment variables to set for the MCP server process
	/// (e.g., GITHUB_PERSONAL_ACCESS_TOKEN, API keys)
	/// Optional for MCP_STDIO
	/// </summary>
	public IDictionary<string, string?>? EnvironmentVariables { get; set; }

	/// <summary>
	/// Working directory for the MCP server process
	/// Optional for MCP_STDIO
	/// </summary>
	public string? WorkingDirectory { get; set; }

	/// <summary>
	/// Timeout for graceful shutdown of MCP server process
	/// Default is 5 seconds
	/// </summary>
	public TimeSpan ShutdownTimeout { get; set; } = TimeSpan.FromSeconds(5);

	/// <summary>
	/// Callback for MCP server stderr output (for logging/debugging)
	/// </summary>
	public Action<string>? OnStandardError { get; set; }

	#endregion

	#region Public Methods

	/// <summary>
	/// Discovers and returns all available AI tools from this connection
	/// </summary>
	/// <param name="cancellationToken">Cancellation token</param>
	/// <returns>List of discovered AI tools, or null if discovery fails</returns>
	public async Task<IList<AITool>?> GetToolsAsync(CancellationToken cancellationToken = default)
	{
		return ToolType switch
		{
			ConnectionToolType.MCP_HTTP => await DiscoveryMcpHttpAsync(cancellationToken),
			ConnectionToolType.MCP_STDIO => await DiscoveryMcpStdioAsync(cancellationToken),
			ConnectionToolType.OpenApi => await DiscoveryOpenApiAsync(cancellationToken),
			_ => throw new NotSupportedException($"ToolType '{ToolType}' is not supported")
		};
	}

	#endregion

	#region Private Methods - MCP Discovery

	/// <summary>
	/// Discovers tools from an MCP server accessible via HTTP
	/// </summary>
	private async Task<IList<AITool>?> DiscoveryMcpHttpAsync(CancellationToken cancellationToken = default)
	{
		// Validate required properties
		if (string.IsNullOrWhiteSpace(Endpoint))
		{
			throw new ArgumentNullException(nameof(Endpoint),
				"Endpoint is required for MCP_HTTP connection type");
		}

		// Create HTTP transport
		var clientTransport = new HttpClientTransport(new HttpClientTransportOptions
		{
			Name = PluginName,
			Endpoint = new Uri(Endpoint)
		});

		// Create MCP client
		var client = await McpClient.CreateAsync(clientTransport, cancellationToken: cancellationToken);

		// Discover tools from the MCP server
		var mcpTools = await client.ListToolsAsync(cancellationToken: cancellationToken);

		// Convert to AITool list
		return mcpTools?.Cast<AITool>().ToList();
	}

	/// <summary>
	/// Discovers tools from an MCP server running as a local process
	/// </summary>
	private async Task<IList<AITool>?> DiscoveryMcpStdioAsync(CancellationToken cancellationToken = default)
	{
		// Validate required properties
		if (string.IsNullOrWhiteSpace(Command))
		{
			throw new ArgumentNullException(nameof(Command),
				"Command is required for MCP_STDIO connection type");
		}

		// Create stdio transport options
		var options = new StdioClientTransportOptions
		{
			Name = PluginName,
			Command = Command,
			Arguments = Arguments,
			WorkingDirectory = WorkingDirectory,
			EnvironmentVariables = EnvironmentVariables,
			ShutdownTimeout = ShutdownTimeout,
			StandardErrorLines = OnStandardError ?? DefaultStandardErrorHandler
		};

		// Create stdio transport
		var clientTransport = new StdioClientTransport(options);

		// Create MCP client
		var client = await McpClient.CreateAsync(clientTransport, cancellationToken: cancellationToken);

		// Discover tools from the MCP server
		var mcpTools = await client.ListToolsAsync(cancellationToken: cancellationToken);

		// Convert to AITool list
		return mcpTools?.Cast<AITool>().ToList();
	}

	#endregion

	#region Private Methods - OpenAPI Discovery

	/// <summary>
	/// Discovers tools from an OpenAPI endpoint
	/// </summary>
	private async Task<IList<AITool>?> DiscoveryOpenApiAsync(CancellationToken cancellationToken = default)
	{
		// Validate required properties
		if (string.IsNullOrWhiteSpace(Endpoint))
		{
			throw new ArgumentNullException(nameof(Endpoint),
				"Endpoint is required for OpenApi connection type");
		}

		cancellationToken.ThrowIfCancellationRequested();

		// Create a temporary kernel to import the OpenAPI plugin
		var kernel = Kernel.CreateBuilder().Build();

		// Import OpenAPI plugin
		await kernel.ImportPluginFromOpenApiAsync(
			pluginName: PluginName,
			uri: new Uri(Endpoint),
			executionParameters: new OpenApiFunctionExecutionParameters());

		// Extract AI tools from the imported plugin
		var tools = kernel.Plugins
			.SelectMany(plugin => plugin)
			.Cast<AITool>()
			.ToList();

		return tools;
	}

	#endregion

	#region Helper Methods

	/// <summary>
	/// Default handler for MCP server stderr output
	/// </summary>
	private void DefaultStandardErrorHandler(string line)
	{
		if (!string.IsNullOrWhiteSpace(line))
		{
			Console.WriteLine($"[{PluginName} STDERR] {line}");
		}
	}

	/// <summary>
	/// Creates a ConnectionTool from database JSON configuration
	/// </summary>
	/// <param name="pluginName">Plugin name</param>
	/// <param name="toolType">Tool type string ("mcp", "openapi")</param>
	/// <param name="endpoint">Endpoint (for HTTP/OpenAPI)</param>
	/// <param name="configJson">JSON configuration string</param>
	/// <returns>Configured ConnectionTool instance</returns>
	public static ConnectionTool FromDatabaseConfig(
		string pluginName,
		string toolType,
		string? endpoint,
		string configJson)
	{
		var config = JsonSerializer.Deserialize<ToolConfig>(configJson)
			?? throw new InvalidOperationException("Failed to deserialize tool config");

		var connectionType = toolType.ToLower() switch
		{
			"mcp" => string.IsNullOrEmpty(config.Command)
				? ConnectionToolType.MCP_HTTP
				: ConnectionToolType.MCP_STDIO,
			"openapi" => ConnectionToolType.OpenApi,
			_ => throw new NotSupportedException($"Tool type '{toolType}' is not supported")
		};

		return new ConnectionTool
		{
			PluginName = pluginName,
			ToolType = connectionType,
			Endpoint = endpoint ?? config.Endpoint,
			Command = config.Command,
			Arguments = config.Arguments,
			EnvironmentVariables = config.EnvironmentVariables,
			WorkingDirectory = config.WorkingDirectory,
			ShutdownTimeout = config.ShutdownTimeout != null
				? TimeSpan.FromSeconds(config.ShutdownTimeout.Value)
				: TimeSpan.FromSeconds(5)
		};
	}

	#endregion
}

/// <summary>
/// Configuration model for deserializing tool config from database
/// </summary>
public class ToolConfig
{
	public string? Endpoint { get; set; }
	public string? Command { get; set; }
	public List<string>? Arguments { get; set; }
	public Dictionary<string, string?>? EnvironmentVariables { get; set; }
	public string? WorkingDirectory { get; set; }
	public int? ShutdownTimeout { get; set; }
}