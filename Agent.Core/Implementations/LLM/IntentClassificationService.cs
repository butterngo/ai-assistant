using Agent.Core.Models;
using Microsoft.Agents.AI;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Abstractions.Persistents;

namespace Agent.Core.Implementations.LLM
{
	internal class IntentClassificationService(ISemanticKernelBuilder semanticKernelBuilder,
		IIntentClassificationRepository intentClassificationRepository)
		: IIntentClassificationService
	{

		public async Task<IntentClassificationResult> IntentAsync(string userMessage,
			CancellationToken cancellationToken)
		{
			var records = await intentClassificationRepository.SearchAsync(userMessage, top: 1,
				cancellationToken: cancellationToken);

			if (records.Any()) 
			{
				var record = records.First();
				var existingResult = record.GetClassificationResult();
				if (existingResult.Confidence >= 0.8)
				{
					return existingResult;
				}
			}

			var instructions = File.ReadAllText(Path.Combine("Data", "intent_classification.txt"));

			var chatClient = semanticKernelBuilder.Build(Options.LLMProviderType.AzureOpenAI);

			var agent = new ChatClientAgent(chatClient, instructions: instructions);

			var result = await agent.RunAsync(userMessage, cancellationToken: cancellationToken);

			var intentClassificationResult = result.Deserialize<IntentClassificationResult>();
			
			if (intentClassificationResult != null && intentClassificationResult.Confidence >= 0.8)
			{ 
				var record = IntentClassificationRecord.Create(userMessage, intentClassificationResult);

				await intentClassificationRepository.UpsetAsync(record, cancellationToken);
			}

			return intentClassificationResult!;
		}
	}
}
