using System.Linq.Expressions;
using Lamazon.DataAccess.Context;
using Lamazon.DataAccess.Extensions;
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

    private static readonly Dictionary<string, Expression<Func<Order, object>>> _sortableColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["OrderNumber"] = order => order.Id,
        ["User.FullName"] = order => order.User.FullName,
        ["CountryCode"] = order => order.CountryCode!,
        ["TotalAmount"] = order => order.TotalAmount,
        ["OrderStatus"] = order => order.OrderStatusId,
    };

    public async Task<Order?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Table
          .Include(order => order.OrderLineItems)
          .Include(order => order.User)
          .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
    }

    public async Task<PagedResultModel<Order>> GetFilteredAsync(
        PagedQueryModel query,
        CancellationToken cancellationToken = default)
    {
        var orders = Table
            .AsNoTracking()
            .Include(order => order.User)
            .AsQueryable();

        var totalRecords = await orders.CountAsync(cancellationToken);

        var searchValue = query.SearchValue;
        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            orders = orders.Where(order => order.OrderNumber.Contains(searchValue) || order.User.FullName.Contains(searchValue));
        }

        return await orders
            .SortBy(query.SortColumn, query.IsAscending, _sortableColumns, order => order.Id)
            .ToPagedResultAsync(totalRecords, query.Start, query.Length, cancellationToken);
    }

    public async Task<int> GetMaxIdAsync(CancellationToken cancellationToken = default)
    {
        // MAX of an empty table is NULL, hence the int? and the ?? 0
        return await Table.MaxAsync(order => (int?)order.Id, cancellationToken) ?? 0;
    }
}
