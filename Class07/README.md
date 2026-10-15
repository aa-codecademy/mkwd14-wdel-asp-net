# 🌍 Class 07: Middleware and HttpClient

> 🧑‍💼 The CMS · class 7 of 10 · 🏠 [Course overview](../README.md) · ⬅️ [Class 06](../Class06/README.md) · 🗂️ [The domain](../Class01/domain.md) · 🗺️ [Database diagram](../Class01/Lamazon%20DB%20Diagram.png)

The shop is finished, so today it learns about the world around it. Every request goes through a **pipeline of middleware**. Every new order asks a **web API** in which country the customer is.

## 🗺️ What we do today

1. 🔁 Recap of class 6: the cart page and placing an order
2. 🚦 Middleware: the request pipeline, and why the order matters
3. 🌐 A middleware class: `DebugIpAddressMiddleware`
4. 📡 `IHttpClientFactory` and a typed client: every order gets a country and a flag
5. 🚧 `BusinessRuleException`: a message for the user instead of the error page

---

## 💡 Used concepts

### 🔁 From class 6
The cart page and placing an order are finished in the `Class06` folder, and explained in its README. Compare your code with it and copy what you're missing: today starts from `OrdersService.CreateAsync` and `OrdersController.CreateOrder`.

🧪 **Check it first:** fill the cart, open the cart page, **Create order**. In SSMS:
```sql
SELECT OrderNumber, IpAddress, CountryCode, CountryFlagUrl FROM Orders;
```
`IpAddress` is `::1` and the country columns are `NULL`. By the end of the class, a new order has your public IP address, your country code and a link to your flag.

---

### 🚦 Middleware: the request pipeline
Every request goes through the middleware in `Program.cs`, **from top to bottom**. Each one can do something, call the next one, and do something else when the answer comes back up:
```
request  →  UseStatusCodePages → UseHttpsRedirection → UseRouting → UseAuthentication → UseAuthorization → the action
answer   ←         (on its way back, each middleware can still look at the answer)                         ←
```
That's why the order in `Program.cs` matters. `UseAuthentication` reads the login cookie, so it must come before `UseAuthorization`, which checks `[Authorize]` (class 5).

| Method | What it adds |
|---|---|
| `app.Use(async (context, next) => { …; await next(context); … })` | a middleware that calls the next one: work before **and** after the rest of the pipeline |
| `app.Run(async context => …)` | the end of the pipeline: it never calls `next`. Not the same as the `app.Run()` on the last line of `Program.cs`, which starts the web server |
| `app.Map("/ping", branch => …)` | a separate pipeline only for addresses that start with `/ping` |
| `app.UseMiddleware<T>()` | a middleware class (next section) |

🧪 **A middleware without a class.** In `Program.cs`, right after `var app = builder.Build();` (add `using System.Diagnostics;` at the top):
```csharp
// A middleware without a class: code before and after everything that comes later in the pipeline
app.Use(async (context, next) =>
{
    var stopwatch = Stopwatch.StartNew();

    await next(context);

    // The answer is already on its way: we can still read it, but not change it
    app.Logger.LogInformation("{Method} {Path} → {StatusCode} in {ElapsedMilliseconds} ms",
        context.Request.Method, context.Request.Path, context.Response.StatusCode, stopwatch.ElapsedMilliseconds);
});
```
Open a few pages. Every request writes a line to the console:
```
info: Lamazon.Web[0]
      GET /Products → 200 in 163 ms
```

🔀 **The order matters.** Open `/does-not-exist`: one line, `GET /does-not-exist → 404`. Now move the `app.Use(...)` block **below** `app.UseStatusCodePagesWithReExecute(...)`, restart, and open `/does-not-exist` again:
```
GET /does-not-exist → 404 in 71 ms
GET /Home/Error → 404 in 335 ms
```
Two lines! `UseStatusCodePagesWithReExecute` sends the request through the rest of the pipeline a second time, now for `/Home/Error`, and our middleware is part of that rest. Above it, the middleware sees the request only once.

🗑️ **Delete the `app.Use(...)` block** (and the `using`) when you're done.

