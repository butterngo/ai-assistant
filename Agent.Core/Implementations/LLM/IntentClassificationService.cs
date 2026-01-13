using Agent.Core.Abstractions.LLM;
using Agent.Core.Models;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;

namespace Agent.Core.Implementations.LLM
{
	internal class IntentClassificationService(ISemanticKernelBuilder semanticKernelBuilder)
		: IIntentClassificationService
	{

		public async Task<IntentClassificationResult> IntentAsync(string userMessage,
			CancellationToken cancellationToken)
		{
			var instructions = File.ReadAllText(Path.Combine("Data", "intent_classification.txt"));

			var chatClient = semanticKernelBuilder.Build(Options.LLMProviderType.AzureOpenAI);

			var agent = new ChatClientAgent(chatClient, instructions: instructions);

			var result = await agent.RunAsync(userMessage, cancellationToken: cancellationToken);

			var json = result.Text.Replace("\n", "").Replace("\r", "").Trim();

			var intentClassificationResult = System.Text.Json.JsonSerializer.Deserialize<IntentClassificationResult>(json);
			
			return intentClassificationResult!;
		}
	}
}
