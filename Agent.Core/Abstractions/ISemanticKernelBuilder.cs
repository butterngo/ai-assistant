using Agent.Core.Options;
using Microsoft.Extensions.AI;

namespace Agent.Core.Abstractions;

public interface ISemanticKernelBuilder
{
	IChatClient Build(LLMProviderType provider = LLMProviderType.AzureOpenAI);
}
