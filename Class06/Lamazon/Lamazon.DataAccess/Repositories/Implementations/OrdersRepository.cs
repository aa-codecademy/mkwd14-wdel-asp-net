using Lamazon.DataAccess.Context;
using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Entities;

namespace Lamazon.DataAccess.Repositories.Implementations;

public class OrdersRepository : BaseRepository<Order>, IOrdersRepository
{
    public OrdersRepository(LamazonDbContext dbContext) : base(dbContext)
    {
    }
}
