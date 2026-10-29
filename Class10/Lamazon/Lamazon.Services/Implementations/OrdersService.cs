using System.Transactions;
using Lamazon.DataAccess.Repositories.Abstractions;
using Lamazon.Domain.Entities;
using Lamazon.Domain.Enums;
using Lamazon.Domain.Exceptions;
using Lamazon.Services.Abstractions;
using Lamazon.Services.Extensions;
using Lamazon.ViewModels.DataTable;
using Lamazon.ViewModels.Models;
using MapsterMapper;

namespace Lamazon.Services.Implementations;

public class OrdersService : IOrdersService
{
    private readonly IOrdersRepository _ordersRepository;
    private readonly IProductsRepository _productsRepository;
    private readonly IInvoicesRepository _invoicesRepository;
    private readonly IMapper _mapper;
    private readonly IGeoTrackerService _geoTrackerService;

    public OrdersService(
        IOrdersRepository ordersRepository,
        IProductsRepository productsRepository,
        IMapper mapper,
        IGeoTrackerService geoTrackerService,
        IInvoicesRepository invoicesRepository)
    {
        _ordersRepository = ordersRepository;
        _productsRepository = productsRepository;
        _mapper = mapper;
        _geoTrackerService = geoTrackerService;
        _invoicesRepository = invoicesRepository;
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
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var products = await _productsRepository.GetByIdsAsync(productIds, cancellationToken);
        if (products.Count == 0)
        {
            throw new BusinessRuleException("Your shopping cart is empty.");
        }

        var lineItems = products.Select(CreateLineItem).ToList();
        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            OrderStatusId = (int)OrderStatusEnum.Pending,
            OrderLineItems = lineItems,
            TotalAmount = lineItems.Sum(item => item.TotalPrice),
            IpAddress = ipAddress,
        };

        var maxId = await _ordersRepository.GetMaxIdAsync(cancellationToken);

        string orderNumber = $"{maxId + 1}/{order.OrderDate.ToLocalTime().Year}";

        order.OrderNumber = orderNumber;

        await AddLocationAsync(order, cancellationToken);

        await _ordersRepository.AddAsync(order, cancellationToken);

        return orderNumber;
    }

    private async Task AddLocationAsync(Order order, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(order.IpAddress))
        {
            return;
        }

        var ipGeoInfo = await _geoTrackerService.GetIpGeoInfoAsync(order.IpAddress, cancellationToken);

        if (ipGeoInfo is not null && !string.IsNullOrEmpty(ipGeoInfo.CountryCode))
        {
            order.CountryCode = ipGeoInfo.CountryCode;
            order.CountryFlagUrl = _geoTrackerService.GetCountryFlagUrl(ipGeoInfo.CountryCode);
        }
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

    public async Task<OrderViewModel> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var order = await FindOrderAsync(id, cancellationToken);
        return _mapper.Map<OrderViewModel>(order);
    }

    public async Task<PagedResultViewModel<OrderViewModel>> GetFilteredAsync(
        DataTableRequestViewModel request,
        CancellationToken cancellationToken = default)
    {
        var pagedResult = await _ordersRepository.GetFilteredAsync(request.ToPagedQuery(), cancellationToken);
        return _mapper.Map<PagedResultViewModel<OrderViewModel>>(pagedResult);
    }

    public async Task AcceptAsync(int id, CancellationToken cancellationToken = default)
    {
        using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

        // 1) Update Order Status
        var order = await FindOrderAsync(id, cancellationToken);
        if (order.OrderStatusId != (int)OrderStatusEnum.Pending)
        {
            var status = (OrderStatusEnum)order.OrderStatusId;
            throw new BusinessRuleException($"Only pending orders can be accepted. Order {order.OrderNumber} is {status}.");
        }
        order.OrderStatusId = (int)OrderStatusEnum.Accepted;
        await _ordersRepository.UpdateAsync(order, cancellationToken);

        // 2) Create Invoice
        var invoice = _mapper.Map<Invoice>(order);
        var maxId = await _invoicesRepository.GetMaxIdAsync(cancellationToken);
        invoice.InvoiceNumber = $"{maxId + 1}/{invoice.InvoiceDate.ToLocalTime().Year}";
        await _invoicesRepository.AddAsync(invoice, cancellationToken);

        scope.Complete();
    }

    public Task RejectAsync(int id, CancellationToken cancellationToken = default) => throw new NotImplementedException();

    private async Task<Order> FindOrderAsync(int id, CancellationToken cancellationToken)
    {
        return await _ordersRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Order), id);
    }
}
