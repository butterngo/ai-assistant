using Agent.Core.Models;
using Microsoft.Extensions.VectorData;

namespace Agent.Core.Abstractions.Persistents;

public interface IVectorRecord
{
	ReadOnlyMemory<float>? Embedding { get; set; }
}

public interface IVectorDBRepository<TKey, TRecord>
	where TKey : notnull
	where TRecord : class, IVectorRecord
{
	Task UpsetAsync(TRecord record, CancellationToken cancellationToken);

	Task<IEnumerable<TRecord>> SearchAsync(string query, int top = 5,
		VectorSearchOptions<TRecord>? options = null,
		CancellationToken cancellationToken = default);
}


public interface IIntentClassificationRepository : IVectorDBRepository<ulong, IntentClassificationRecord> { }