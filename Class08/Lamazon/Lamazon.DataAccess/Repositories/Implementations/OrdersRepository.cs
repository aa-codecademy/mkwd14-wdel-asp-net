using Lamazon.DataAccess.Context;
using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Lamazon.DataAccess.Repositories.Implementations;

public class OrdersRepository : BaseRepository<Order>, IOrdersRepository
{
    public OrdersRepository(LamazonDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<int> GetMaxIdAsync(CancellationToken cancellationToken = default)
    {
        // MAX of an empty table is NULL, hence the int? and the ?? 0
        return await Table.MaxAsync(order => (int?)order.Id, cancellationToken) ?? 0;
    }
}
