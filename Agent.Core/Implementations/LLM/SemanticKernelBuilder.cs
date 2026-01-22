using Agent.Core.Abstractions;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Models;
using Agent.Core.Options;
using Anthropic.SDK;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpYaml.Serialization.Logging;
using System.ClientModel;
using System.ClientModel.Primitives;

namespace Agent.Core.Implementations.LLM;

internal class LLMLoggingPolicy : PipelinePolicy
{
	public override void Process(PipelineMessage message, IReadOnlyList<PipelinePolicy> pipeline, int currentIndex)
	{
		ProcessNext(message, pipeline, currentIndex);
	}

	public override async ValueTask ProcessAsync(
		PipelineMessage message,
		IReadOnlyList<PipelinePolicy> pipeline,
		int currentIndex)
	{
		if (message.Request?.Content != null)
		{
			var requestBody = await ReadBinaryContentAsync(message.Request.Content);
			Console.WriteLine("=== REQUEST TO LLM ===");
			Console.WriteLine(requestBody);
		}

		await ProcessNextAsync(message, pipeline, currentIndex);
	}

	private static async Task<string> ReadBinaryContentAsync(BinaryContent content)
	{
		// BinaryContent doesn't have ToStream(), we need to write to a MemoryStream
		using var memoryStream = new MemoryStream();
		await content.WriteToAsync(memoryStream, CancellationToken.None);
		memoryStream.Position = 0;

		using var reader = new StreamReader(memoryStream);
		return await reader.ReadToEndAsync();
	}
}

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

		var pipelineOptions = new AzureOpenAIClientOptions();

		// Add your custom logging policy
		pipelineOptions.AddPolicy(
			new LLMLoggingPolicy(),
			PipelinePosition.PerCall  // Runs once per request
		);

		var chatClient = new AzureOpenAIClient(
		  new Uri(provider.Endpoint),
		  new AzureCliCredential(), pipelineOptions)
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
