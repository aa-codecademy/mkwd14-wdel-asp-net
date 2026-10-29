using System.ComponentModel.DataAnnotations;
using Lamazon.ViewModels.Enums;

namespace Lamazon.ViewModels.Models;

public class InvoiceViewModel
{
    public int Id { get; set; }

    [Display(Name = "Invoice number")]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Display(Name = "Invoice date")]
    public DateTime InvoiceDate { get; set; }

    public int OrderId { get; set; }

    [Display(Name = "Customer")]
    public UserViewModel? User { get; set; }

    [Display(Name = "Status")]
    public InvoiceStatusEnum InvoiceStatus { get; set; }

    public string InvoiceStatusName => InvoiceStatus.ToString();

    [Display(Name = "Total amount")]
    public decimal TotalAmount { get; set; }

    public List<InvoiceLineItemViewModel> InvoiceLineItems { get; set; } = [];
}
