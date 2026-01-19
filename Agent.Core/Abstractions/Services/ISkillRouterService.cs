using Agent.Core.VectorRecords;

namespace Agent.Core.Abstractions.Services
{
	public interface ISkillRouterService
	{
		Task<IEnumerable<SkillRoutingRecord>> GetBySkillCodeAsync(string skillCode, CancellationToken ct = default);
		Task<SkillRoutingRecord?> GetByIdAsync(Guid id, CancellationToken ct = default);

		Task<SkillRoutingRecord> CreateAsync(string skillCode,
			string skillName,
			string userQueries, 
			CancellationToken ct = default);

		Task<SkillRoutingRecord?> UpdateAsync(Guid id,
			string userQueries,
			CancellationToken ct = default);

		Task DeleteAsync(Guid id, CancellationToken ct = default);
		Task DeleteBySkillCodeAsync(string skillCode, CancellationToken ct = default);
	}
}
