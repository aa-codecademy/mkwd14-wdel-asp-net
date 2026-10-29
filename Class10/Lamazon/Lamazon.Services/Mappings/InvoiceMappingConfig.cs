using Lamazon.Domain.Entities;
using Lamazon.Domain.Enums;
using Mapster;

namespace Lamazon.Services.Mappings;

public class InvoiceMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Order, Invoice>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.User)
            .Map(dest => dest.OrderId, src => src.Id)
            .Map(dest => dest.InvoiceDate, src => DateTime.UtcNow)
            .Map(dest => dest.InvoiceStatusId, src => (int)InvoiceStatusEnum.PendingPayment)
            .Map(dest => dest.InvoiceLineItems, src => src.OrderLineItems);

        config.NewConfig<OrderLineItem, InvoiceLineItem>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.Product)
            .Map(dest => dest.OrderLineItemId, src => src.Id);
    }
}
