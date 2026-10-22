using System.ComponentModel.DataAnnotations;
using Lamazon.ViewModels.Enums;

namespace Lamazon.ViewModels.Models;

public class OrderViewModel
{
    public int Id { get; set; }

    [Display(Name = "Order number")]
    public string OrderNumber { get; set; } = string.Empty;

    [Display(Name = "Order date")]
    public DateTime OrderDate { get; set; }

    public int UserId { get; set; }

    [Display(Name = "Customer")]
    public UserViewModel? User { get; set; }

    [Display(Name = "Total amount")]
    public decimal TotalAmount { get; set; }

    [Display(Name = "IP address")]
    public string? IpAddress { get; set; }

    [Display(Name = "Country")]
    public string? CountryCode { get; set; }

    [Display(Name = "Flag")]
    public string? CountryFlagUrl { get; set; }

    [Display(Name = "Status")]
    public OrderStatusEnum OrderStatus { get; set; }

    public string OrderStatusName => OrderStatus.ToString();

    public List<OrderLineItemViewModel> OrderLineItems { get; set; } = [];
}