### 🌐 A middleware class: `DebugIpAddressMiddleware`
On your own computer every request comes from `::1` (localhost), so every order saves `::1`, and no API can say in which country "localhost" is. In development, this middleware replaces that address with your computer's **public** IP address. In `Lamazon.Web/Middlewares/DebugIpAddressMiddleware.cs`:
```csharp
using System.Net;

namespace Lamazon.Web.Middlewares;

/// <summary>
/// For development only. On your own computer every request comes from 127.0.0.1 (or ::1),
/// and the geo-location API can't say where "localhost" is. This middleware replaces that address
/// with the computer's public IP address, so new orders get a country and a flag.
///
/// A middleware is a class with a constructor that receives the next middleware (RequestDelegate)
/// and an InvokeAsync(HttpContext) method. It can do work before and after calling the next one.
/// </summary>
public class DebugIpAddressMiddleware
{
    private readonly RequestDelegate _next;

    // The middleware is created ONCE for the whole app, so the public IP is looked up once and remembered.
    // Lazy makes sure that happens only once, even when many requests arrive at the same time.
    private readonly Lazy<Task<IPAddress?>> _publicIpAddress;

    public DebugIpAddressMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory, ILogger<DebugIpAddressMiddleware> logger)
    {
        _next = next;
        _publicIpAddress = new Lazy<Task<IPAddress?>>(() => LookUpPublicIpAddressAsync(httpClientFactory, logger));
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        IPAddress? remoteIpAddress = httpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress is not null && IPAddress.IsLoopback(remoteIpAddress))
        {
            IPAddress? publicIpAddress = await _publicIpAddress.Value;
            if (publicIpAddress is not null)
            {
                httpContext.Connection.RemoteIpAddress = publicIpAddress;
            }
        }

        // Pass the request on to the next middleware
        await _next(httpContext);
    }

    private static async Task<IPAddress?> LookUpPublicIpAddressAsync(IHttpClientFactory httpClientFactory, ILogger logger)
    {
        try
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(5);

            // api.ipify.org answers with just the IP address as plain text
            string response = await httpClient.GetStringAsync(new Uri("https://api.ipify.org"));
            return IPAddress.Parse(response.Trim());
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or FormatException)
        {
            // No internet? Then orders keep ::1 and get no country, and the app keeps working.
            logger.LogWarning(exception, "Could not look up the public IP address");
            return null;
        }
    }
}
```
| Piece | Why |
|---|---|
| `RequestDelegate next` | the rest of the pipeline. Not calling it would end the request here |
| `InvokeAsync(HttpContext)` | runs for every request. ASP.NET Core finds it by its name |
| created **once** | a middleware lives as long as the app, so `Lazy` looks the address up on the first request only. That first request takes a second or two longer |
| `IPAddress.IsLoopback` | only `127.0.0.1` and `::1` are replaced. A real visitor's address is never touched |
| `IHttpClientFactory` | hands out `HttpClient`s that share their connections. More in the next section |
| `ILogger<DebugIpAddressMiddleware>` | ASP.NET Core's own logging: the warning goes to the console |
| `catch … when (…)` | no internet: a warning in the console, the order keeps `::1`, and the app keeps working |

📄 **Handout:** a readable name for `Program.cs`, in `Lamazon.Web/Extensions/MiddlewareExtensions.cs`:
```csharp
using Lamazon.Web.Middlewares;

namespace Lamazon.Web.Extensions;

public static class MiddlewareExtensions
{
    /// <summary>
    /// app.UseDebugIpAddress() reads better in Program.cs than app.UseMiddleware&lt;DebugIpAddressMiddleware&gt;().
    /// </summary>
    public static IApplicationBuilder UseDebugIpAddress(this IApplicationBuilder app)
    {
        return app.UseMiddleware<DebugIpAddressMiddleware>();
    }
}
```
The middleware asks for an `IHttpClientFactory`, so register one. In `AddWeb()`, after `services.AddAuthorization();`:
```csharp
// IHttpClientFactory: hands out HttpClients that share their connections (DebugIpAddressMiddleware uses it)
services.AddHttpClient();
```
In `Program.cs`, only in development, at the **start** of the pipeline, so everything after it sees the new address. The `if` gets an `else`:
```csharp
// The order of the middleware matters: every request goes through them from top to bottom
if (app.Environment.IsDevelopment())
{
    // localhost (::1) → the computer's public IP address, so new orders get a country
    app.UseDebugIpAddress();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
```
🔒 **Development only:** on a real server, visitors come with their own addresses, and the server must not call api.ipify.org for them.

