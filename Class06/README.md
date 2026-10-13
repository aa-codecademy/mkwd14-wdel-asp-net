# 🛒 Class 06: The cart and orders

> 🛍️ The shop · class 6 of 10 · 🏠 [Course overview](../README.md) · ⬅️ [Class 05](../Class05/README.md) · 🗂️ [The domain](../Class01/domain.md) · 🗺️ [Database diagram](../Class01/Lamazon%20DB%20Diagram.png)

The **rest of class 5**: pop-up notifications, a **cart** that works on every page without reloading it, and **orders** saved with the prices from the database. Middleware, HttpClient, Polly and logging move to class 7.

## 🗺️ What we do today

1. 🔁 Recap and the class 5 homework.
2. 📦 LibMan, and pop-up notifications with Notyf. XSS: why a message needs encoding
3. 🍪 The cart in a cookie: `BaseController`
4. ➕ `AddToCart` and `RemoveFromCart`: actions that answer with JSON
5. ⚡ `site.js`: Add to cart and Remove from cart without reloading the page
6. 🛒 The cart page (mostly a handout)
7. 📦 Placing an order: the prices come from the database, never from the browser
8. ⏩ If there's time: middleware

---

## 💡 Used concepts

### 📦 LibMan: client-side libraries
`wwwroot/lib` holds the CSS and JavaScript libraries the browser downloads: Bootstrap and jQuery came with the project template. For a new one we use **LibMan** (Library Manager), which is built into Visual Studio:
- 📥 it downloads **only the files you pick** from a CDN (jsDelivr, cdnjs or unpkg) into `wwwroot/lib`
- 📝 it writes them into `libman.json`, like a `.csproj` lists the NuGet packages
- 🚫 no Node.js, no npm, no `node_modules` folder with hundreds of megabytes

**Install Notyf:**
1. Right-click `wwwroot/lib` → **Add** → **Client-Side Library…**
2. Provider: **jsdelivr**. Library: `notyf@3.10.0`
3. **Choose specific files:** `notyf.min.css`, `notyf.min.js` and `LICENSE.md`
4. Target location: `wwwroot/lib/notyf/` → **Install**

Visual Studio creates `libman.json` in the project's root:
```json
{
  "version": "3.0",
  "defaultProvider": "jsdelivr",
  "libraries": [
    {
      "library": "notyf@3.10.0",
      "destination": "wwwroot/lib/notyf/",
      "files": [
        "notyf.min.js",
        "notyf.min.css",
        "LICENSE.md"
      ]
    }
  ]
}
```
🔁 Right-click `libman.json` → **Restore Client-Side Libraries** downloads them all again, e.g. after a clone without `wwwroot/lib`. In class 7 the CMS adds Material Dashboard and DataTables the same way.

### 🔔 Pop-up notifications
After Register, the page redirects to the home page. How does the home page know that it should say "Welcome"?
```
POST /Users/Register  →  AddNotificationMessage("Welcome to Lamazon, Ana!")  →  TempData  →  302 to /
GET /                 →  _Layout  →  _Notification reads TempData  →  <script>notyf.open(…)</script>
GET / again           →  TempData is empty: the message showed once
```
📮 **TempData** survives exactly one redirect: it's written now and read on the next page, then it's gone. `ViewBag` wouldn't work: it lives only for the current request, and the redirect is a new request. By default TempData is kept in a cookie (encrypted) until it's read.

**The keys and the two kinds of message**, in `Lamazon.Web/Helpers/Notifications.cs`:
```csharp
/// <summary>
/// The green/red pop-up message after a redirect ("Order 5/2026 was created").
/// TempData survives exactly one redirect: it's written now and read by the next page (the _Notification partial).
/// </summary>
public static class Notifications
{
    public const string MessageKey = "NotificationMessage";
    public const string TypeKey = "NotificationType";

    public static void AddSuccess(ITempDataDictionary tempData, string message)
    {
        tempData[MessageKey] = message;
        tempData[TypeKey] = "success";
    }

    public static void AddError(ITempDataDictionary tempData, string message)
    {
        tempData[MessageKey] = message;
        // Notyf's two built-in types are "success" (green) and "error" (red)
        tempData[TypeKey] = "error";
    }
}
```
`AddError` is for class 8: a rule that's broken ("a category with products can't be deleted").

