namespace Lamazon.ViewModels.Models;

public class DashboardViewModel
{
    public int TotalCustomers { get; set; }

    public int TotalOrders { get; set; }
    public decimal TotalOrdersAmount { get; set; }
    public int TotalPendingOrders { get; set; }
    public int TotalAcceptedOrders { get; set; }
    public int TotalRejectedOrders { get; set; }

    public int TotalInvoices { get; set; }
    public decimal TotalInvoicesAmount { get; set; }
    public int TotalPendingInvoices { get; set; }
    public int TotalPaidInvoices { get; set; }
    public int TotalCanceledInvoices { get; set; }

    public int TotalActiveProducts { get; set; }
    public int TotalDeletedProducts { get; set; }

    public List<UserViewModel> Administrators { get; set; } = [];
}
