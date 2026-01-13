using Agent.Core.Abstractions.LLM;
using Agent.Core.Options;
using Anthropic.SDK;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;

namespace Agent.Core.Implementations.LLM;

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

	public IEmbeddingGenerator<string, Embedding<float>> GetEmbeddingGenerator(LLMProviderType provider = LLMProviderType.AzureOpenAI)
	{

		var azureClient = new AzureOpenAIClient(
		   new Uri(options.AzureOpenAI.Endpoint),
		   new Azure.AzureKeyCredential(options.AzureOpenAI.ApiKey)
		);
		
		var embeddingClient = azureClient
			.GetEmbeddingClient("text-embedding-3-small")
			.AsIEmbeddingGenerator();
		
		return embeddingClient;
	}
}