**`BaseController`**: one base class for the shop's controllers. For now it has only the shortcut; the cart comes next.
```csharp
// Lamazon.Web/Controllers/BaseController.cs
/// <summary>
/// What every controller of the shop needs: the shopping cart cookie and the notification messages.
/// </summary>
public abstract class BaseController : Controller
{
    protected void AddNotificationMessage(string message)
    {
        Notifications.AddSuccess(TempData, message);
    }
}
```
🧱 **`abstract`:** MVC never routes to an abstract class, so `/Base` is a 404. It's only the base of the real controllers.

**In `UsersController`**: `public class UsersController : BaseController`, and in the `Register` POST, after the sign-in:
```csharp
await AuthHelper.SignInUserAsync(HttpContext, user);

// TempData: shown by the _Notification partial on the next page (after the redirect)
AddNotificationMessage($"Welcome to Lamazon, {user.FullName}!");

return RedirectToAction("Index", "Home");
```

**The partial that shows it**, `Views/Shared/_Notification.cshtml`:
```cshtml
@using Lamazon.Web.Helpers
@{
    var message = TempData[Notifications.MessageKey] as string;
    var type = TempData[Notifications.TypeKey] as string ?? "success";
}

<script>
    // At the top right (the default is the bottom right), for 5 seconds instead of 2, with an × to close it sooner
    var notyf = new Notyf({ duration: 5000, dismissible: true, position: { x: 'right', y: 'top' } });
</script>

@if (!string.IsNullOrEmpty(message))
{
    <script>
        notyf.open({ type: @Json.Serialize(type), message: @Json.Serialize(Html.Encode(message)) });
    </script>
}
```
`Html.Encode` and `Json.Serialize` are there because of XSS: see the next section.

**In `_Layout.cshtml`**: the Notyf CSS in `<head>`, after Bootstrap's:
```cshtml
<link rel="stylesheet" href="~/lib/notyf/notyf.min.css" />
```
At the end of `<body>`, delete the two commented-out lines (bootstrap-notify and `_Notification`), and load Notyf and the partial **before** `site.js`:
```cshtml
<script src="~/lib/jquery/dist/jquery.min.js"></script>
<script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
<script src="~/lib/notyf/notyf.min.js"></script>
@* The pop-up messages: before site.js, which uses the same notyf when an AJAX call fails *@
<partial name="_Notification" />
<script src="~/js/site.js" asp-append-version="true"></script>
@await RenderSectionAsync("Scripts", required: false)
```
↕️ **The order matters:** `new Notyf(...)` needs `notyf.min.js` above it, and `site.js` uses the `notyf` that the partial creates.

🧪 **Try it:** register a new account. The home page says "Welcome to Lamazon, …" in the top right corner. Reload the page: the message is gone.

### 🛡️ XSS (cross-site scripting)
**The attack.** Write the message without `Html.Encode` first:
```cshtml
notyf.open({ type: @Json.Serialize(type), message: @Json.Serialize(message) });
```
Register with this full name: `<img src=x onerror=alert(1)>`. The welcome message pops up, and so does an **alert**: the browser ran code that someone typed into a form.
- 🧨 **Why:** Notyf puts the message into the page with `innerHTML`, so the browser reads it as HTML. `<img src=x>` can't load, so its `onerror` runs.
- 🎯 **Here the victim is you.** The same mistake in text that **someone else** reads is *stored XSS*: in class 7 the admin sees every customer's name in the CMS, and the script would run in the admin's browser, with the admin's login.
- 🍪 **`HttpOnly` isn't enough:** a script can't read the login cookie, but on our page it can do anything the user can. It can read the page, click buttons, and send forms, with the anti-forgery token that's in the page.

**The fix:** `@Json.Serialize(Html.Encode(message))`. Register again with the same name: the message shows `<img src=x onerror=alert(1)>` as text.
| Step | `<img src=x onerror=alert(1)>` becomes | Without it |
|---|---|---|
| `Html.Encode(...)` | `&lt;img src=x onerror=alert(1)&gt;`: text, not a tag | the alert |
| `Json.Serialize(...)` | `"&lt;img src=x onerror=alert(1)&gt;"`: a JavaScript string. A quote, a `\` or a `</script>` in it can't end the string early | `O'Brien` ends a `'…'` string: a JavaScript error, or worse |
| Notyf's `innerHTML` | shows `<img src=x onerror=alert(1)>` as text | |

🔍 **Razor's `@` already encodes:** the navbar shows the same name as plain text (`@User.DisplayName()`). The danger is text that leaves HTML: into JavaScript, and from there into `innerHTML`. And `Html.Raw(...)`, which turns the encoding off.

---

