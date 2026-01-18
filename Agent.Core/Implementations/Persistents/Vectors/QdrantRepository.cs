using Microsoft.Extensions.AI;
using Agent.Core.Abstractions.LLM;
using Microsoft.Extensions.VectorData;
using Agent.Core.Abstractions.Persistents;
using Microsoft.SemanticKernel.Connectors.Qdrant;

namespace Agent.Core.Implementations.Persistents.Vectors;

public class QdrantRepository<TRecord> : IQdrantRepository<TRecord>
	where TRecord : QdrantRecordBase, IVectorRecord
{
	protected readonly VectorStoreCollection<Guid, TRecord> _collection;

	protected readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

	private readonly float _scoreThreshold;

	public QdrantRepository(
		QdrantVectorStore vectorStore,
		ISemanticKernelBuilder semanticKernelBuilder,
		string vectorName,
		float scoreThreshold = 0.7f)
	{
		_collection = GetCollection(vectorStore, vectorName);

		EnsureCollectionExists();

		_scoreThreshold = scoreThreshold;

		_embeddingGenerator = semanticKernelBuilder.GetEmbeddingGenerator();

	}

	protected void EnsureCollectionExists()
	{
		_collection.EnsureCollectionExistsAsync().Wait();
	}

	protected virtual VectorStoreCollection<Guid, TRecord> GetCollection(VectorStore vectorStore, string vectorName)
		=> vectorStore.GetCollection<Guid, TRecord>(vectorName);

	public async Task UpsertAsync(TRecord record, CancellationToken ct = default)
	{
		
		record.Embedding = await GenerateVectorAsync(record, ct);

		await _collection.UpsertAsync(record, ct);
	}

	public async Task DeleteAsync(Guid id, CancellationToken ct = default)
	{
		await _collection.DeleteAsync(id, cancellationToken: ct);
	}

	public async Task<IEnumerable<TRecord>> SearchAsync(
		string query,
		int top = 5,
		float? similarityThreshold = null,
		CancellationToken ct = default)
	{
		return await SearchAsync(query, top, similarityThreshold, options: null, ct);
	}

	public async Task<IEnumerable<TRecord>> SearchAsync(
		string query,
		int top = 5,
		float? similarityThreshold = null,
		VectorSearchOptions<TRecord>? options = null,
		CancellationToken ct = default)
	{
		var results = _collection.SearchAsync(query, top: top, options: options, cancellationToken: ct);

		var records = new List<TRecord>();

		var actualThreshold = similarityThreshold ?? _scoreThreshold;

		await foreach (var item in results)
		{
			if (item is null) continue;

			item.Record.Score = item.Score;

			if (item.Score >= actualThreshold)
			{
				records.Add(item.Record);
			}
		}

		return records;
	}

	public async Task<TRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
	{
		return await _collection.GetAsync(id, cancellationToken: ct);
	}

	private Task<ReadOnlyMemory<float>> GenerateVectorAsync(TRecord record, CancellationToken cancellationToken)
	{
		return _embeddingGenerator.GenerateVectorAsync(record.GetTextToEmbed(), cancellationToken: cancellationToken);
	}
}
