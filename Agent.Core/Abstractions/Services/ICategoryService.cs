using Agent.Core.Entities;

namespace Agent.Core.Abstractions.Services;

public interface ICategoryService
{
	Task<CategoryEntity> CreateAsync(string name, string? description = null, CancellationToken ct = default);
	Task<CategoryEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
	Task<IEnumerable<CategoryEntity>> GetAllAsync(CancellationToken ct = default);
	Task<CategoryEntity> UpdateAsync(Guid id, string? name = null, string? description = null, CancellationToken ct = default);
	Task DeleteAsync(Guid id, CancellationToken ct = default);
}
