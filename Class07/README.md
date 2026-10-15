# 🌍 Class 07: Middleware, HttpClient, Polly, logging and the CMS area

> 🧑‍💼 The CMS · class 7 of 10 · 🏠 [Course overview](../README.md) · ⬅️ [Class 06](../Class06/README.md) · 🗂️ [The domain](../Class01/domain.md) · 🗺️ [Database diagram](../Class01/Lamazon%20DB%20Diagram.png)

The shop is finished, so today it learns about the world around it. Every request goes through a **pipeline of middleware**. Every new order asks a **web API** in which country the customer is, and that call survives a slow or failing API thanks to **Polly**. The app writes **logs** to the console, to a file and to a database table. Then the **CMS** starts: an area of the app that only admins can open, with its own layout.

## 🗺️ What we do today

1. 🔁 Recap of class 6: the cart page and placing an order
2. 🚦 Middleware: the request pipeline, and why the order matters
3. 🌐 A middleware class: `DebugIpAddressMiddleware`
4. 📡 `IHttpClientFactory` and a typed client: every order gets a country and a flag
5. 🛡️ Polly: retries and timeouts for an API that fails
6. 📝 Logging: `ILogger<T>`, Serilog, and the `Logs` table
7. 🧑‍💼 The CMS: an area for admins only
8. 🎨 The CMS layout with Material Dashboard (a handout)

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

🗑️ **Delete the `app.Use(...)` block** (and the `using`) when you're done. Logging properly comes later today.

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
| `ILogger<DebugIpAddressMiddleware>` | ASP.NET Core's own logging: the warning goes to the console. More about logging later today |
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
| **`IHttpClientFactory`** | it reuses the connections and renews them every 2 minutes, so it avoids both problems. The settings (the base address, Polly) are in one place, and every call is logged |

**A typed client** is a class that receives a ready `HttpClient` in its constructor. The factory creates that client with our settings.

📦 **The package:** right-click `Lamazon.Services` → **Manage NuGet Packages** → `Microsoft.Extensions.Http.Resilience`, version **10.10.0**. It brings `AddHttpClient` for a class library, and Polly for the next section. With Central Package Management the version lands in `Directory.Packages.props`, as in class 3.

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
using Polly.Timeout;

namespace Lamazon.Services.Implementations;

/// <summary>
/// Calls ip-api.com. The HttpClient comes from IHttpClientFactory, already set up in DependencyInjection.cs
/// with the base address (and, with Polly, retries and timeouts).
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
        catch (Exception exception) when (exception is HttpRequestException or TimeoutRejectedException or JsonException)
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
| `TimeoutRejectedException` | Polly's timeout (next section) |
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
⚠️ **Don't also add `services.AddScoped<IGeoTrackerService, GeoTrackerService>()`.** `AddHttpClient<…>` already registers the service. A second registration would replace it, and `GeoTrackerService` would get a plain `HttpClient`: no base address, and no Polly.

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

### 🛡️ Resilience with Polly
An API on the internet fails sometimes: a dropped connection, a `503 Service Unavailable` for a second, a `429 Too Many Requests`. Often the **second try works**. And sometimes the API is just slow, and we don't want the customer to wait forever.

**Polly** is .NET's resilience library. `Microsoft.Extensions.Http.Resilience` (already installed) plugs it into `IHttpClientFactory`: every request of our typed client goes through a pipeline of strategies:
```
GetFromJsonAsync
  → 1. total timeout: 10 s          the whole call, retries included
    → 2. retry: up to 3 more tries    only after a temporary failure
      → 3. attempt timeout: 3 s       one single try
        → ip-api.com
```

