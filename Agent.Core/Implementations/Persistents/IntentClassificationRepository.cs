using Agent.Core.Models;
using Microsoft.Extensions.AI;
using Agent.Core.Abstractions.LLM;
using Agent.Core.Abstractions.Persistents;
using Microsoft.SemanticKernel.Connectors.Qdrant;

namespace Agent.Core.Implementations.Persistents;

internal class IntentClassificationRepository : QdrantVectorBase<ulong, IntentClassificationRecord>, IIntentClassificationRepository
{
	public IntentClassificationRepository(QdrantVectorStore vectorStore, ISemanticKernelBuilder semanticKernelBuilder) 
		: this(vectorStore, semanticKernelBuilder, "intent_classifications")
	{ }

	public IntentClassificationRepository(QdrantVectorStore vectorStore, ISemanticKernelBuilder semanticKernelBuilder, string vectorName) 
		: base(vectorStore, semanticKernelBuilder, vectorName)
	{
	}

	protected override double ScoreThreshold => 0.85;


	protected override async Task<ReadOnlyMemory<float>> GenerateVectorAsync(IntentClassificationRecord record, CancellationToken cancellationToken)
	{
		var embedding = await _embeddingGenerator.GenerateVectorAsync(record.UserMessage, cancellationToken: cancellationToken);

		return embedding;
	}
}
