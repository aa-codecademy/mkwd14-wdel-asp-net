using Lamazon.Domain.Entities;
using Lamazon.ViewModels.Models;
using Mapster;

namespace Lamazon.Services.Mappings;

public class OrderMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<OrderLineItem, OrderLineItemViewModel>();
    }
}