### 🍪 The cart in a cookie
**Where the cart lives:** in a cookie. It works before the login (you fill the cart and log in only to order), it needs no table, and the browser sends it with every request, so every action can read it.
- 🔢 **Only the product ids, never a price.** A cookie is in the visitor's hands, and they can change it. Names and prices always come from the database.
- 📏 **About 4 KB per cookie:** enough for the ids of a few hundred products.

```csharp
// Lamazon.ViewModels/Models/ShoppingCartItemViewModel.cs
public class ShoppingCartItemViewModel
{
    public int ProductId { get; set; }
}
```
```csharp
// Lamazon.ViewModels/Models/ShoppingCartViewModel.cs
/// <summary>
/// The shopping cart, stored as JSON in a cookie. Only the product ids:
/// names and prices are always read again from the database.
/// </summary>
public class ShoppingCartViewModel
{
    public List<ShoppingCartItemViewModel> Items { get; set; } = [];
}
```
As JSON, a cart with two products is `{"Items":[{"ProductId":1},{"ProductId":4}]}`.

The cookie's name goes next to the login cookie's, in `Lamazon.Web/Constants/Cookies.cs`:
```csharp
/// <summary>The shopping cart: the ids of the products in it, as JSON (see BaseController).</summary>
public const string ShoppingCart = "Lamazon.ShoppingCart";
```

**`BaseController` reads and writes it** (add `using System.Text.Json;` and `using Microsoft.AspNetCore.Mvc.Filters;`):
```csharp
// Runs before every action of every controller that inherits BaseController
public override void OnActionExecuting(ActionExecutingContext context)
{
    base.OnActionExecuting(context);

    // The navbar shows the number of items in the cart on every page
    ViewBag.ShoppingCartItemsCount = GetShoppingCart().Items.Count;
}

protected ShoppingCartViewModel GetShoppingCart()
{
    string? cookieValue = Request.Cookies[Cookies.ShoppingCart];
    if (string.IsNullOrEmpty(cookieValue))
    {
        return new ShoppingCartViewModel();
    }

    try
    {
        return JsonSerializer.Deserialize<ShoppingCartViewModel>(cookieValue) ?? new ShoppingCartViewModel();
    }
    catch (JsonException)
    {
        // Someone edited the cookie by hand: start again with an empty cart
        return new ShoppingCartViewModel();
    }
}

protected void SaveShoppingCart(ShoppingCartViewModel shoppingCart)
{
    if (shoppingCart.Items.Count == 0)
    {
        Response.Cookies.Delete(Cookies.ShoppingCart);
        return;
    }

    // A cookie can hold about 4 KB: enough for the ids of a few hundred products
    Response.Cookies.Append(Cookies.ShoppingCart, JsonSerializer.Serialize(shoppingCart), new CookieOptions
    {
        Expires = DateTimeOffset.Now.AddDays(2),
        HttpOnly = true,          // only the server reads the cart; JavaScript doesn't need to
        IsEssential = true,       // the shop doesn't work without it
        SameSite = SameSiteMode.Lax,
    });
}
```
| Option | What it does |
|---|---|
| `Expires` | the cart stays for 2 days. Without it, the cookie is gone when the browser closes |
| `HttpOnly` | JavaScript can't read it. `site.js` doesn't need to: the server answers with the number of items |
| `IsEssential` | a cookie-consent banner (if the app had one) can't block it: the shop doesn't work without the cart |
| `SameSite = Lax` | the browser doesn't send it with a POST that comes from another site |

🪝 **`OnActionExecuting`** is a method of `Controller` that MVC calls before every action. It's a filter, like `AppExceptionFilter`, only built into the controller. Every controller that inherits `BaseController` puts the counter into `ViewBag`, and `_Navbar` already reads `ViewBag.ShoppingCartItemsCount` (since class 3).

`HomeController` and `ProductsController` inherit `BaseController` too: `public class HomeController : BaseController`.

🧪 **Try it:** nothing changes on screen yet: the cart is empty, so the counter on the cart icon stays hidden. Every page still works.

