using Agent.Core.Options;
using Microsoft.Extensions.AI;

namespace Agent.Core.Abstractions.LLM;

public interface ISemanticKernelBuilder
{
	IChatClient Build(LLMProviderType provider = LLMProviderType.AzureOpenAI);

	IEmbeddingGenerator<string, Embedding<float>> GetEmbeddingGenerator(LLMProviderType provider = LLMProviderType.AzureOpenAI);
}