📄 **Handout:** add it to `AddGeoTracker()`, right after the `AddHttpClient<…>(…)` call, before its `;`. Two `using`s at the top: `Microsoft.Extensions.Http.Resilience` and `Polly`.
```csharp
services.AddHttpClient<IGeoTrackerService, GeoTrackerService>(client =>
    {
        // ip-api.com is free over plain HTTP only (HTTPS is a paid plan)
        client.BaseAddress = new Uri("http://ip-api.com/");
    })
    // Resilience with Polly: an external API can fail for a moment (network error, 5xx, 429 Too Many Requests)
    .AddResilienceHandler("geo-tracker", pipeline =>
    {
        // 1. The whole call, retries included, may take at most 10 seconds
        pipeline.AddTimeout(TimeSpan.FromSeconds(10));

        // 2. After a temporary failure, try again up to 3 times: after ~0.5s, ~1s, ~2s
        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromMilliseconds(500),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
        });

        // 3. Each single attempt may take at most 3 seconds
        pipeline.AddTimeout(TimeSpan.FromSeconds(3));
    });
```
| Piece | Why |
|---|---|
| `HttpRetryStrategyOptions` | retries only **temporary** failures: no connection, `5xx`, `408`, `429`, and an attempt that timed out. A `400` or a `404` stays the same however often you ask |
| `BackoffType.Exponential` | wait longer each time (about 0.5 s, 1 s, 2 s), so a struggling API gets time to recover |
| `UseJitter = true` | a bit of randomness in each wait, so a thousand clients don't all retry at the same moment |
| the order of the strategies | the outer timeout limits everything inside it. If the 3-second timeout were outside the retry, it would limit all four tries together |
| after the last try | the exception reaches `GeoTrackerService`, which logs a warning and returns `null`. The order is saved without a country |

