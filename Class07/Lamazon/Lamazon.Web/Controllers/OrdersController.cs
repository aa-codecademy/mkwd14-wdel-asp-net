using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.Models;
using Lamazon.Web.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Lamazon.Web.Controllers;

public class OrdersController : BaseController
{
    private readonly IOrdersService _ordersService;

    public OrdersController(IOrdersService ordersService)
    {
        _ordersService = ordersService;
    }

    public async Task<IActionResult> ShoppingCart(CancellationToken cancellationToken)
    {
        var shoppingCart = GetShoppingCart();
        var productIds = shoppingCart.Items.Select(i => i.ProductId).ToList();
        var lineItems = await _ordersService.GetCartLineItemsAsync(productIds, cancellationToken);

        if (lineItems.Count != shoppingCart.Items.Count)
        {
            shoppingCart.Items.RemoveAll(item => !lineItems.Any(lineItem => lineItem.ProductId == item.ProductId));
            SaveShoppingCart(shoppingCart);
            ViewBag.ShoppingCartItemsCount = shoppingCart.Items.Count;
        }

        if (lineItems.Count == 0)
        {
            return RedirectToAction("Index", "Products");
        }

        return View(lineItems);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrder(CancellationToken cancellationToken)
    {
        var productIds = GetShoppingCartProductIds();
        // ipAddress

        var orderNumber = await _ordersService.CreateAsync(User.GetUserId(), productIds, cancellationToken);
        SaveShoppingCart(new ShoppingCartViewModel());
        AddSuccessNotification($"Order {orderNumber} was created. Thank you!");

        return RedirectToAction("Index", "Home");
    }
}
