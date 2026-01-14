using Microsoft.Extensions.VectorData;

namespace Agent.Core.Abstractions.Persistents;

public abstract class QdrantRecordBase
{
	[VectorStoreKey]
	public ulong Id { get; set; } = (ulong)DateTime.UtcNow.Ticks;

	[VectorStoreData(StorageName = "created_at")]
	public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

	public abstract string GetTextToEmbed();
}

public interface IVectorRecord
{
	ReadOnlyMemory<float>? Embedding { get; set; }
}

public interface IQdrantRepository<TRecord> where TRecord : QdrantRecordBase
{
	Task UpsertAsync(TRecord record, CancellationToken cancellationToken = default);

	Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

	Task<IEnumerable<TRecord>> SearchAsync(
		string query,
		int top = 5,
		CancellationToken cancellationToken = default);

	Task<IEnumerable<TRecord>> SearchAsync(
		string query,
		int top = 5,
		VectorSearchOptions<TRecord>? options = null,
		CancellationToken cancellationToken = default);

	Task<TRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
