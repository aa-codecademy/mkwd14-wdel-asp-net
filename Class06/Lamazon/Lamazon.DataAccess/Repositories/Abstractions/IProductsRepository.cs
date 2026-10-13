using Lamazon.Domain.Entities;

namespace Lamazon.DataAccess.Repositories.Abstractions;

public interface IProductsRepository : IRepository<Product>
{
    Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<Product>> GetFeaturedAsync(CancellationToken cancellationToken = default);
    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
