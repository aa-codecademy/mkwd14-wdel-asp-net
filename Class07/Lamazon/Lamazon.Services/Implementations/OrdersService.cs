using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Entities;
using Lamazon.Domain.Enums;
using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.Models;
using MapsterMapper;

namespace Lamazon.Services.Implementations;

public class OrdersService : IOrdersService
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IMapper _mapper;

    public OrdersService(
        IOrdersRepository ordersRepository,
        IProductsRepository productsRepository,
        IMapper mapper)
    {
        _ordersRepository = ordersRepository;
        _productsRepository = productsRepository;
        _mapper = mapper;
    }

    public async Task<List<OrderLineItemViewModel>> GetCartLineItemsAsync(
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken)
    {
        var products = await _productsRepository.GetByIdsAsync(productIds, cancellationToken);

        var lineItems = products.Select(product =>
        {
            var lineItem = CreateLineItem(product);

            var lineItemVM = _mapper.Map<OrderLineItemViewModel>(lineItem);

            lineItemVM.ProductDescription = product.Description;
            lineItemVM.ProductImageUrl = product.ImageUrl;
            return lineItemVM;
        });

        return lineItems.ToList();
    }

    public async Task<string> CreateAsync(
        int userId,
        IReadOnlyCollection<int> productIds,
        CancellationToken cancellationToken)
    {
        var products = await _productsRepository.GetByIdsAsync(productIds, cancellationToken);
        if (products.Count == 0)
        {
            throw new Exception();
        }

        var lineItems = products.Select(CreateLineItem).ToList();
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            OrderStatusId = (int)OrderStatusEnum.Pending,
            OrderLineItems = lineItems,
            TotalAmount = lineItems.Sum(item => item.TotalPrice)
            // IpAddress
        };

        var maxId = await _ordersRepository.GetMaxIdAsync(cancellationToken);

        string orderNumber = $"{maxId + 1}/{order.OrderDate.ToLocalTime().Year}";

        order.OrderNumber = orderNumber;

        await _ordersRepository.AddAsync(order, cancellationToken);

        return orderNumber;
    }

    private OrderLineItem CreateLineItem(Product product)
    {
        return new OrderLineItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            ProductPrice = product.DiscountedPrice,
            DiscountPercentage = product.DiscountPercentage,
            Quantity = 1,
            TotalPrice = product.DiscountedPrice * 1
        };
    }
}
