using Lamazon.Domain.Entities;
using Lamazon.Domain.Models;

namespace Lamazon.DataAccess.Repositories.Abstractions;

public interface IOrdersRepository : IRepository<Order>
{
    Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<PagedResultModel<Order>> GetFilteredAsync(PagedQueryModel query, CancellationToken cancellationToken = default);
    Task<int> GetMaxIdAsync(CancellationToken cancellationToken = default);
}
