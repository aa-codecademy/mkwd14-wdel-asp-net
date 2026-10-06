using Lamazon.Domain.Entities;

namespace Lamazon.DataAccess.Repositories.Abstractions;

public interface IProductCategoriesRepository : IRepository<ProductCategory>
{
    Task<List<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