🧪 **Try it:** place an order. `SELECT IpAddress FROM Orders` shows your public address instead of `::1`. Then change the URL to `https://api.ipify.invalid` and restart: a warning in the console, the order is saved with `::1`, and the app keeps working. Put the URL back.

---

### 📡 HttpClient the right way: `IHttpClientFactory`
Now that orders have a real IP address, a web API can tell us the country. [ip-api.com](https://ip-api.com) is free and needs no key:
```
GET http://ip-api.com/json/77.28.200.111?fields=status,message,countryCode
→ { "status": "success", "countryCode": "MK" }

GET http://ip-api.com/json/::1?fields=status,message,countryCode
→ { "status": "fail", "message": "reserved range" }
```
Try both in the browser. Without `?fields=` the API sends 14 fields (the city, the coordinates, the internet provider…); we ask only for what we use.

🤔 **Why not `new HttpClient()`?**
| Approach | Problem |
|---|---|
| `using var client = new HttpClient();` for every call | every disposed client leaves its connection open for a few minutes. Under load, the server runs out of connections (*socket exhaustion*) |
| one `static HttpClient` for the whole app | it never notices when the API's DNS address changes |
| **`IHttpClientFactory`** | it reuses the connections and renews them every 2 minutes, so it avoids both problems. The settings (the base address) are in one place, and every call is logged |

**A typed client** is a class that receives a ready `HttpClient` in its constructor. The factory creates that client with our settings.

📦 **The package:** right-click `Lamazon.Services` → **Manage NuGet Packages** → `Microsoft.Extensions.Http`, version **10.0.13**. It brings `AddHttpClient` for a class library: `Lamazon.Web` gets it from ASP.NET Core, a plain class library doesn't. With Central Package Management the version lands in `Directory.Packages.props`, as in class 3.

📄 **Handout: the answer of the API**, `Lamazon.Services/Models/IpGeoInfo.cs`:
```csharp
namespace Lamazon.Services.Models;

/// <summary>
/// The answer of http://ip-api.com/json/{ip}?fields=status,message,countryCode, for example:
/// { "status": "success", "countryCode": "MK" }   or   { "status": "fail", "message": "private range" }
/// The API can send much more (country name, city, coordinates, ...), but the "fields" parameter asks only
/// for what we use. System.Text.Json matches the names without caring about upper and lower case.
/// </summary>
public class IpGeoInfo
{
    /// <summary>The fields to ask for: the names of the properties below.</summary>
    public const string Fields = "status,message,countryCode";

    /// <summary>"success" or "fail".</summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>Why it failed, for example "private range" for 192.168.x.x, or "reserved range" for 127.0.0.1.</summary>
    public string? Message { get; set; }

    /// <summary>Two letters (ISO 3166), for example "MK".</summary>
    public string? CountryCode { get; set; }

    public bool IsSuccess => Status == "success";
}
```
It lives in `Lamazon.Services/Models`, not in `ViewModels`: it's the shape of an external API, and no view ever shows it.

**The service**, `Lamazon.Services/Abstractions/IGeoTrackerService.cs`:
```csharp
using Lamazon.Services.Models;

namespace Lamazon.Services.Abstractions;

/// <summary>
/// Finds out where an IP address is, with an external API (ip-api.com).
/// </summary>
public interface IGeoTrackerService
{
    /// <summary>The location of the IP address, or null if the API can't tell (private address, API down, ...).</summary>
    Task<IpGeoInfo?> GetIpGeoInfoAsync(string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>A small picture of the country's flag, for example for "MK".</summary>
    string GetCountryFlagUrl(string countryCode);
}
```
And `Lamazon.Services/Implementations/GeoTrackerService.cs`:
```csharp
using System.Net.Http.Json;
using System.Text.Json;
using Lamazon.Services.Abstractions;
using Lamazon.Services.Models;
using Microsoft.Extensions.Logging;

namespace Lamazon.Services.Implementations;

/// <summary>
/// Calls ip-api.com. The HttpClient comes from IHttpClientFactory, already set up in DependencyInjection.cs
/// with the base address.
/// </summary>
public class GeoTrackerService : IGeoTrackerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GeoTrackerService> _logger;

    public GeoTrackerService(HttpClient httpClient, ILogger<GeoTrackerService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<IpGeoInfo?> GetIpGeoInfoAsync(string ipAddress, CancellationToken cancellationToken = default)
    {
        try
        {
            // Relative to the base address: http://ip-api.com/json/77.28.200.111?fields=status,message,countryCode
            string url = $"json/{Uri.EscapeDataString(ipAddress)}?fields={IpGeoInfo.Fields}";
            IpGeoInfo? ipGeoInfo = await _httpClient.GetFromJsonAsync<IpGeoInfo>(url, cancellationToken);

            if (ipGeoInfo is null || !ipGeoInfo.IsSuccess)
            {
                _logger.LogWarning("No location for IP {IpAddress}: {Message}", ipAddress, ipGeoInfo?.Message);
                return null;
            }

            return ipGeoInfo;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or JsonException)
        {
            // The location is nice to have, not required: the order is saved without it
            _logger.LogWarning(exception, "The geo-location API failed for IP {IpAddress}", ipAddress);
            return null;
        }
    }

    public string GetCountryFlagUrl(string countryCode)
    {
        // flagcdn.com only understands lower-case country codes: mk, not MK
        return $"https://flagcdn.com/w80/{countryCode.ToLowerInvariant()}.png";
    }
}
```
| Piece | Why |
|---|---|
| `HttpClient httpClient` in the constructor | the typed client: the factory hands it over, with the base address already set |
| `GetFromJsonAsync<IpGeoInfo>` | sends the GET, checks the status code and turns the JSON into an `IpGeoInfo`, in one call |
| `Uri.EscapeDataString(ipAddress)` | an IPv6 address has `:` in it. Escaping keeps the URL valid, whatever the address looks like |
| `return null` instead of an exception | the country is nice to have. A failing API must never stop a customer from ordering |
| `HttpRequestException` | no connection, or an error status code (`5xx`, `429`) |
| `TaskCanceledException` | the API took too long: `HttpClient`'s timeout (100 seconds by default) |
| `JsonException` | the API answered with something that isn't our JSON |

**Register the typed client.** In `Lamazon.Services/DependencyInjection.cs`, a new private method:
```csharp
private static void AddGeoTracker(this IServiceCollection services)
{
    // A "typed client": GeoTrackerService receives an HttpClient made by IHttpClientFactory
    services.AddHttpClient<IGeoTrackerService, GeoTrackerService>(client =>
    {
        // ip-api.com is free over plain HTTP only (HTTPS is a paid plan)
        client.BaseAddress = new Uri("http://ip-api.com/");
    });
}
```
And call it at the end of `AddServices()`, before `return services;`:
```csharp
// The HttpClient for the geo-location API
services.AddGeoTracker();
```
⚠️ **Don't also add `services.AddScoped<IGeoTrackerService, GeoTrackerService>()`.** `AddHttpClient<…>` already registers the service. A second registration would replace it, and `GeoTrackerService` would get a plain `HttpClient` with no base address.

🔁 **`AddHttpClient()` in `AddWeb()` stays.** `AddHttpClient<…>` registers the factory too, but the middleware lives in `Lamazon.Web`, so `Lamazon.Web` registers what it uses. Registering the factory twice is harmless.

**`OrdersService` asks for the location.** One more field and constructor parameter, `IGeoTrackerService geoTrackerService` (add `using Lamazon.Services.Models;` at the top):
```csharp
private readonly IGeoTrackerService _geoTrackerService;
```
In `CreateAsync`, right after `var order = new Order { … };`:
```csharp
// Where the order came from: a country code and a flag (the order is saved without them if the API fails)
await AddLocationAsync(order, cancellationToken);
```
And a new private method:
```csharp
private async Task AddLocationAsync(Order order, CancellationToken cancellationToken)
{
    if (string.IsNullOrEmpty(order.IpAddress))
    {
        return;
    }

    IpGeoInfo? ipGeoInfo = await _geoTrackerService.GetIpGeoInfoAsync(order.IpAddress, cancellationToken);

    // "is { } countryCode": the country code is not null, and from here on it's called countryCode
    if (ipGeoInfo?.CountryCode is { } countryCode)
    {
        order.CountryCode = countryCode;
        order.CountryFlagUrl = _geoTrackerService.GetCountryFlagUrl(countryCode);
    }
}
```
🗄️ **No migration:** `IpAddress`, `CountryCode` and `CountryFlagUrl` have been optional columns of `Orders` since class 2.

🧪 **Try it:** place an order.
```sql
SELECT OrderNumber, IpAddress, CountryCode, CountryFlagUrl FROM Orders;
```
The new order has your public IP, `MK` and `https://flagcdn.com/w80/mk.png`. Open the flag's URL in the browser. In class 9, the CMS orders table shows it.

---

### 🚧 `BusinessRuleException`: an order with an empty cart
`OrdersService.CreateAsync` still has a placeholder from class 6:
```csharp
if (products.Count == 0)
{
    throw new Exception();
}
```
A plain `Exception` says nothing, and `AppExceptionFilter` doesn't know it, so the user gets the error page. But an empty cart isn't a bug: the user broke a **business rule**, and should see why. `NotFoundException` got its own class in class 3, so this case gets one too, next to it in `Lamazon.Domain/Exceptions/BusinessRuleException.cs`:
```csharp
namespace Lamazon.Domain.Exceptions;

public class BusinessRuleException : AppException
{
    public BusinessRuleException(string message) : base(message)
    {
    }
}
```
In `OrdersService` (add `using Lamazon.Domain.Exceptions;` at the top):
```csharp
if (products.Count == 0)
{
    throw new BusinessRuleException("Your shopping cart is empty.");
}
```
**The filter gets its second `case`**, as promised in class 4. `Lamazon.Web/Filters/AppExceptionFilter.cs`:
```csharp
using Lamazon.Domain.Exceptions;
using Lamazon.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Lamazon.Web.Filters;

public class AppExceptionFilter : IExceptionFilter
{
    private readonly ITempDataDictionaryFactory _tempDataFactory;

    // options.Filters.Add<AppExceptionFilter>() lets dependency injection create the filter, so the constructor can ask for services
    public AppExceptionFilter(ITempDataDictionaryFactory tempDataFactory)
    {
        _tempDataFactory = tempDataFactory;
    }

    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case NotFoundException notFound:
                // log info...
                context.Result = new NotFoundResult();
                context.ExceptionHandled = true;
                break;

            case BusinessRuleException businessRule:
                // Back to the page the user came from, with the message in a red notification
                var tempData = _tempDataFactory.GetTempData(context.HttpContext);
                tempData[Notifications.MessageKey] = businessRule.Message;
                tempData[Notifications.TypeKey] = "error";
                context.Result = new RedirectResult(GetPreviousPage(context.HttpContext.Request));
                context.ExceptionHandled = true;
                break;
        }
    }

    private static string GetPreviousPage(HttpRequest request)
    {
        // The Referer header is the page the user came from. Only follow it back to our own site.
        if (Uri.TryCreate(request.Headers.Referer.ToString(), UriKind.Absolute, out var referer)
            && referer.Host == request.Host.Host)
        {
            return referer.PathAndQuery;
        }

        return "/";
    }
}
```
| Piece | Why |
|---|---|
| `BusinessRuleException : AppException` | the user did something the shop doesn't allow. The message is written for the user, so the filter can show it as it is |
| `ITempDataDictionaryFactory` | a filter isn't a controller, so it has no `TempData` property. The factory gives it the same `TempData` that `AddErrorNotification` in `BaseController` writes to |
| `Notifications.MessageKey`, `TypeKey` | the same keys `_Notification.cshtml` reads (class 6), so the red pop-up needs no new code |
| `Referer` | the browser sends the address of the page the form was on. We go back there, and only if it's our own site, never to an address someone else chose |
| any other exception | no `case` for it: it's a bug, and it still goes to the error page |

🧪 **Try it:** put a product in the cart and open the cart page in **two tabs**. In the first tab, **Create order**. In the second tab, **Create order** again: the cart is empty now, so `CreateAsync` throws. The filter sends you back to the cart page, the empty cart sends you on to the products, and a red *"Your shopping cart is empty."* pops up there. `TempData` waits until a page reads it.

---

## ✅ By the end of the class
- 🚦 You can explain why the timing middleware logged two lines for `/does-not-exist` below `UseStatusCodePagesWithReExecute`, and one line above it
- 🌐 A new order saves your public IP address instead of `::1`
- 🏳️ A new order has a country code and a flag URL. With a broken API address, the order is still saved, without them
- 🚧 Ordering with an empty cart shows a red "Your shopping cart is empty." instead of the error page

---

🔜 **Next class:** the CMS grows. A custom tag helper marks the current page in the sidebar, the dashboard shows the numbers, and the product categories get their first table with DataTables, paged, searched and sorted by the server.
