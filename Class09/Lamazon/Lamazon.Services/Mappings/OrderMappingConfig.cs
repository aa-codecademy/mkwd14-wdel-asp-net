using Lamazon.Domain.Entities;
using Lamazon.Domain.Models;
using Lamazon.ViewModels.DataTable;
using Lamazon.ViewModels.Enums;
using Lamazon.ViewModels.Models;
using Mapster;

namespace Lamazon.Services.Mappings;

public class OrderMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<OrderLineItem, OrderLineItemViewModel>();

        config.NewConfig<Order, OrderViewModel>()
           .Map(dest => dest.OrderStatus, src => (OrderStatusEnum)src.OrderStatusId)
           .Map(dest => dest.OrderDate, src => src.OrderDate.ToLocalTime());

        config.NewConfig<PagedResultModel<Order>, PagedResultViewModel<OrderViewModel>>();
    }
}