🧪 **Try it: an API that fails.**
1. Change the base address to `http://ip-api.invalid/` (a host that can't exist), restart, and place an order. The console shows four attempts (`Attempt: '0'` to `'3'`) with an `OnRetry` warning between them, then `The geo-location API failed for IP …`. The order is saved, without a country, about 2 seconds later.
2. Put the address back, and make the attempt timeout tiny: `TimeSpan.FromMilliseconds(10)`. Now each attempt ends with `OnTimeout`, and the order again gets no country. Polly refuses anything under 10 ms: *"The field Timeout must be between 00:00:00.0100000 and 1.00:00:00"*.
3. Put it back to `TimeSpan.FromSeconds(3)`.

---

### 📝 Logging
#### `ILogger<T>`
We have logged since this morning: `DebugIpAddressMiddleware` and `GeoTrackerService` receive an `ILogger<T>`, and their warnings appear in the console. `ILogger<T>` is .NET's logging **abstraction**: the code says *what* happened, and the configuration decides *where* it goes. `T` is the **category**, the class that writes the line, so you can see where a line came from.

**The levels**, from the least to the most important:
| Level | When | In Lamazon |
|---|---|---|
| `Trace`, `Debug` | details for the developer | not used |
| `Information` | something normal happened | an order was created |
| `Warning` | something unexpected, but the app copes | the geo-location API failed; a wrong password |
| `Error` | an operation failed | an exception nobody handled |
| `Critical` | the whole app is in trouble | the database is gone |

**A message template, not string interpolation:**
```csharp
// ✅ A template: the line is "Order 5/2026 was created by user 2", AND the values are kept separately
//    (OrderNumber = "5/2026", UserId = 2), so a log tool can search and filter by them
_logger.LogInformation("Order {OrderNumber} was created by user {UserId}", order.OrderNumber, userId);

// ❌ Interpolation: one finished string. The values are lost, and the string is built even when the level is switched off
_logger.LogInformation($"Order {order.OrderNumber} was created by user {userId}");
```
🧯 **An exception goes first:** `_logger.LogWarning(exception, "The geo-location API failed for IP {IpAddress}", ipAddress)` logs the message **and** the exception with its stack trace.

🔐 **Never log** passwords, tokens or card numbers. Log a user's id, not their email: logs are read by many people and kept for a long time.

#### Serilog
ASP.NET Core's built-in logging writes to the console and to Visual Studio's Output window. It can't write to a file or to a database. **Serilog** is a logging library that can, with **sinks**: Console, File, MSSqlServer, and many more. Our code doesn't change: it still uses `ILogger<T>`, and Serilog becomes the place where the lines go. That's the point of the abstraction.

📦 **The packages:** right-click `Lamazon.Web` → **Manage NuGet Packages**:
| Package | Version | What for |
|---|---|---|
| `Serilog.AspNetCore` | 10.0.0 | Serilog in ASP.NET Core, with the Console and File sinks |
| `Serilog.Enrichers.Environment` | 3.0.1 | adds the Windows user name to every line |
| `Serilog.Sinks.MSSqlServer` | 10.0.0 | writes to a SQL Server table |

📄 **Handout:** in `Lamazon.Web/Extensions/DependencyInjection.cs`, a second method next to `AddWeb()`. Read it together, don't type it. The `using`s at the top: `System.Collections.ObjectModel`, `System.Data`, `System.Globalization`, `Serilog`, `Serilog.Events` and `Serilog.Sinks.MSSqlServer`.
```csharp
/// <summary>
/// Serilog: structured logging to the console, to a monthly file in Logs/ and, from Warning up, to the Logs table.
/// </summary>
public static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
{
    var connectionString = configuration.GetConnectionString("LamazonDb");
    var logFilePath = Path.Combine(environment.ContentRootPath, "Logs", "lamazon-.txt");

    // The columns Serilog fills in the Logs table. The table itself comes from the EF Core migration (LogEntry).
    var columnOptions = new ColumnOptions
    {
        Store = new Collection<StandardColumn>
        {
            StandardColumn.Id,
            StandardColumn.TimeStamp,
            StandardColumn.Level,
            StandardColumn.Message,
            StandardColumn.Exception,
        },
        AdditionalColumns = new Collection<SqlColumn>
        {
            // Filled by Enrich.WithEnvironmentUserName(): the Windows user the app runs as
            new SqlColumn(columnName: "EnvironmentUserName", dataType: SqlDbType.NVarChar, dataLength: 100, allowNull: true),
        },
    };

    // UTC, like every other date in the database (the console and the file show local time: people read those)
    columnOptions.TimeStamp.ConvertToUtc = true;

    services.AddSerilog(logger => logger
        .MinimumLevel.Information()
        // ASP.NET Core and EF Core write a lot at Information level: only their warnings and errors
        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
        .MinimumLevel.Override("System", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        // Fills the EnvironmentUserName column of the Logs table
        .Enrich.WithEnvironmentUserName()
        // formatProvider: numbers and dates in the logs look the same on every computer
        .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
        .WriteTo.File(
            path: logFilePath,
            rollingInterval: RollingInterval.Month,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
            formatProvider: CultureInfo.InvariantCulture)
        .WriteTo.MSSqlServer(
            connectionString: connectionString,
            sinkOptions: new MSSqlServerSinkOptions
            {
                TableName = "Logs",
                // The migration creates the table. With true, Serilog would connect to the database while the
                // app starts, and crash the app (and dotnet ef) when the database doesn't exist yet.
                AutoCreateSqlTable = false,
            },
            restrictedToMinimumLevel: LogEventLevel.Warning,
            formatProvider: CultureInfo.InvariantCulture,
            columnOptions: columnOptions));

    return services;
}
```
| Piece | What it does |
|---|---|
| `MinimumLevel.Information()` | `Debug` and `Trace` lines are skipped |
| `Override("Microsoft", Warning)` | ASP.NET Core and EF Core: only their warnings and errors. Until now, every SQL query was in the console |
| `Override("Microsoft.Hosting.Lifetime", Information)` | except "Now listening on: https://localhost:…" when the app starts |
| `WriteTo.Console` | the console, as before, in a shorter format: `[22:27:48 INF] Order 1/2026 was created …` |
| `WriteTo.File` + `RollingInterval.Month` | `Lamazon.Web/Logs/lamazon-202610.txt`: a new file every month |
| `WriteTo.MSSqlServer` + `restrictedToMinimumLevel` | the `Logs` table, only `Warning` and above. A database is no place for every page view |
| `ColumnOptions` | the columns Serilog fills: the five standard ones our table has, plus `EnvironmentUserName` |
| `AutoCreateSqlTable = false` | the table comes from a migration (next section) |
| `ConvertToUtc` | the database stores UTC, like the orders (class 6) |

**In `Program.cs`**, first thing after `CreateBuilder`:
```csharp
// Serilog replaces the built-in logging: every ILogger<T> in the app writes to the console, a file and SQL Server
builder.Services.AddSerilogLogging(builder.Configuration, builder.Environment);
```
🧹 **Delete the `"Logging"` section** from `appsettings.json` and from `appsettings.Development.json`. Serilog doesn't read it: its levels are in `AddSerilogLogging`. Even `"Default": "Critical"` there changes nothing, so the section would only mislead. `appsettings.Development.json` is then an empty `{ }`.

🙈 **The `Logs` folder is in `.gitignore`** (the Visual Studio template's `[Ll]ogs/` line), so log files never get committed.

#### The `Logs` table
Serilog writes the rows, but the table comes from **EF Core**, like every other table: a migration creates it, and `Update-Database` builds the whole database from scratch on any computer.

📄 **Handout: the entity**, `Lamazon.Domain/Entities/LogEntry.cs`:
```csharp
namespace Lamazon.Domain.Entities;

/// <summary>
/// One row of the Logs table. Serilog writes these rows (warnings and errors, see AddSerilogLogging in the Web project);
/// EF Core only creates the table, with the migrations, so it exists before the app starts.
/// </summary>
public class LogEntry : BaseEntity
{
    public DateTime TimeStamp { get; set; }
    public string? Level { get; set; }
    public string? Message { get; set; }
    public string? Exception { get; set; }

    /// <summary>The Windows user the app runs as (Serilog's Enrich.WithEnvironmentUserName()).</summary>
    public string? EnvironmentUserName { get; set; }
}
```
📄 **Handout: the configuration**, `Lamazon.DataAccess/Configurations/LogEntryConfiguration.cs`:
```csharp
using Lamazon.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Lamazon.DataAccess.Configurations;

/// <summary>
/// The Logs table, in the shape Serilog's SQL Server sink expects (see the ColumnOptions in the Web project).
/// </summary>
public class LogEntryConfiguration : IEntityTypeConfiguration<LogEntry>
{
    public void Configure(EntityTypeBuilder<LogEntry> builder)
    {
        builder.ToTable("Logs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Level).HasMaxLength(128);
        builder.Property(x => x.EnvironmentUserName).HasMaxLength(100);

        // The newest logs are read first
        builder.HasIndex(x => x.TimeStamp);
    }
}
```
**In `LamazonDbContext`**, after `Users`:
```csharp
// Written by Serilog, not by our code. EF Core creates the table.
public DbSet<LogEntry> Logs { get; set; }
```
**A new migration**, the first since class 3. In the Package Manager Console, with `Lamazon.DataAccess` as the default project:
```
Add-Migration AddLogs
```
📋 Open the new file and check `Up()`: one `CreateTable("Logs", …)` and one `CreateIndex` on `TimeStamp`, nothing else. If it changes other tables, your model and your database don't match: ask before you go on. Then:
```
Update-Database
```
In SSMS, refresh **Tables**: `dbo.Logs` is there, empty.

🤔 **Why not `AutoCreateSqlTable = true`?** Serilog would then create the table itself, and connect to the database while the app starts. On a computer without the database yet, the app crashes at startup, and so does `Add-Migration`, which starts the app to read the model. Last year's app had exactly that problem.

**The first log line of our own**, in `OrdersService`: one more field and constructor parameter, `ILogger<OrdersService> logger` (with `using Microsoft.Extensions.Logging;`), and at the end of `CreateAsync`:
```csharp
await _ordersRepository.AddAsync(order, cancellationToken);

// A message template: {OrderNumber}, {UserId} and {TotalAmount} are filled in AND kept as separate values
_logger.LogInformation("Order {OrderNumber} was created by user {UserId}, total {TotalAmount}", order.OrderNumber, userId, order.TotalAmount);
return order.OrderNumber;
```
After the save, so the line is only written for an order that really exists.

🧪 **Try it:** use the `https` launch profile (the `http` one logs "Failed to determine the https port" as a warning at every start).
| Do | Console and `Logs/lamazon-….txt` | `Logs` table |
|---|---|---|
| start the app | `[… INF] Now listening on: https://localhost:…`, and no more SQL queries | nothing |
| place an order | `[… INF] Order 2/2026 was created by user 2, total 49.50` | nothing: it's `Information` |
| the wrong-host demo from the Polly section | the four attempts and our warning | one row per warning and error, with the time in UTC and your Windows user name |

```sql
SELECT TOP 20 * FROM Logs ORDER BY Id DESC;
```
In the table, the values inside a message are in quotes (`IP "79.125.206.236"`). The file's `{Message:lj}` writes them without.

---

### 🧑‍💼 The CMS: an area
The CMS is a second app inside the same project: its own controllers, its own views, its own layout, and its own address, `/Administration`. ASP.NET Core calls that an **area**:
```
Lamazon.Web/
├── Areas/
│   └── Administration/
│       ├── Controllers/     AdminBaseController, HomeController
│       └── Views/
│           ├── _ViewImports.cshtml, _ViewStart.cshtml
│           ├── Shared/      _Layout, _Sidebar, _Navbar, _Footer
│           └── Home/        Index.cshtml
├── Controllers/             the shop's controllers
└── Views/                   the shop's views
```
Create the folders `Areas/Administration/Controllers` and `Areas/Administration/Views/Home` by hand (right-click → **Add** → **New Folder**).

**One base class for every admin controller**, `Areas/Administration/Controllers/AdminBaseController.cs`:
```csharp
using Lamazon.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Lamazon.Web.Areas.Administration.Controllers;

/// <summary>
/// Base class of every controller in the Administration area (the CMS).
/// Attributes on a base class are inherited: every admin controller is in the area and is for admins only.
/// </summary>
[Area("Administration")]
[Authorize(Roles = Roles.Admin)]
public abstract class AdminBaseController : Controller
{
    /// <summary>The title shown in the admin navbar. Each controller sets it in its constructor.</summary>
    protected string PageName { get; init; } = string.Empty;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        ViewBag.PageName = PageName;

        base.OnActionExecuting(context);
    }
}
```
| Piece | Why |
|---|---|
| `[Area("Administration")]` | the controller belongs to the area: its views are in `Areas/Administration/Views`, and its address starts with `/Administration` |
| `[Authorize(Roles = Roles.Admin)]` | only a user whose login cookie has the role claim `admin` (class 5's `AuthHelper`) |
| on the **base class** | attributes are inherited. Every admin controller from now on is protected, and nobody can forget it |
| `PageName` | the title in the CMS navbar and in the browser tab. `init`: set once, in the constructor |
| `abstract` | MVC never routes to it, like the shop's `BaseController` |

**The first page**, `Areas/Administration/Controllers/HomeController.cs`:
```csharp
using Microsoft.AspNetCore.Mvc;

namespace Lamazon.Web.Areas.Administration.Controllers;

// The same class name as the shop's HomeController: a different namespace, and [Area] (from AdminBaseController) keeps them apart
public class HomeController : AdminBaseController
{
    public HomeController()
    {
        PageName = "Dashboard";
    }

    public IActionResult Index()
    {
        // The dashboard's numbers come next class
        return View();
    }
}
```
And its view, `Areas/Administration/Views/Home/Index.cshtml`:
```cshtml
@* For now only a welcome card. The dashboard's numbers come next class. *@
<div class="card">
    <div class="card-header card-header-primary">
        <h4 class="card-title">Welcome to the Lamazon CMS</h4>
        <p class="card-category">Only admins can open this page</p>
    </div>
    <div class="card-body">
        <p>Next class: the dashboard, with the customers, the orders, the invoices and the sales.</p>
    </div>
</div>
```
**The route.** In `Program.cs`, **before** the `default` route:
```csharp
// /Administration/Products/Edit/5 → Area = Administration, ProductsController.Edit(5) of the area
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
```
- 🧭 **`{area:exists}`** matches only the name of an area that exists: `/Administration` does, `/Products` doesn't, so `/Products` still goes to the `default` route.
- ⬆️ **Before `default`:** the more specific route comes first.
- 🏠 **`/Administration`** alone → the area's `Home` controller and its `Index` action, from the defaults.

**`asp-area` in links.** Inside the area, a link without `asp-area` **stays in the area**: `asp-controller="Home"` would point to the CMS's home page. That's why every link in the shop's `_Navbar` and `_Footer` has had `asp-area=""` since class 3. Now the navbar's "Lamazon CMS" item finally gets its link, in `Views/Shared/_Navbar.cshtml`:
```cshtml
<li><a class="dropdown-item" asp-area="Administration" asp-controller="Home" asp-action="Index">Lamazon CMS</a></li>
```

🧪 **Try it:**
| Do | You should see |
|---|---|
| logged out, open `/Administration` | the login page, and after the login back to the CMS (`returnUrl`, class 5) |
| logged in as `user@lamazon.com`, open `/Administration` | the AccessDenied page (the class 5 homework) |
| logged in as `admin@lamazon.com`: the user menu → **Lamazon CMS** | the welcome card, as plain text without any style |

🤔 **Why without any style?** `Views/_ViewStart.cshtml` (which sets the layout) and `Views/_ViewImports.cshtml` (the `@using`s and the tag helpers) apply only to the views in `Views/` and its subfolders. `Areas/Administration/Views` isn't one of them. The area needs its own, next.

### 🎨 The CMS layout (handout)
The CMS uses **Material Dashboard 2.1.0**, a free (MIT) admin theme built on **Bootstrap 4**. The CMS doesn't load the shop's Bootstrap 5: two Bootstraps on one page fight over the same class names. Last year's CMS loaded both, and its pop-up windows needed hacks to close.

**Install it with LibMan**, like Notyf in class 6. Right-click `wwwroot/lib` → **Add** → **Client-Side Library…**:
1. Provider: **jsdelivr**. Library: `material-dashboard@2.1.0`
2. **Choose specific files:**
   - `assets/css/material-dashboard.min.css`
   - `assets/js/core/popper.min.js`
   - `assets/js/core/bootstrap-material-design.min.js`
   - `assets/js/plugins/perfect-scrollbar.jquery.min.js`
   - `assets/js/material-dashboard.min.js`
   - `LICENSE.md`
3. Target location: `wwwroot/lib/material-dashboard/` → **Install**

Five files and the license, about 500 KB, instead of the whole 8 MB package. `libman.json` gets a new entry:
```json
{
  "library": "material-dashboard@2.1.0",
  "destination": "wwwroot/lib/material-dashboard/",
  "files": [
    "assets/css/material-dashboard.min.css",
    "assets/js/core/popper.min.js",
    "assets/js/core/bootstrap-material-design.min.js",
    "assets/js/plugins/perfect-scrollbar.jquery.min.js",
    "assets/js/material-dashboard.min.js",
    "LICENSE.md"
  ]
}
```
⚠️ **`LICENSE.md` in capitals.** The file names are case-sensitive on the CDN. With `license.md`, the build fails with *LIB018: "material-dashboard@2.1.0" does not contain the following: license.md*, because our project restores `libman.json` on every build.

📄 **Handout: the area's own `_ViewStart` and `_ViewImports`.** `Areas/Administration/Views/_ViewStart.cshtml`:
```cshtml
@{
    Layout = "_Layout";
}
```
`"_Layout"` is looked up next to the view first, then in `Areas/Administration/Views/Shared`, and only then in the shop's `Views/Shared`.

`Areas/Administration/Views/_ViewImports.cshtml`:
```cshtml
@using Lamazon.ViewModels.Models
@using Lamazon.Web.Extensions
@using Lamazon.Web.Models
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```
Without the `@addTagHelper` line, `asp-controller` and the other `asp-` attributes would stay as plain text in the HTML. Without `Lamazon.Web.Extensions`, the navbar's `User.DisplayName()` doesn't build.

📄 **Handout: the layout**, `Areas/Administration/Views/Shared/_Layout.cshtml`:
```cshtml
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewBag.PageName - Lamazon CMS</title>
    <link rel="icon" href="~/favicon.ico" />
    <link rel="stylesheet" href="https://fonts.googleapis.com/css?family=Roboto:300,400,500,700|Roboto+Slab:400,700|Material+Icons" />
    @* Material Dashboard brings its own Bootstrap 4 (CSS and JS): the CMS doesn't load the shop's Bootstrap 5 *@
    <link rel="stylesheet" href="~/lib/material-dashboard/assets/css/material-dashboard.min.css" />
    <link rel="stylesheet" href="~/lib/notyf/notyf.min.css" />
    <link rel="stylesheet" href="~/css/admin-site.css" asp-append-version="true" />
</head>
<body>
    <div class="wrapper">
        <partial name="_Sidebar" />

        <div class="main-panel">
            <partial name="_Navbar" />

            <div class="content">
                <div class="container-fluid">
                    @RenderBody()
                </div>
            </div>

            <partial name="_Footer" />
        </div>
    </div>

    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/material-dashboard/assets/js/core/popper.min.js"></script>
    <script src="~/lib/material-dashboard/assets/js/core/bootstrap-material-design.min.js"></script>
    <script src="~/lib/material-dashboard/assets/js/plugins/perfect-scrollbar.jquery.min.js"></script>
    <script src="~/lib/material-dashboard/assets/js/material-dashboard.min.js"></script>
    <script src="~/lib/notyf/notyf.min.js"></script>
    @* The same partial as the shop: Views/Shared is searched after Areas/Administration/Views/Shared *@
    <partial name="_Notification" />
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```
- 🏷️ **`ViewBag.PageName`**, from `AdminBaseController`: the browser tab says "Dashboard - Lamazon CMS".
- 📦 **jQuery** is the shop's copy in `wwwroot/lib/jquery`: Material Dashboard's JavaScript needs it.
- 🔔 **`_Notification`** isn't copied: a partial is looked up in the area's `Shared` folder first, then in `Views/Shared`. The CMS shows the same pop-ups as the shop.

📄 **Handout: the sidebar**, `Areas/Administration/Views/Shared/_Sidebar.cshtml`:
```cshtml
@* Every page of the CMS, from the start. Until its class, a link answers with the "not found" page. *@
<div class="sidebar">
    <div class="logo">
        <a asp-area="Administration" asp-controller="Home" asp-action="Index" class="simple-text logo-normal">
            <img src="~/images/lamazon.svg" alt="Lamazon" height="100" />
        </a>
    </div>
    <div class="sidebar-wrapper">
        <ul class="nav">
            <li class="nav-item">
                <a class="nav-link" asp-area="Administration" asp-controller="Home" asp-action="Index">
                    <i class="material-icons">dashboard</i>
                    <p>Dashboard</p>
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" asp-area="Administration" asp-controller="ProductCategories" asp-action="Index">
                    <i class="material-icons">category</i>
                    <p>Product Categories</p>
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" asp-area="Administration" asp-controller="Products" asp-action="Index">
                    <i class="material-icons">inventory</i>
                    <p>Products</p>
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" asp-area="Administration" asp-controller="Users" asp-action="Index">
                    <i class="material-icons">person</i>
                    <p>Users</p>
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" asp-area="Administration" asp-controller="Orders" asp-action="Index">
                    <i class="material-icons">description</i>
                    <p>Orders</p>
                </a>
            </li>
            <li class="nav-item">
                <a class="nav-link" asp-area="Administration" asp-controller="Invoices" asp-action="Index">
                    <i class="material-icons">receipt</i>
                    <p>Invoices</p>
                </a>
            </li>
        </ul>
    </div>
</div>
```
📄 **Handout: the navbar**, `Areas/Administration/Views/Shared/_Navbar.cshtml`. Its links back to the shop have `asp-area=""`:
```cshtml
<nav class="navbar navbar-expand-lg navbar-transparent navbar-absolute fixed-top">
    <div class="container-fluid">
        <div class="navbar-wrapper">
            <span class="navbar-brand">@ViewBag.PageName</span>
        </div>

        <div class="collapse navbar-collapse justify-content-end">
            <ul class="navbar-nav">
                <li class="nav-item dropdown">
                    <a class="nav-link" href="#" id="userMenu" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
                        <i class="material-icons">person</i> @User.DisplayName()
                    </a>
                    <div class="dropdown-menu dropdown-menu-right" aria-labelledby="userMenu">
                        <a class="dropdown-item" asp-area="" asp-controller="Home" asp-action="Index">Lamazon shop</a>
                        <form asp-area="" asp-controller="Users" asp-action="Logout" method="post">
                            <button type="submit" class="dropdown-item">Logout</button>
                        </form>
                    </div>
                </li>
            </ul>
        </div>
    </div>
</nav>
```
`data-toggle="dropdown"`, not the shop's `data-bs-toggle`: that's Bootstrap 4.

📄 **Handout: the footer**, `Areas/Administration/Views/Shared/_Footer.cshtml`:
```cshtml
<footer class="footer">
    <div class="container-fluid">
        <nav class="float-left">
            <ul>
                <li>
                    <a asp-area="" asp-controller="Home" asp-action="Privacy">Privacy</a>
                </li>
            </ul>
        </nav>
        <div class="copyright float-right">
            &copy; @DateTime.Now.Year - Lamazon
        </div>
    </div>
</footer>
```
📄 **Handout: the CMS's own styles**, `wwwroot/css/admin-site.css`. The table and form styles are for the next classes:
```css
/* Styles for the CMS (Areas/Administration), on top of Material Dashboard */

.hidden {
    display: none !important;
}

.main-panel > .content {
    padding: 10px;
    padding-top: 0;
}

/* The active page in the sidebar (ActiveRouteTagHelper adds the "active" class) */
.sidebar .nav li.active > a {
    background-color: #634949;
}

/* Tables
-------------------------------------------------- */
.action-button {
    margin-left: 20px;
}

.column-actions {
    width: 100px;
    text-align: right;
}

.btn-add-new,
.btn-search {
    padding: 10px;
}

.txt-search {
    min-width: 180px;
}

.ddl-search {
    padding: 7px 10px;
    min-width: 150px;
}

select.form-control {
    height: 36px !important;
}

.dataTables_length {
    padding-top: 3.5px;
}

.order-country-flag {
    height: 24px;
}

.preview-table > thead > tr > th {
    color: #999;
    font-weight: 400;
    font-size: 14px;
}

.preview-table > tbody > tr > td {
    font-size: 14px;
}

/* Forms
-------------------------------------------------- */
.product-featured-checkbox {
    height: 1.5rem;
    width: 1.5rem;
    margin-top: 7px;
    margin-left: 2px;
}

.card-footer .btn + .btn,
.card-footer .btn + form,
.card-footer form + .btn {
    margin-left: 10px;
}
```
🔨 **Rebuild** after adding files to `wwwroot`: `MapStaticAssets` lists them at build time (class 3).

🧪 **Try it** as `admin@lamazon.com`:
| Do | You should see |
|---|---|
| the user menu → **Lamazon CMS** | the CMS: the sidebar with the logo, "Dashboard" in the navbar and in the browser tab, the welcome card |
| the user menu at the top right → **Lamazon shop** | back in the shop |
| **Product Categories** in the sidebar | the "not found" page: that page comes next class |
| the user menu → **Logout** | logged out, on the shop's home page |

---

## 🏠 Homework
Log lines in two more places. Use message templates, not string interpolation.

### 1. 👤 `UsersService`
- 📥 an `ILogger<UsersService>` through the constructor, like `OrdersService`
- ✅ `Information` when a new user registers, with the new user's id
- ⚠️ `Warning` when the password is wrong for an account that exists, with the user's id. Never the password, and not the email

### 2. 🧯 `AppExceptionFilter`
- 📥 an `ILogger<AppExceptionFilter>` through a new constructor. `options.Filters.Add<AppExceptionFilter>()` in `AddWeb()` lets dependency injection create the filter, so a constructor parameter works like in a controller
- ℹ️ `Information` with the `NotFoundException`'s message, where the comment `// log info...` is. A wrong id in the address bar is normal, not a warning

✅ **Check it:**
| Do | Console and file | `Logs` table |
|---|---|---|
| register a new account | `INF New user 3 registered` | no new row |
| log in with a wrong password | `WRN Failed login for user 2` | a new row |
| open `/Products/Details/999` | `INF Product with id 999 was not found.` | no new row |

Your ids can differ: they're the ids in your own `Users` table.

🆘 **Stuck?** After the next class, compare your files with the same files in the `Class08` folder.

## ✅ By the end of the class
- 🚦 You can explain why the timing middleware logged two lines for `/does-not-exist` below `UseStatusCodePagesWithReExecute`, and one line above it
- 🌐 A new order saves your public IP address instead of `::1`
- 🏳️ A new order has a country code and a flag URL. With a broken API address, the order is still saved, without them, after Polly's retries
- 📝 The console no longer shows every SQL query. `Logs/lamazon-YYYYMM.txt` has the "Order … was created" lines, and the `Logs` table has the warnings
- 🧑‍💼 As the admin, `/Administration` opens the CMS with its sidebar and navbar. As a customer, the AccessDenied page. Logged out, the login page

---

🔜 **Next class:** the CMS grows. A custom tag helper marks the current page in the sidebar, the dashboard shows the numbers, and the product categories get their first table with DataTables, paged, searched and sorted by the server.
