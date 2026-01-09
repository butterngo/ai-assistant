using Agent.Core.Abstractions;
using Agent.Core.Options;
using Anthropic.SDK;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;

namespace Agent.Core.Implementations;

public class SemanticKernelBuilder(LLMProviderOptions options) : ISemanticKernelBuilder
{
	public IChatClient Build(LLMProviderType provider = LLMProviderType.AzureOpenAI)
	{
		return provider switch
		{
			LLMProviderType.AzureOpenAI => CreateAzureOpenAIClient(),
			LLMProviderType.Anthropic => CreateAnthropic(),
			_ => throw new NotSupportedException($"LLM Provider '{provider}' is not supported.")
		};
	}

	private IChatClient CreateAzureOpenAIClient()
	{
		var provider = options.AzureOpenAI;

		var chatClient = new AzureOpenAIClient(
		  new Uri(provider.Endpoint),
		  new AzureCliCredential())
			.GetChatClient(provider.ModelId);


		return chatClient.AsIChatClient();
	}

	private IChatClient CreateAnthropic() 
	{
		var provider = options.Anthropic;

		var anthropicClient = new AnthropicClient(provider.ApiKey);

		var chatClient = anthropicClient.Messages
			.AsBuilder()
			.ConfigureOptions(options =>
			{
				options.ModelId = provider.ModelId;
			})
			//.UseFunctionInvocation()
			.Build();

		return chatClient;
	}
}
