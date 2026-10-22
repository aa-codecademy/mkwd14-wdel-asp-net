using Lamazon.DataAccess.Context;
using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Entities;
using Lamazon.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Lamazon.DataAccess.Repositories.Implementations;

public class OrdersRepository : BaseRepository<Order>, IOrdersRepository
{
    public OrdersRepository(LamazonDbContext dbContext) : base(dbContext)
    {
    }

    public Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<PagedResultModel<Order>> GetFilteredAsync(PagedQueryModel query, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public async Task<int> GetMaxIdAsync(CancellationToken cancellationToken = default)
    {
        // MAX of an empty table is NULL, hence the int? and the ?? 0
        return await Table.MaxAsync(order => (int?)order.Id, cancellationToken) ?? 0;
    }
}
