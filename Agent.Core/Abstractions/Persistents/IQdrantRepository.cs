using Microsoft.Extensions.VectorData;
using System.Text.RegularExpressions;

namespace Agent.Core.Abstractions.Persistents;

public abstract class QdrantRecordBase
{
	[VectorStoreKey]
	public Guid Id { get; set; } = Guid.NewGuid();

	[VectorStoreData(StorageName = "created_at")]
	public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
	public DateTimeOffset? UpdatedAt { get; set; }

	public double? Score { get; set; }

	protected static string NormalizeText(string text)
	{
		if (string.IsNullOrEmpty(text))
			return string.Empty;

		return Regex.Replace(text, @"[\r\n\t]+", " ")  
					.Replace("  ", " ")                
					.Trim();
	}

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
		float? similarityThreshold = null,
		CancellationToken cancellationToken = default);

	Task<IEnumerable<TRecord>> SearchAsync(
		string query,
		int top = 5,
		float? similarityThreshold = null,
		VectorSearchOptions<TRecord>? options = null,
		CancellationToken cancellationToken = default);

	Task<TRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
