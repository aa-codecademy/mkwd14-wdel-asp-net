using Lamazon.Domain.Entities;
using Lamazon.Domain.Models;

namespace Lamazon.DataAccess.Repositories.Abstractions;

public interface IProductsRepository : IRepository<Product>
{
    Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Product>> GetFeaturedAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Product>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);
    Task<PagedResultModel<Product>> GetFilteredAsync(int? categoryId, PagedQueryModel query, CancellationToken cancellationToken = default);
}