### ➕ AddToCart and RemoveFromCart
Two actions that `site.js` calls in the background. They answer with **JSON**, not with a view. In `ProductsController`:
```csharp
// Called with AJAX from site.js. It answers with JSON, not with a view.
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddToCart(int productId, CancellationToken cancellationToken)
{
    // Only products that really exist go into the cart (an unknown id → NotFoundException → 404)
    await _productsService.GetByIdAsync(productId, cancellationToken);

    ShoppingCartViewModel shoppingCart = GetShoppingCart();
    if (!shoppingCart.Items.Any(item => item.ProductId == productId))
    {
        shoppingCart.Items.Add(new ShoppingCartItemViewModel { ProductId = productId });
        SaveShoppingCart(shoppingCart);
    }

    return Json(new { itemsCount = shoppingCart.Items.Count });
}

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult RemoveFromCart(int productId)
{
    ShoppingCartViewModel shoppingCart = GetShoppingCart();
    shoppingCart.Items.RemoveAll(item => item.ProductId == productId);
    SaveShoppingCart(shoppingCart);

    return Json(new { itemsCount = shoppingCart.Items.Count });
}
```
- 📤 **`Json(new { itemsCount = 2 })`** answers `{"itemsCount":2}`, with `Content-Type: application/json`. An anonymous object is enough for a one-off answer.
- 1️⃣ **No quantity:** a product is in the cart once, or not at all. Adding it twice changes nothing.
- 🛡️ **`[ValidateAntiForgeryToken]`:** these actions change the cart, so they're POSTs with a token, like every form. AJAX sends it in a header (next section).

**Which buttons to show:** `ProductViewModel.IsAddedToCart` (since class 3) decides whether `_CartButtons` shows "Add to cart" or "Remove from cart". A method in `BaseController` sets it:
```csharp
/// <summary>Sets IsAddedToCart on every product that is in the visitor's cart (for the Add/Remove buttons).</summary>
protected void MarkProductsInCart(IEnumerable<ProductViewModel> products)
{
    HashSet<int> productIdsInCart = GetShoppingCart().Items.Select(item => item.ProductId).ToHashSet();

    foreach (ProductViewModel product in products)
    {
        product.IsAddedToCart = productIdsInCart.Contains(product.Id);
    }
}
```
Call it in `HomeController.Index` (`MarkProductsInCart(featuredProducts);`) and in `ProductsController.Index` (`MarkProductsInCart(products);`), right after the service call. `Details` is the homework.

🧪 **Try it in the browser's console** (F12 → Console), on the home page. Without the token:
```js
fetch('/Products/AddToCart', { method: 'POST', body: new URLSearchParams({ productId: 1 }) }).then(r => r.status)
```
`400`: no anti-forgery token, no cart. Now with the token from the page:
```js
fetch('/Products/AddToCart', {
    method: 'POST',
    headers: { RequestVerificationToken: document.querySelector('[name=__RequestVerificationToken]').value },
    body: new URLSearchParams({ productId: 1 })
}).then(r => r.json())
```
`{itemsCount: 1}`. Reload the page: the counter shows 1, and the first product says "Remove from cart". That's exactly what `site.js` does, without the reload.

### ⚡ site.js: AJAX
Replace the template's comment in `wwwroot/js/site.js`:
```js
// JavaScript for the shop (every page that uses _Layout)

// The anti-forgery token that _Layout renders once (@Html.AntiForgeryToken()).
// ASP.NET Core accepts it in the "RequestVerificationToken" header of an AJAX request.
function getAntiForgeryToken() {
    return $('input[name="__RequestVerificationToken"]').first().val();
}

// The red counter on the cart icon in the navbar
function updateShoppingCartItemsCount(count) {
    var counter = $('#shoppingCartItemsCount');
    counter.text(count);
    counter.toggleClass('hidden', count <= 0);
}

// Add to cart / Remove from cart: POST the product id, the server updates the cart cookie
// and answers with the new number of items: { "itemsCount": 3 }
function changeShoppingCart(clickedButton) {
    $.ajax({
        type: 'POST',
        url: clickedButton.data('url'),
        data: { productId: clickedButton.data('productId') },
        headers: { 'RequestVerificationToken': getAntiForgeryToken() },
        success: function (result) {
            updateShoppingCartItemsCount(result.itemsCount);

            // In the shopping cart itself, removing a product reloads the page (new list, new total)
            if (clickedButton.data('reload')) {
                window.location.reload();
                return;
            }

            // Show the other button of the pair (Add ⇄ Remove)
            clickedButton.addClass('hidden');
            clickedButton.siblings('.btn-add-to-cart, .btn-remove-from-cart').removeClass('hidden');
        },
        error: function () {
            // notyf comes from the _Notification partial
            notyf.error('Something went wrong. Please try again.');
        }
    });
}

// "Event delegation": one handler on the document works for every button, also ones added later
$(document).on('click', '.btn-add-to-cart, .btn-remove-from-cart', function () {
    changeShoppingCart($(this));
});
```
| Piece | Why |
|---|---|
| `$.ajax({ type: 'POST', … })` | the request goes in the background, and the page stays where it is. `data` is sent as form fields: `productId=1` |
| `headers: { 'RequestVerificationToken': … }` | a form sends its token as a hidden field. AJAX has no form, so it takes the token that `_Layout` writes once (`@Html.AntiForgeryToken()`, since class 3) and sends it in a header. `[ValidateAntiForgeryToken]` checks both places. This is the CSRF lesson from class 5, for AJAX |
| `clickedButton.data('url')` | the `data-url` attribute of the button. `.data('productId')` reads `data-product-id` |
| `updateShoppingCartItemsCount` | the number comes from the server's answer. JavaScript can't read the `HttpOnly` cookie, and doesn't need to |
| `$(document).on('click', '.btn-add-to-cart, …', …)` | **event delegation:** one handler on the whole document instead of one per button. It works on every page, for every button, also for buttons that are added later |
| `data('reload')` | for the Remove buttons on the cart page (the homework) |
| `error` | 400 (no token), 404 (no such product), or the server is down: a red Notyf message |

