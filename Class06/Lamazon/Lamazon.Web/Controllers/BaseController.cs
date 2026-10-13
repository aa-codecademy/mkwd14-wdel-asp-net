using System.Text.Json;
using Lamazon.ViewModels.Models;
using Lamazon.Web.Constants;
using Lamazon.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lamazon.Web.Controllers;

/// <summary>
/// Base controller that provides common functionality for all controllers in the application, such as shopping cart management and notification handling.
/// </summary>
public abstract class BaseController : Controller
{
    /// <summary>
    /// Called every time before the action method is executed. 
    /// </summary>
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        base.OnActionExecuting(context);

        var shoppingCart = GetShoppingCart();
        ViewBag.ShoppingCartItemsCount = shoppingCart.Items.Count;
    }

    #region Shopping Cart

    /// <summary>
    /// Retrieves the shopping cart from the cookies. 
    /// </summary>
    protected ShoppingCartViewModel GetShoppingCart()
    {
        string? cookieValue = Request.Cookies[Cookies.ShoppingCart];
        if (string.IsNullOrEmpty(cookieValue))
        {
            return new ShoppingCartViewModel();
        }

        try
        {
            ShoppingCartViewModel? shoppingCart = JsonSerializer.Deserialize<ShoppingCartViewModel>(cookieValue);
            return shoppingCart ?? new ShoppingCartViewModel();
        }
        catch (JsonException)
        {
            return new ShoppingCartViewModel();
        }
    }

    /// <summary>
    /// Saves the shopping cart to the cookies.
    /// </summary>
    protected void SaveShoppingCart(ShoppingCartViewModel shoppingCart)
    {
        if (shoppingCart.Items.Count == 0)
        {
            Response.Cookies.Delete(Cookies.ShoppingCart);
            return;
        }

        CookieOptions cookieOptions = new()
        {
            Expires = DateTime.UtcNow.AddDays(3),
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.Lax
        };

        string shoppingCartJson = JsonSerializer.Serialize(shoppingCart);

        Response.Cookies.Append(Cookies.ShoppingCart, shoppingCartJson, cookieOptions);
    }

    /// <summary>
    /// Marks the products that are already in the shopping cart by setting the IsAddedToCart property to true for those products.
    /// </summary>
    protected void MarkProductsInCart(IEnumerable<ProductViewModel> products)
    {
        var shoppingCart = GetShoppingCart();
        var productIdsInCart = shoppingCart.Items.Select(item => item.ProductId).ToHashSet();

        foreach (var product in products)
        {
            product.IsAddedToCart = productIdsInCart.Contains(product.Id);
        }
    }

    #endregion


    #region Notifications

    // Used to add success and error notifications to the TempData, which can be displayed in the views using the JS library 'notyf'

    protected void AddSuccessNotification(string message)
    {
        TempData[Notifications.MessageKey] = message;
        TempData[Notifications.TypeKey] = "success";
    }

    protected void AddErrorNotification(string message)
    {
        TempData[Notifications.MessageKey] = message;
        TempData[Notifications.TypeKey] = "error";
    }

    #endregion

}
