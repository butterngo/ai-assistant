using Agent.Core.Abstractions.LLM;
using Agent.Core.Abstractions.Persistents;
using Agent.Core.VectorRecords;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.SemanticKernel.Embeddings;
using System.Linq.Expressions;
using System.Threading;

namespace Agent.Core.Implementations.Persistents.Vectors;

public class QdrantRepository<TRecord> : IQdrantRepository<TRecord>
	where TRecord : QdrantRecordBase, IVectorRecord
{
	protected readonly VectorStoreCollection<ulong, TRecord> _collection;

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

	protected virtual VectorStoreCollection<ulong, TRecord> GetCollection(VectorStore vectorStore, string vectorName)
		=> vectorStore.GetCollection<ulong, TRecord>(vectorName);

	public async Task UpsertAsync(TRecord record, CancellationToken ct = default)
	{
		record.Embedding = await GenerateVectorAsync(record, ct);

		await _collection.UpsertAsync(record, ct);
	}

	public async Task DeleteAsync(Guid id, CancellationToken ct = default)
	{
		var key = (ulong)id.GetHashCode();
		await _collection.DeleteAsync(key, cancellationToken: ct);
	}

	public async Task<IEnumerable<TRecord>> SearchAsync(
		string query,
		int top = 5,
		CancellationToken ct = default)
	{
		return await SearchAsync(query, top, options: null, ct);
	}

	public async Task<IEnumerable<TRecord>> SearchAsync(
		string query,
		int top = 5,
		VectorSearchOptions<TRecord>? options = null,
		CancellationToken ct = default)
	{
		var results = _collection.SearchAsync(query, top: top, options: options, cancellationToken: ct);

		var records = new List<TRecord>();

		await foreach (var item in results)
		{
			if (item is null) continue;

			if (item.Score >= _scoreThreshold)
			{
				records.Add(item.Record);
			}
		}

		return records;
	}

	public async Task<TRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
	{
		var key = (ulong)id.GetHashCode();
		return await _collection.GetAsync(key, cancellationToken: ct);
	}

	private Task<ReadOnlyMemory<float>> GenerateVectorAsync(TRecord record, CancellationToken cancellationToken)
	{
		return _embeddingGenerator.GenerateVectorAsync(record.GetTextToEmbed(), cancellationToken: cancellationToken);
	}
}