**The URLs come from the server**, in `Views/Shared/_CartButtons.cshtml`:
```cshtml
@model ProductViewModel
@*
    The two buttons are both always there; site.js shows one and hides the other after each click.
    The URLs come from Url.Action, so they are right on every page (a relative "Products/AddToCart"
    would break on /Products/Details/1).
*@
<button type="button" class="btn btn-remove-from-cart btn-danger @(Model.IsAddedToCart ? "" : "hidden")"
        data-product-id="@Model.Id" data-url="@Url.Action("RemoveFromCart", "Products", new { area = "" })">
    Remove from cart
</button>
<button type="button" class="btn btn-add-to-cart btn-success @(Model.IsAddedToCart ? "hidden" : "")"
        data-product-id="@Model.Id" data-url="@Url.Action("AddToCart", "Products", new { area = "" })">
    Add to cart
</button>
```
- 🧭 **`Url.Action`** writes `/Products/AddToCart`. Last year `site.js` had `url: 'Products/AddToCart'`, a relative address: on `/Products/Details/1` the browser turned it into `/Products/Details/Products/AddToCart`, a 404.
- 🗂️ **`new { area = "" }`:** the shop's controllers. It matters from class 7, when the CMS has its own area.

🧪 **Try it:**
| Do | You should see |
|---|---|
| Add to cart on the home page | the button turns into "Remove from cart", and the counter on the cart icon shows 1. No reload |
| Reload the page, open `/Products` | the cart is still there: the same buttons and the same counter |
| F12 → Application → Cookies | `Lamazon.ShoppingCart`, with **HttpOnly** ticked. The value is URL-encoded JSON: `%7B%22Items%22%3A%5B%7B%22ProductId%22%3A1%7D%5D%7D` |
| `document.cookie` in the console | no `Lamazon.ShoppingCart`: `HttpOnly` hides it |
| Change the cookie's value to `abc`, reload | an empty cart, and no error page (the `catch (JsonException)`) |
| Delete the hidden `__RequestVerificationToken` input (Elements), then click Add to cart | a red "Something went wrong" message: the server answered 400 |

---

### 🛒 The cart page
A click on the cart icon opens `/Orders/ShoppingCart` (the link is in `_Navbar` since class 3):
```
GET /Orders/ShoppingCart  →  [Authorize]: not logged in?  →  the login page  →  back here (returnUrl, class 5)
  →  GetShoppingCart(): the product ids [1, 4]
  →  OrdersService.GetCartLineItemsAsync([1, 4])
       →  ProductsRepository.GetByIdsAsync: ONE query, WHERE Id IN (1, 4), without the deleted products
       →  one line per product, with today's price
  →  View(lineItems): the products, the prices and the total, and the "Create order" button
```

**Typed: all the products of the cart in one query.** In `IProductsRepository`:
```csharp
/// <summary>The products with these ids, for example the ones in the shopping cart.</summary>
Task<List<Product>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);
```
In `ProductsRepository`:
```csharp
public async Task<List<Product>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default)
{
    // One query for all of them: WHERE Id IN (1, 4, 6). Deleted products are skipped (ActiveProducts).
    return await ActiveProducts
        .AsNoTracking()
        .Where(product => ids.Contains(product.Id))
        .ToListAsync(cancellationToken);
}
```
🔎 **`ids.Contains(product.Id)`** becomes SQL's `IN`. A `foreach` with `GetByIdAsync` would send one query per product.

