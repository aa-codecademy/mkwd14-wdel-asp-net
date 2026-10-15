using Lamazon.Domain.Entities;

namespace Lamazon.DataAccess.Repositories.Abstractions;

public interface IOrdersRepository : IRepository<Order>
{
    Task<int> GetMaxIdAsync(CancellationToken cancellationToken = default);
}
