using Lamazon.Domain.Enums;
using Lamazon.Domain.Models;

namespace Lamazon.DataAccess.Repositories.Abstractions;

public interface IDashboardRepository
{
    Task<int> CountCustomersAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<OrderStatusEnum, StatusTotalsModel>> GetOrderTotalsByStatusAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<InvoiceStatusEnum, StatusTotalsModel>> GetInvoiceTotalsByStatusAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<ProductStatusEnum, int>> CountProductsByStatusAsync(CancellationToken cancellationToken = default);
}