📄 **Handout: one line of the cart (and later of an order)**, `Lamazon.ViewModels/Models/OrderLineItemViewModel.cs`:
```csharp
public class OrderLineItemViewModel
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }

    /// <summary>The price of one item, with the discount already applied.</summary>
    public decimal ProductPrice { get; set; }
    public int DiscountPercentage { get; set; }
    public decimal TotalPrice { get; set; }

    // Only shown in the shopping cart, not saved with the order
    public string? ProductDescription { get; set; }
    public string? ProductImageUrl { get; set; }
}
```

📄 **Handout: the service**, `Lamazon.Services/Abstractions/IOrdersService.cs`:
```csharp
public interface IOrdersService
{
    /// <summary>
    /// The shopping cart as order lines, with today's prices from the database.
    /// Products that don't exist anymore are left out.
    /// </summary>
    Task<List<OrderLineItemViewModel>> GetCartLineItemsAsync(IReadOnlyCollection<int> productIds, CancellationToken cancellationToken = default);
}
```
And `Lamazon.Services/Implementations/OrdersService.cs`:
```csharp
public class OrdersService : IOrdersService
{
    private readonly IProductsRepository _productsRepository;
    private readonly IMapper _mapper;

    public OrdersService(
        IProductsRepository productsRepository,
        IMapper mapper)
    {
        _productsRepository = productsRepository;
        _mapper = mapper;
    }

    public async Task<List<OrderLineItemViewModel>> GetCartLineItemsAsync(IReadOnlyCollection<int> productIds, CancellationToken cancellationToken = default)
    {
        List<Product> products = await _productsRepository.GetByIdsAsync(productIds, cancellationToken);

        return products.Select(product =>
        {
            // The same line the order will save (CreateLineItem), so the cart shows exactly what you'll pay
            OrderLineItemViewModel lineItem = _mapper.Map<OrderLineItemViewModel>(CreateLineItem(product));

            // Shown in the cart, but not part of an order line
            lineItem.ProductDescription = product.Description;
            lineItem.ProductImageUrl = product.ImageUrl;
            return lineItem;
        }).ToList();
    }

    private static OrderLineItem CreateLineItem(Product product)
    {
        const int quantity = 1;

        return new OrderLineItem
        {
            ProductId = product.Id,
            ProductName = product.Name,
            ProductPrice = product.DiscountedPrice,
            DiscountPercentage = product.DiscountPercentage,
            Quantity = quantity,
            TotalPrice = product.DiscountedPrice * quantity,
        };
    }
}
```
- 🧾 **`CreateLineItem`** builds the line that the order saves (next section). The cart page shows the same line, so what you see is what you pay.
- 💲 **`ProductPrice` is the price after the discount** (`DiscountedPrice`, class 1). `DiscountPercentage` is saved too, so the invoice can show it later.
- 🖼️ **The description and the image** aren't columns of `OrderLineItem`, so the service fills them in for the cart page.

📄 **Handout:** the mapping, `Lamazon.Services/Mappings/OrderMappingConfig.cs`. The names match, so one line is enough:
```csharp
public class OrderMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<OrderLineItem, OrderLineItemViewModel>();
    }
}
```
Register the service in `AddServices()`: `services.AddScoped<IOrdersService, OrdersService>();`.

**Typed: the controller**, `Lamazon.Web/Controllers/OrdersController.cs`:
```csharp
// Only logged-in users can see the cart and order. Anyone else is sent to the login page first.
[Authorize]
public class OrdersController : BaseController
{
    private readonly IOrdersService _ordersService;

    public OrdersController(IOrdersService ordersService)
    {
        _ordersService = ordersService;
    }

    public async Task<IActionResult> ShoppingCart(CancellationToken cancellationToken)
    {
        ShoppingCartViewModel shoppingCart = GetShoppingCart();
        List<int> productIds = shoppingCart.Items.Select(item => item.ProductId).ToList();

        List<OrderLineItemViewModel> lineItems = await _ordersService.GetCartLineItemsAsync(productIds, cancellationToken);

        // Products deleted since they were added disappear from the cart (and from the navbar counter)
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
}
```
- 🔐 **`[Authorize]` on the whole controller:** only a logged-in customer can see the cart page and order. This is the "for real" from class 5.
- 🗑️ **A product deleted in the CMS** (class 8) disappears from the cookie too, and from the counter. `ViewBag` is set again, because `OnActionExecuting` counted before the cleanup.
- 🫙 **An empty cart** has nothing to show: back to the products page.

