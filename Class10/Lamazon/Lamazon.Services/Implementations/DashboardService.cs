using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Enums;
using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.Models;

namespace Lamazon.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly IUsersService _usersService;

    public DashboardService(
        IDashboardRepository dashboardRepository,
        IUsersService usersService)
    {
        _dashboardRepository = dashboardRepository;
        _usersService = usersService;
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync(CancellationToken cancellationToken = default)
    {
        var totalCustomers = await _dashboardRepository.CountCustomersAsync(cancellationToken);
        var orders = await _dashboardRepository.GetOrderTotalsByStatusAsync(cancellationToken);
        var invoices = await _dashboardRepository.GetInvoiceTotalsByStatusAsync(cancellationToken);
        var products = await _dashboardRepository.CountProductsByStatusAsync(cancellationToken);
        var administrators = await _usersService.GetAdministratorsAsync(cancellationToken);

        return new DashboardViewModel
        {
            // Total Customers
            TotalCustomers = totalCustomers,

            // Orders
            TotalOrders = orders.Values.Sum(totals => totals.Count),
            TotalOrdersAmount = orders.GetValueOrDefault(OrderStatusEnum.Accepted)?.Amount ?? 0,
            TotalPendingOrders = orders.GetValueOrDefault(OrderStatusEnum.Pending)?.Count ?? 0,
            TotalAcceptedOrders = orders.GetValueOrDefault(OrderStatusEnum.Accepted)?.Count ?? 0,
            TotalRejectedOrders = orders.GetValueOrDefault(OrderStatusEnum.Rejected)?.Count ?? 0,

            // Invoices
            TotalInvoices = invoices.Values.Sum(totals => totals.Count),
            TotalInvoicesAmount = invoices.GetValueOrDefault(InvoiceStatusEnum.Paid)?.Amount ?? 0,
            TotalPendingInvoices = invoices.GetValueOrDefault(InvoiceStatusEnum.PendingPayment)?.Count ?? 0,
            TotalPaidInvoices = invoices.GetValueOrDefault(InvoiceStatusEnum.Paid)?.Count ?? 0,
            TotalCanceledInvoices = invoices.GetValueOrDefault(InvoiceStatusEnum.Canceled)?.Count ?? 0,

            // Products
            TotalActiveProducts = products.GetValueOrDefault(ProductStatusEnum.Active),
            TotalDeletedProducts = products.GetValueOrDefault(ProductStatusEnum.Deleted),

            // Admins
            Administrators = administrators
        };
    }
}
