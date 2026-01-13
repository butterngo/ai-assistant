using Agent.Core.Abstractions.LLM;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.Qdrant;
using System.Threading;

namespace Agent.Core.Abstractions.Persistents;

public abstract class QdrantVectorBase<TKey, TRecord> : VectorDBBase<QdrantVectorStore, TKey, TRecord>
where TKey : notnull
where TRecord : class, IVectorRecord
{
	public QdrantVectorBase(QdrantVectorStore vectorStore, ISemanticKernelBuilder semanticKernelBuilder, string vectorName)
		: base(vectorStore, semanticKernelBuilder, vectorName)
	{
	}
}

public abstract class VectorDBBase<TVectorStore, TKey, TRecord> : IVectorDBRepository<TKey, TRecord>
where TVectorStore : VectorStore
where TKey : notnull
where TRecord : class, IVectorRecord

{
	protected VectorStoreCollection<TKey, TRecord> Collection { get; set; }

	protected readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

	protected abstract double ScoreThreshold { get; }

	protected readonly string _vectorName;

	public VectorDBBase(VectorStore vectorStore, ISemanticKernelBuilder semanticKernelBuilder, string vectorName)
	{
		_embeddingGenerator = semanticKernelBuilder.GetEmbeddingGenerator();
		
		Collection = GetCollection(vectorStore, vectorName);

		EnsureCollectionExists();
		_vectorName = vectorName;
	}

	protected void EnsureCollectionExists()
	{
		Collection.EnsureCollectionExistsAsync().Wait();
	}

	protected virtual VectorStoreCollection<TKey, TRecord> GetCollection(VectorStore vectorStore, string vectorName)
	=> vectorStore.GetCollection<TKey, TRecord>(vectorName);

	public async Task<IEnumerable<TRecord>> SearchAsync(string query,
		int top = 5,
		VectorSearchOptions<TRecord>? options = null,
		CancellationToken cancellationToken = default)
	{
		var results = Collection.SearchAsync(query, top: top, options: options, cancellationToken: cancellationToken);

		var records = new List<TRecord>();

		await foreach (var item in results)
		{
			if (item is null) continue;

			if (item.Score >= ScoreThreshold)
			{
				records.Add(item.Record);
			}
		}

		return records;
	}

	public async Task UpsetAsync(TRecord record, CancellationToken cancellationToken)
	{
		record.Embedding = await GenerateVectorAsync(record, cancellationToken);

		await Collection.UpsertAsync(record, cancellationToken);
	}

	protected abstract Task<ReadOnlyMemory<float>> GenerateVectorAsync(TRecord record, CancellationToken cancellationToken);
}
