using Lamazon.DataAccess.Context;
using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Constants;
using Lamazon.Domain.Enums;
using Lamazon.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Lamazon.DataAccess.Repositories.Implementations;

public class DashboardRepository : IDashboardRepository
{
    private readonly LamazonDbContext _dbContext;

    public DashboardRepository(LamazonDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CountCustomersAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users.CountAsync(user => user.RoleKey == Roles.User, cancellationToken);
    }
    public async Task<Dictionary<OrderStatusEnum, StatusTotalsModel>> GetOrderTotalsByStatusAsync(CancellationToken cancellationToken = default)
    {
        var statusGroups = _dbContext.Orders
            .GroupBy(order => order.OrderStatusId);

        // OrderStatusId = 1 : [...]
        // OrderStatusId = 2 : [...]
        // OrderStatusId = 3 : [...]

        var statusTotals = statusGroups
            .Select(group => new StatusTotalsModel
            {
                StatusId = group.Key,
                Count = group.Count(),
                Amount = group.Sum(order => order.TotalAmount)
            });

        return await statusTotals
            .ToDictionaryAsync(totals => (OrderStatusEnum)totals.StatusId, cancellationToken);
    }

    public async Task<Dictionary<InvoiceStatusEnum, StatusTotalsModel>> GetInvoiceTotalsByStatusAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Invoices
            .GroupBy(invoice => invoice.InvoiceStatusId)
            .Select(group => new StatusTotalsModel
            {
                StatusId = group.Key,
                Count = group.Count(),
                Amount = group.Sum(invoice => invoice.TotalAmount)
            })
            .ToDictionaryAsync(totals => (InvoiceStatusEnum)totals.StatusId, cancellationToken);
    }

    public async Task<Dictionary<ProductStatusEnum, int>> CountProductsByStatusAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Products
            .GroupBy(product => product.ProductStatusId)
            .Select(group => new { StatusId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(row => (ProductStatusEnum)row.StatusId, row => row.Count, cancellationToken);
    }

}
