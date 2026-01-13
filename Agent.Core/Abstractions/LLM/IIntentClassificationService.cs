using Agent.Core.Models;

namespace Agent.Core.Abstractions.LLM;

public interface IIntentClassificationService
{
	public Task<IntentClassificationResult> IntentAsync(string userMessage, CancellationToken cancellationToken);
}
