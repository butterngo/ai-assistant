using Agent.Core.Abstractions.Persistents;
using Agent.Core.Abstractions.Services;
using Agent.Core.VectorRecords;
using Microsoft.Extensions.VectorData;

namespace Agent.Core.Implementations.Services;

public class SkillRouterService : ISkillRouterService
{
	private readonly IQdrantRepository<SkillRoutingRecord> _qdrantRepository;

	public SkillRouterService(IQdrantRepository<SkillRoutingRecord> qdrantRepository)
	{
		_qdrantRepository = qdrantRepository;
	}

	public async Task<IEnumerable<SkillRoutingRecord>> GetBySkillCodeAsync(
		string skillCode,
		CancellationToken ct = default)
	{
		// Use SearchAsync with filter on skill_code
		var options = new VectorSearchOptions<SkillRoutingRecord>
		{
			Filter = x=>x.SkillCode == skillCode
		};

		// Search with empty query to get all matching records
		var results = await _qdrantRepository.SearchAsync(
			query: skillCode, // Use skillCode as query (will be filtered anyway)
			top: 100,
			similarityThreshold: 0, // No threshold - get all
			options: options,
			cancellationToken: ct);

		return results;
	}

	public async Task<SkillRoutingRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
	{
		return await _qdrantRepository.GetByIdAsync(id, ct);
	}

	public async Task<SkillRoutingRecord> CreateAsync(
			string skillCode,
			string skillName,
			string userQueries,
			CancellationToken ct = default)
	{
		var record = new SkillRoutingRecord
		{
			Id = Guid.NewGuid(),
			SkillCode = skillCode,
			SkillName = skillName,
			UserQueries = userQueries
		};

		await _qdrantRepository.UpsertAsync(record, ct);
		return record;
	}

	public async Task<SkillRoutingRecord?> UpdateAsync(
		Guid id,
		string userQueries,
		CancellationToken ct = default)
	{
		var record = await _qdrantRepository.GetByIdAsync(id, ct);
		if (record is null) return null;

		record.UserQueries = userQueries;
		record.UpdatedAt = DateTimeOffset.UtcNow;

		// Upsert will update existing record
		await _qdrantRepository.UpsertAsync(record, ct);
		return record;
	}

	public async Task DeleteAsync(Guid id, CancellationToken ct = default)
	{
		await _qdrantRepository.DeleteAsync(id, ct);
	}

	public async Task DeleteBySkillCodeAsync(string skillCode, CancellationToken ct = default)
	{
		// 1. Get all records with this skill_code
		var records = await GetBySkillCodeAsync(skillCode, ct);

		// 2. Delete each record by Id
		foreach (var record in records)
		{
			await _qdrantRepository.DeleteAsync(record.Id, ct);
		}
	}
}