📄 **Handout:** `Views/Orders/ShoppingCart.cshtml`. The styles (`order-item`, `order-label`, …) are in `site.css` since class 3.
```cshtml
@model List<OrderLineItemViewModel>
@{
    ViewData["Title"] = "Shopping cart";
}

<h2>Shopping cart</h2>

<div class="row">
    <div class="col-md-8">
        @foreach (OrderLineItemViewModel lineItem in Model)
        {
            <div class="row order-item">
                <div class="col-md-2">
                    <img class="order-item-image" src="@lineItem.ProductImageUrl" alt="@lineItem.ProductName" />
                </div>
                <div class="col-md-6">
                    <a asp-controller="Products" asp-action="Details" asp-route-id="@lineItem.ProductId">
                        <p class="order-item-name">@lineItem.ProductName</p>
                    </a>
                    <p class="order-item-price">Price: $@lineItem.ProductPrice.ToString("N2")</p>
                    <p class="order-item-description">@lineItem.ProductDescription</p>
                    @* Homework: the Remove button *@
                </div>
                <div class="col-md-2 text-center">
                    <p class="order-label">Qty</p>
                    <p class="order-value">@lineItem.Quantity</p>
                </div>
                <div class="col-md-2 text-end">
                    <p class="order-label">Total</p>
                    <p class="order-value">$@lineItem.TotalPrice.ToString("N2")</p>
                </div>
            </div>
        }
    </div>

    <div class="col-md-4 text-center ps-5">
        <p class="order-label">Total: $@Model.Sum(lineItem => lineItem.TotalPrice).ToString("N2")</p>

        @*
            The form sends NO prices, only the anti-forgery token (added by the form tag helper).
            The server reads the products from the cart cookie and their prices from the database.
        *@
        <form asp-controller="Orders" asp-action="CreateOrder" method="post" class="mt-3">
            <button class="btn btn-create-order btn-success" type="submit">Create order</button>
        </form>
    </div>
</div>
```

🧪 **Try it:**
| Do | You should see |
|---|---|
| Logged out, with something in the cart, click the cart icon | the login page. Log in, and you're back on the cart page |
| The cart page | each product with its price after the discount, and the total |
| Empty the cart (Remove from cart on the home page), open `/Orders/ShoppingCart` | the products page |

### 📦 Placing an order
**Never trust the client.** Last year's cart page had hidden form fields with the prices:
```html
<input type="hidden" name="ProductPrice" value="999.99" />
```
In the browser's dev tools anyone could change `999.99` to `0.01`, and the server saved the order with that price. Today the form sends **only the anti-forgery token**. The server takes the product ids from the cart cookie, and everything else from the database.

**Dates in UTC.** The server's clock, the database and the customer can be in different time zones, so the database stores UTC: `DateTime.UtcNow`, never `DateTime.Now`. SQL Server's `datetime2` doesn't remember that a value is UTC, so a converter marks it again when EF Core reads it. In `Lamazon.DataAccess/Converters/UtcDateTimeConverter.cs`:
```csharp
/// <summary>
/// Every date in the database is UTC. SQL Server's datetime2 doesn't remember that, so without this converter
/// EF Core would read the values back as DateTimeKind.Unspecified, and ToLocalTime() would have to guess.
/// Reading: the value is marked as UTC. Writing: a local time is turned into UTC (a safety net: our code writes UtcNow).
/// </summary>
public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            value => value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value,
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
    {
    }
}
```
One rule for every `DateTime` of every entity, in `LamazonDbContext`:
```csharp
protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
{
    // Every DateTime in the database is UTC, and comes back marked as UTC (see UtcDateTimeConverter)
    configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
}
```
🗄️ **No migration:** the columns stay `datetime2`. Only the way EF Core reads and writes them changes.

**The orders repository**, with one method of its own (`AddAsync` comes from `BaseRepository`):
```csharp
// Lamazon.DataAccess/Repositories/Abstractions/IOrdersRepository.cs
public interface IOrdersRepository : IRepository<Order>
{
    /// <summary>The highest order id so far (0 when there are no orders), used for the next order number.</summary>
    Task<int> GetMaxIdAsync(CancellationToken cancellationToken = default);
}
```
```csharp
// Lamazon.DataAccess/Repositories/Implementations/OrdersRepository.cs
public class OrdersRepository : BaseRepository<Order>, IOrdersRepository
{
    public OrdersRepository(LamazonDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<int> GetMaxIdAsync(CancellationToken cancellationToken = default)
    {
        // MAX of an empty table is NULL, hence the int? and the ?? 0
        return await Table.MaxAsync(order => (int?)order.Id, cancellationToken) ?? 0;
    }
}
```
Register it in `AddDataAccess()`: `services.AddScoped<IOrdersRepository, OrdersRepository>();`.

