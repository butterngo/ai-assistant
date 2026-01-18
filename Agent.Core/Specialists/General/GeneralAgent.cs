using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;

namespace Agent.Core.Specialists;

public sealed class GeneralAgent : BaseAgent<GeneralAgent>
{
	public GeneralAgent(ILogger<GeneralAgent> logger,
		ISemanticKernelBuilder semanticKernelBuilder,
		ChatMessageStore chatMessageStore,
		AIContextProvider aIContextProvider)
		: base(logger, chatMessageStore, aIContextProvider, semanticKernelBuilder)
	{
		Id = Guid.Parse("00000000-0000-0000-0000-000000000001");
	}

	protected override void SetAgentOptions(IChatClient chatClient, ChatClientAgentOptions options)
	{
		var userInfo = new UserInfo
		{
			Name = "Vu Ngo",
			PreferredLanguage = "English",
			Interests = new List<string> { "technology", "travel", "music" },
			ConnectionTool = new ConnectionTool
			{
				PluginName = "ichibaApi",
				//Endpoint = "http://localhost:8050/swagger/bff-v1.0/swagger.json",
				Endpoint = "http://localhost:1234/sse",
				ToolType = ConnectionToolType.MCP,
			}
		};

		options.AIContextProviderFactory = (_) => _aIContextProvider;
	}
}
