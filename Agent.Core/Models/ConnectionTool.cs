using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.OpenApi;
using ModelContextProtocol.Client;
namespace Agent.Core.Models;

public enum ConnectionToolType 
{
	MCP,
	OpenApi,
}

public class ConnectionTool
{
	public required string PluginName { get; set; }
	public required string Endpoint { get; set; }

	public required ConnectionToolType ToolType { get; set; } = ConnectionToolType.MCP;

	public async Task<IList<AITool>?> GetToolsAsync(CancellationToken cancellationToken = default) 
	{
		return ToolType switch
		{
			ConnectionToolType.MCP => await DiscoveryMcp(cancellationToken),
			ConnectionToolType.OpenApi => await DiscoveryOpenApi(cancellationToken),
			_ => throw new NotSupportedException()
		};
	}

	private async Task<IList<AITool>?> DiscoveryMcp(CancellationToken cancellationToken = default)
	{
		try
		{
			var clientTransport = new HttpClientTransport(new HttpClientTransportOptions
			{
				Name = PluginName,
				Endpoint = new Uri(Endpoint)
			});

			var client = await McpClient.CreateAsync(clientTransport);
			
			var mcpTools = await client.ListToolsAsync(cancellationToken: cancellationToken);

			return mcpTools.Cast<AITool>().ToList();
		}
		catch
		{
			return null;
		}
	}

	private async Task<IList<AITool>?> DiscoveryOpenApi(CancellationToken cancellationToken = default)
	{
		try
		{
			cancellationToken.ThrowIfCancellationRequested();

			var kernel = Kernel.CreateBuilder().Build();

			await kernel.ImportPluginFromOpenApiAsync(
			   pluginName: PluginName,
			   uri: new Uri(Endpoint),
			   executionParameters: new OpenApiFunctionExecutionParameters());

			var tools = kernel.Plugins
			.SelectMany(plugin => plugin)
			.Cast<AITool>()
			.ToList();

			return tools;
		}
		catch
		{
			return null;
		}
	}
}