**`IOrdersService` gets a second method:**
```csharp
/// <summary>
/// Creates a Pending order for these products and returns its order number.
/// Prices and the total are calculated here, never taken from the browser.
/// </summary>
Task<string> CreateAsync(int userId, IReadOnlyCollection<int> productIds, string? ipAddress, CancellationToken cancellationToken = default);
```
**`OrdersService`** receives `IOrdersRepository _ordersRepository` in its constructor too, and:
```csharp
public async Task<string> CreateAsync(int userId, IReadOnlyCollection<int> productIds, string? ipAddress, CancellationToken cancellationToken = default)
{
    // The browser only tells us WHICH products. Names, prices and the total come from the database:
    // a price in a form or a cookie can be changed by anyone with the browser's developer tools.
    List<Product> products = await _productsRepository.GetByIdsAsync(productIds, cancellationToken);
    List<OrderLineItem> lineItems = products.Select(CreateLineItem).ToList();

    var order = new Order
    {
        UserId = userId,
        // Dates are stored in UTC (see UtcDateTimeConverter)
        OrderDate = DateTime.UtcNow,
        OrderStatusId = (int)OrderStatusEnum.Pending,
        OrderLineItems = lineItems,
        TotalAmount = lineItems.Sum(lineItem => lineItem.TotalPrice),
        IpAddress = ipAddress,
    };

    int maxId = await _ordersRepository.GetMaxIdAsync(cancellationToken);
    // The year of the order's own date, in local time: an order placed at 00:30 on 1 January belongs to
    // the new year, even though it's still 31 December in UTC
    order.OrderNumber = $"{maxId + 1}/{order.OrderDate.ToLocalTime().Year}";

    // Saves the order AND its line items in one go
    await _ordersRepository.AddAsync(order, cancellationToken);

    return order.OrderNumber;
}
```
| Piece | Why |
|---|---|
| `products.Select(CreateLineItem)` | the same lines as the cart page, with the prices from the database |
| `OrderStatusId = (int)OrderStatusEnum.Pending` | a new order waits for an admin to accept or reject it (class 9) |
| `$"{maxId + 1}/{year}"` | the order number, e.g. `7/2026`. The unique index on `OrderNumber` (class 2) makes sure two orders never share one. Two orders placed at the very same moment would both get `7/2026`, and the second one fails on that index. We leave that on purpose (class 10: "what's next") |
| `AddAsync(order)` | one `SaveChanges`: EF Core inserts the order, takes its new `Id`, and inserts the line items with that `OrderId`. All of it in one transaction |

**The action**, in `OrdersController`:
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> CreateOrder(CancellationToken cancellationToken)
{
    // Only the product ids come from the cart. The service reads the prices from the database.
    List<int> productIds = GetShoppingCart().Items.Select(item => item.ProductId).ToList();
    if (productIds.Count == 0)
    {
        // No empty orders (the cart page already sends an empty cart to the products page)
        return RedirectToAction("Index", "Products");
    }

    // ::1 on your own computer (localhost)
    string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

    string orderNumber = await _ordersService.CreateAsync(User.GetUserId(), productIds, ipAddress, cancellationToken);

    // An empty cart deletes the cookie
    SaveShoppingCart(new ShoppingCartViewModel());
    AddNotificationMessage($"Order {orderNumber} was created. Thank you!");

    return RedirectToAction("Index", "Home");
}
```
- 👤 **`User.GetUserId()`** reads the `NameIdentifier` claim from the login cookie (class 5). The user id doesn't come from the form either.
- 🌍 **The IP address** is `::1` on your own computer. The middleware below (or class 7) replaces it with your real address, and class 7 adds the country.
- 🔁 **Post/Redirect/Get:** after the POST, a redirect to the home page. A reload there doesn't send the order a second time.

🧪 **Try it:** fill the cart, open the cart page, **Create order**.
1. The home page says "Order 1/2026 was created. Thank you!", and the counter on the cart icon is gone.
2. In SSMS:
   ```sql
   SELECT * FROM Orders;
   SELECT * FROM OrderLineItems;
   ```
   One order, its total, `::1` as the IP address and status 1 (Pending), and one line per product with its price.
3. `OrderDate` is 1 or 2 hours behind your clock: that's UTC (2 hours in summer time, 1 in winter).

---

🔜 **Next class:** logging with Serilog (mostly a handout), middleware if we didn't get to it today, and HttpClient with Polly, so every order records the customer's country. Then the CMS starts: an admin-only area with its own layout.
