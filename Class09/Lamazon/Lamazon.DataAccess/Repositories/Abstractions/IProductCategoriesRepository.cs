using Lamazon.Domain.Entities;
using Lamazon.Domain.Models;

namespace Lamazon.DataAccess.Repositories.Abstractions;

public interface IProductCategoriesRepository : IRepository<ProductCategory>
{
    Task<List<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ProductCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<PagedResultModel<ProductCategory>> GetFilteredAsync(PagedQueryModel query, CancellationToken cancellationToken);
    Task<bool> HasActiveProductsAsync(int id, CancellationToken cancellationToken = default);
}
