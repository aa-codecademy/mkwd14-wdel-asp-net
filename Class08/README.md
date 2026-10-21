# 🧑‍💼 Class 08: The CMS: area, dashboard and DataTables

> 🧑‍💼 The CMS · class 8 of 10 · 🏠 [Course overview](../README.md) · ⬅️ [Class 07](../Class07/README.md) · 🗂️ [The domain](../Class01/domain.md) · 🗺️ [Database diagram](../Class01/Lamazon%20DB%20Diagram.png)

The shop is finished, and today the **CMS** starts: a part of the app that only admins can open, at `/Administration`, with its own controllers, views and layout. A **custom tag helper** marks the current page in the sidebar, the **dashboard** shows the numbers of the shop, and **DataTables** gets ready for the first admin table: the server will do the paging, the search and the sorting, so a table stays fast however many rows there are.

📂 This page explains the ideas behind today's code, with small pieces of it. Every file is whole in the `Lamazon` folder next to this README: open it next to the explanation.

## 🗺️ What we do today

1. 🔁 Recap of class 7
2. 🧑‍💼 The CMS: an area for admins only
3. 🎨 The CMS layout with Material Dashboard (a handout)
4. 🏷️ A custom tag helper: the current page in the sidebar
5. 📊 The dashboard
6. 📋 DataTables: the contract, the shared defaults and the request (the first table comes in class 9)

---

## 💡 Used concepts

### 🔁 From class 7
Every request goes through the middleware in `Program.cs`, from top to bottom. `DebugIpAddressMiddleware` replaces `::1` with your public IP address, and `GeoTrackerService`, a typed client from `IHttpClientFactory`, asks ip-api.com in which country it is. `BusinessRuleException` turns a broken business rule into a red notification instead of the error page.

🧪 **Check it first:** put a product in the cart and **Create order**. In SSMS:
```sql
SELECT OrderNumber, IpAddress, CountryCode, CountryFlagUrl FROM Orders;
```
The new order has your public IP address, `MK` and the link to the flag.

---

### 🧑‍💼 The CMS: an area
The CMS is a second app inside the same project: its own controllers, its own views, its own layout, and its own address, `/Administration`. ASP.NET Core calls that an **area**: a folder under `Areas/` with its own `Controllers` and `Views`.
```
Lamazon.Web/
├── Areas/
│   └── Administration/
│       ├── Controllers/     AdminBaseController, HomeController, ProductCategoriesController
│       └── Views/
│           ├── _ViewImports.cshtml, _ViewStart.cshtml
│           ├── Shared/      _Layout, _Sidebar, _Navbar, _Footer
│           ├── Home/        Index.cshtml
│           └── ProductCategories/ Index.cshtml
├── Controllers/             the shop's controllers
└── Views/                   the shop's views
```

**`[Area]` puts a controller in the area.** `[Area("Administration")]` changes two things: MVC looks for the controller's views in `Areas/Administration/Views/…` instead of `Views/…`, and the controller's addresses start with `/Administration`. That's why the CMS can have its own `HomeController` next to the shop's: a different namespace and a different area keep them apart.

**One base class for every admin controller.** Attributes on a base class are inherited, so the area and the security are written once:
```csharp
[Area("Administration")] // asp-area="Administration" in the view
[Authorize(Roles = Roles.Admin)]
public abstract class AdminBaseController : Controller
{
    protected string PageName { get; set; } = string.Empty;
    …
}
```
- 🔒 **`[Authorize(Roles = Roles.Admin)]`:** only a user whose login cookie has the role claim `admin` (class 5's `AuthHelper`). A customer gets the AccessDenied page, and a visitor who isn't logged in gets the login page.
- 🧬 **On the base class**, every admin controller from now on is protected, and nobody can forget it on a new page.
- 🏷️ **`PageName`:** each controller sets it once, in its constructor (`PageName = "Dashboard";`). `OnActionExecuting` copies it into `ViewBag.PageName`, and the layout shows it in the navbar and in the browser tab.
- 🚫 **`abstract`:** MVC never routes to it, like the shop's `BaseController`.

**A route for the areas**, in `Program.cs`, **before** the `default` route:
```csharp
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
```
- 🧭 **`{area:exists}`** matches only the name of an area that exists: `/Administration` does, `/Products` doesn't, so `/Products` still goes to the `default` route.
- ⬆️ **Before `default`:** the more specific route comes first.
- 🏠 **`/Administration`** alone → the area's `Home` controller and its `Index` action, from the defaults.

**`asp-area` in links.** A link without `asp-area` stays in the area it's in: inside the CMS, `asp-controller="Home"` points to the CMS's home page. That's why every link in the shop's navbar and footer has had `asp-area=""` since class 3. Now the navbar's "Lamazon CMS" item finally gets its link:
```cshtml
<li><a asp-area="Administration" asp-controller="Home" asp-action="Index" class="dropdown-item">Lamazon CMS</a></li>
```

🧪 **Try it.** The CMS's `HomeController` starts with a welcome card:
| Do | You should see |
|---|---|
| logged out, open `/Administration` | the login page, and after the login back to the CMS (`returnUrl`, class 5) |
| logged in as `user@lamazon.com`, open `/Administration` | the AccessDenied page |
| logged in as `admin@lamazon.com`: the user menu → **Lamazon CMS** | the welcome card, as plain text without any style |

🤔 **Why without any style?** `Views/_ViewStart.cshtml` (which sets the layout) and `Views/_ViewImports.cshtml` (the `@using`s and the tag helpers) apply only to the views in `Views/` and its subfolders. `Areas/Administration/Views` isn't one of them. The area needs its own, next.

### 🎨 The CMS layout (handout)
The CMS uses **Material Dashboard 2.1.0**, a free (MIT) admin theme built on **Bootstrap 4**. The CMS doesn't load the shop's Bootstrap 5: two Bootstraps on one page fight over the same class names. Last year's CMS loaded both, and its pop-up windows needed hacks to close.

📦 **Install it with LibMan**, like Notyf in class 6: from jsdelivr, `material-dashboard@2.1.0` into `wwwroot/lib/material-dashboard/`, with **Choose specific files**. The layout uses five files: `assets/css/material-dashboard.min.css`, and `popper.min.js`, `bootstrap-material-design.min.js`, `perfect-scrollbar.jquery.min.js` and `material-dashboard.min.js` from `assets/js`. Add `LICENSE.md`, and that's about 500 KB instead of the whole 8 MB package.

⚠️ **`LICENSE.md` in capitals.** The file names are case-sensitive on the CDN. With `license.md`, the build fails with *LIB018: "material-dashboard@2.1.0" does not contain the following: license.md*, because our project restores `libman.json` on every build.

**The area gets its own `_ViewStart` and `_ViewImports`.** Both apply to the views in their folder and below, so the area's copies apply to the CMS only:
- 📐 **`_ViewStart`** sets `Layout = "_Layout"`. The name is looked up next to the view first, then in `Areas/Administration/Views/Shared`, and only then in the shop's `Views/Shared`. So the CMS finds its own layout.
- 📥 **`_ViewImports`** has the `@using`s (`Lamazon.Web.Extensions`, for example, gives the navbar `User.DisplayName()`) and the line without which every `asp-` attribute would stay plain text in the HTML:
```cshtml
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

**The layout** splits the page into four partials:
```
┌───────────┬────────────────────────────────────────────┐
│           │ _Navbar:  @ViewBag.PageName       👤 menu  │
│ _Sidebar  ├────────────────────────────────────────────┤
│           │                                            │
│ every CMS │   @RenderBody(): the page                  │
│ page      │                                            │
│           ├────────────────────────────────────────────┤
│           │ _Footer                                    │
└───────────┴────────────────────────────────────────────┘
```
- 🏷️ **The browser tab** says `@ViewBag.PageName - Lamazon CMS`, so "Dashboard - Lamazon CMS".
- 📦 **jQuery** is the shop's copy in `wwwroot/lib/jquery`: Material Dashboard's JavaScript needs it.
- 🔔 **`_Notification` isn't copied:** a partial is looked up in the area's `Shared` folder first, then in `Views/Shared`. The CMS shows the same pop-ups as the shop.
- 🔗 **The navbar's user menu** has links back to the shop (`asp-area=""`) and the Logout form. It opens with `data-toggle="dropdown"`, Bootstrap 4's name for the shop's `data-bs-toggle`.
- 🎨 **`wwwroot/css/admin-site.css`** has the CMS's own styles on top of Material Dashboard: the color of the active sidebar item, and the spacing of tables and forms.

**The sidebar** lists every page of the CMS from the start. Until its class, a link answers with the "not found" page. Each item looks like this:
```cshtml
<li is-active-route asp-controller="ProductCategories" class="nav-item">
    <a class="nav-link" asp-area="Administration" asp-controller="ProductCategories" asp-action="Index">
        <i class="material-icons">category</i>
        <p>Product Categories</p>
    </a>
</li>
```
`is-active-route` does nothing yet: it's for the tag helper we write next.

🔨 **Rebuild** after adding files to `wwwroot`: `MapStaticAssets` lists them at build time (class 3).

🧪 **Try it** as `admin@lamazon.com`:
| Do | You should see |
|---|---|
| the user menu → **Lamazon CMS** | the CMS: the sidebar with the logo, "Dashboard" in the navbar and in the browser tab, the welcome card |
| the user menu at the top right → **Lamazon shop** | back in the shop |
| **Product Categories** in the sidebar | an empty page: the categories table comes in class 9 |
| right-click → **View page source**, find `nav-item` | `<li is-active-route asp-controller="Home" class="nav-item">`: the attributes are still plain text |

---

### 🏷️ A custom tag helper: `ActiveRouteTagHelper`
You've used tag helpers since class 3: `asp-controller` on an `<a>` becomes an `href`, and `asp-for` on an `<input>` becomes `name`, `id` and `value`. A **tag helper** is a C# class that changes an HTML element on the server, before the page is sent. We write our own, for the sidebar: the item of the current page gets the `active` class, and `admin-site.css` colors it.
```
in _Sidebar.cshtml:        <li is-active-route asp-controller="Home" class="nav-item">
what the browser gets:     <li class="nav-item active">          (on the dashboard)
                           <li class="nav-item">                 (everywhere else)
```
The heart of `Lamazon.Web/TagHelpers/ActiveRouteTagHelper.cs`:
```csharp
[HtmlTargetElement(Attributes = "is-active-route")]
public class ActiveRouteTagHelper : TagHelper
{
    [HtmlAttributeName("asp-controller")]
    public string? Controller { get; set; }

    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; } = default!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        var currentController = ViewContext.RouteData.Values["controller"]?.ToString();
        var currentAction = ViewContext.RouteData.Values["action"]?.ToString();

        if (IsCurrentRoute(currentController, currentAction))
        {
            output.AddClass("active", HtmlEncoder.Default);
        }

        output.Attributes.RemoveAll("is-active-route");
    }
    …
}
```
| Piece | Why |
|---|---|
| `[HtmlTargetElement(Attributes = "is-active-route")]` | the tag helper runs on **any** element that has an `is-active-route` attribute: here, the sidebar's `<li>` |
| `[HtmlAttributeName("asp-controller")]` | the attribute's value lands in the `Controller` property, the way a form field lands in an action's parameter. An optional `asp-action` works the same way |
| `[ViewContext]` + `[HtmlAttributeNotBound]` | MVC fills it with the current request. It isn't an attribute in the HTML |
| `RouteData.Values["controller"]` | the current request's route values: `ProductCategories`, `Index`, … |
| `IsCurrentRoute(…)` | compares `Controller` (and the action, if given) with the current route's controller and action, ignoring upper and lower case |
| `Process` | changes the element: `AddClass` keeps the classes already there, so `class="nav-item"` becomes `class="nav-item active"` |
| `RemoveAll("is-active-route")` | the marker was only for us, and the browser never sees it. `asp-controller` disappears by itself, because the tag helper used it |
| no `asp-action` in the sidebar | the item stays active on every page of its controller: Index, and from class 9 Create and Edit |

**Register it** in the area's `_ViewImports.cshtml`, at the end:
```cshtml
@addTagHelper *, Lamazon.Web
```
⚠️ **The name after the comma is the assembly**, not a namespace: the project's name, `Lamazon.Web`. Last year's CMS had `@addTagHelper *, Lamazon`. There's no assembly called `Lamazon`, and the build gives **no error**: the tag helpers just never run, and nothing tells you why.

🧪 **Try it:** reload `/Administration`, and **View page source** again: `<li class="nav-item active">` on Dashboard, and plain `<li class="nav-item">` on the others. The Dashboard item is colored.

---

### 📊 The dashboard
The CMS's home page shows the numbers of the shop: customers, products, orders and invoices. The **database** counts and sums them: we never load all the orders into memory to count them in C#. And it takes **four queries**, one per table, not one query per number.
```
IDashboardRepository    one query per table: GROUP BY the status                  DataAccess
DashboardService        4 queries → 13 numbers, + the administrators → one view model     Services
HomeController.Index    the view model → the view                                 Web (the CMS area)
Home/Index.cshtml       four cards and the administrators (a handout)
```

**Every status in one query.** The dashboard needs the orders five ways: all of them, the pending, accepted and rejected ones, and the amount of the accepted ones. Five `COUNT`s and `SUM`s would read the `Orders` table five times. A `GROUP BY` reads it once, and gives one row per status:
```csharp
public async Task<Dictionary<OrderStatusEnum, StatusTotalsModel>> GetOrderTotalsByStatusAsync(CancellationToken cancellationToken = default)
{
    var statusGroups = _dbContext.Orders
        .GroupBy(order => order.OrderStatusId);

    // OrderStatusId = 1 : [...]
    // OrderStatusId = 2 : [...]
    // OrderStatusId = 3 : [...]

    var statusTotals = statusGroups
        .Select(group => new StatusTotalsModel
        {
            StatusId = group.Key,
            Count = group.Count(),
            Amount = group.Sum(order => order.TotalAmount)
        });

    return await statusTotals
        .ToDictionaryAsync(totals => (OrderStatusEnum)totals.StatusId, cancellationToken);
}
```
Two variables, but still one query: `statusGroups` and `statusTotals` are `IQueryable`s that haven't run yet (class 3). Only `ToDictionaryAsync` sends the query, and only its few rows leave the database:
```sql
SELECT [o].[OrderStatusId] AS [StatusId], COUNT(*) AS [Count], COALESCE(SUM([o].[TotalAmount]), 0.0) AS [Amount]
FROM [Orders] AS [o]
GROUP BY [o].[OrderStatusId]
```
- 🧺 **`GroupBy`** puts the orders with the same `OrderStatusId` in one group. In the `Select`, `Key` is the group's status, and `Count()` and `Sum()` work on the orders of that group only.
- 📦 **`StatusTotalsModel`** (in `Lamazon.Domain/Models`) is a plain class for one row of the result: `StatusId`, `Count` and `Amount`.
- 📖 **`ToDictionaryAsync`** runs the query and turns its rows into a dictionary by status, so the service can ask for `OrderStatusEnum.Pending` directly. The cast to `OrderStatusEnum` runs in C#, after the query.
- 🗄️ **Not a `BaseRepository<T>`:** `DashboardRepository` reads several tables and never adds or updates anything, so it gets the `DbContext` directly. The invoices are the same query on `Invoices`, and the products only need the counts (a `Dictionary<ProductStatusEnum, int>`): it's a handout.

**Five queries, one after the other.** `DashboardService` asks the repository four times and `UsersService` once (the administrators, below), then fills a `DashboardViewModel` from the answers:
```csharp
var totalCustomers = await _dashboardRepository.CountCustomersAsync(cancellationToken);
var orders = await _dashboardRepository.GetOrderTotalsByStatusAsync(cancellationToken);
…
var administrators = await _usersService.GetAdministratorsAsync(cancellationToken);

return new DashboardViewModel
{
    // Total Customers
    TotalCustomers = totalCustomers,

    // Orders
    TotalOrders = orders.Values.Sum(totals => totals.Count),
    TotalOrdersAmount = orders.GetValueOrDefault(OrderStatusEnum.Accepted)?.Amount ?? 0,
    TotalPendingOrders = orders.GetValueOrDefault(OrderStatusEnum.Pending)?.Count ?? 0,
    …
    Administrators = administrators
};
```
- 🕳️ **A status that no order has isn't in the dictionary:** `GROUP BY` only makes groups from rows that exist. In a new shop nothing is accepted yet, so there's no `Accepted` key, and `orders[OrderStatusEnum.Accepted]` would throw a `KeyNotFoundException`. `GetValueOrDefault` gives `null` instead, and `?.Amount ?? 0` turns it into 0.
- ➕ **`TotalOrders` needs no query of its own:** every order has exactly one status, so the counts of the groups add up to all the orders.
- 🐢 **Why not all five at once** with `Task.WhenAll`? One `DbContext` runs one query at a time. EF Core would throw *"A second operation was started on this context instance before a previous operation completed"*.
- 💰 **The amounts count only real money:** the **accepted** orders and the **paid** invoices.

**The administrators** are a list, not a number, so they come from the users: `GetAdministratorsAsync` in `UsersRepository`, and in `UsersService`, which maps them to `UserViewModel`s:
```csharp
public async Task<List<User>> GetAdministratorsAsync(CancellationToken cancellationToken = default)
{
    return await Table
        .AsNoTracking()
        .Where(user => user.RoleKey == Roles.Admin)
        .OrderBy(user => user.FullName)
        .ToListAsync(cancellationToken);
}
```
`AsNoTracking()`: the dashboard only reads them, so EF Core doesn't need to watch them for changes.

The repository and the service are registered as scoped in their layer's `DependencyInjection` class, like all the others. The CMS's `HomeController` gets `IDashboardService` in its constructor, and its `Index` passes the view model to the view: four Material Dashboard cards and the table of administrators.

🧪 **Try it:** open `/Administration` and compare the cards with SSMS:
```sql
SELECT (SELECT COUNT(*) FROM Users WHERE RoleKey = 'user') AS Customers,
       (SELECT COUNT(*) FROM Products WHERE ProductStatusId = 1) AS Products,
       (SELECT COUNT(*) FROM Orders) AS Orders,
       (SELECT COUNT(*) FROM Invoices) AS Invoices;
```
And the rows the repository gets for the orders:
```sql
SELECT OrderStatusId, COUNT(*) AS Count, SUM(TotalAmount) AS Amount FROM Orders GROUP BY OrderStatusId;
```
Before the first order there are no rows at all: that's what the `?? 0` is for.

Place one more order in the shop and reload the dashboard: **Orders** and **Pending** go up by one. The amounts stay `$0.00`: nothing is accepted or paid yet. Accepting orders and paying invoices come in the last class.

---

### 📋 DataTables: the server pages, searches and sorts
An admin table can have thousands of rows. **DataTables** is a JavaScript library that turns a plain `<table>` into a table with pages, a search and sortable columns. With **server-side processing**, the browser never gets all the rows: for every page, search or click on a column, DataTables asks the server for just the rows it shows.

**The contract.** DataTables sends a POST with form fields like these (open the browser's dev tools → **Network**, click a request → **Payload**):
```
draw=3                         a counter: we send it back, so DataTables draws the answers in the right order
start=10&length=10             the second page, 10 rows per page (length=-1 means "All")
search[value]=boo              the text in the search box
order[0][column]=0             sorted by the first column…
order[0][dir]=asc              …ascending
columns[0][name]=Name          the name we gave each column (the one the server sorts by)
```
And it expects this JSON back:
```json
{ "draw": 3, "recordsTotal": 21, "recordsFiltered": 1, "data": [ { "id": 3, "name": "Books" } ] }
```
`recordsTotal` is every row, `recordsFiltered` the rows that match the search. DataTables builds "Showing 1 to 1 of 1 rows (filtered from 21 rows)" and the pager from them.

📦 **Install it with LibMan:** two packages from jsdelivr, **both into `wwwroot/lib/datatables/`** (not the dialog's default folders: the layout loads from this one). `datatables.net@3.1.4` is the script (`js/dataTables.min.js`, and `License.txt`), and `datatables.net-dt@3.1.4` its default look (`css/dataTables.dataTables.min.css`). The CMS layout gets three lines: the CSS after Material Dashboard's, the script, and our own `admin-site.js` after `_Notification`.

🆕 **DataTables 3 doesn't need jQuery.** Its script adds one global, `DataTable`: `new DataTable('#myTable', { … })` creates a table, and `DataTable.defaults`, `DataTable.render` hold the shared settings and helpers. (Older code and many answers online write `$('#myTable').DataTable({ … })` and `$.fn.dataTable`: the jQuery way of DataTables 1. It still works when jQuery is on the page, but we use the global.)

**`admin-site.js`: what every table shares** (a handout). Three things:
- ⚙️ **The defaults for every DataTable in the CMS,** so each page only declares its URL and its columns:
  ```js
  Object.assign(DataTable.defaults, {
      serverSide: true,
      layout: {
          topStart: 'info',
          topEnd: null,
          bottomStart: 'pageLength',
          bottomEnd: 'paging'
      }
  });
  ```
  `serverSide: true` is the switch: the server pages, searches and sorts. `layout` puts the parts around the table: above it on the left, `info` ("Showing 1 to 10 of 57 rows"); below it, `pageLength` (the rows per page) on the left and `paging` (the pager) on the right. `topEnd` is where DataTables' search box would go: `null`, because that box sends a request on every key you type, and we use our own box above the table instead.
  
  Two more defaults: our texts (`Object.assign(DataTable.defaults.language, { … })`, "Show 10 rows" instead of "10 entries per page"), and `DataTable.defaults.column.orderSequence = ['asc', 'desc']`, so a click on a column title switches between A–Z and Z–A. Without it, every third click removes the sort.
- 🔍 **`bindTableSearch(table)`:** Enter in `#inputSearch`, or a click on `#btnSearch`, reloads the table.
- 🔗 **`actionLink(url, icon, cssClass)`:** the HTML of one icon link in the actions column.

**The request, as view models.** Model binding understands DataTables' nested names and fills nested objects. Upper and lower case don't matter, and the fields we don't declare (`columns[0][searchable]`, `search[regex]`, `order[0][name]`, …) are simply skipped:
| DataTables sends | Model binding fills, in `DataTableRequestViewModel` |
|---|---|
| `draw=3` | `Draw` |
| `start=10`, `length=10` | `Start`, `Length` |
| `search[value]=boo` | `Search.Value` (a `DataTableSearchViewModel`) |
| `order[0][column]=0`, `order[0][dir]=asc` | `Order[0].Column`, `Order[0].Dir` (a list of `DataTableOrderViewModel`) |
| `columns[0][name]=Name` | `Columns[0].Name` (a list of `DataTableColumnViewModel`) |

DataTables says "sorted by column **number** 0". The server needs the column's **name**, so the view model works it out:
```csharp
public string? SortColumn
{
    get
    {
        if (Order.Count == 0)
        {
            return null;
        }

        var columnIndex = Order[0].Column;
        return columnIndex >= 0 && columnIndex < Columns.Count ? Columns[columnIndex].Name : null;
    }
}
```
The index comes from the browser, so it's checked against the list before it's used. `IsAscending` is true unless `order[0][dir]` is `desc`.

⏭️ **Not finished yet: the rest of DataTables opens class 9.** The request is ready, but nothing answers it yet. Class 9 adds the answer (`DataTableResponseViewModel`, `PagedResultViewModel`), the repository side (`PagedQueryModel`, `PagedResultModel`, `ToPagedQuery()`, the whitelisted `SortBy`), and the first table: product categories.

📁 **In the `Lamazon` folder,** the four request classes are in `Lamazon.ViewModels/DataTable/`, in the namespace `Lamazon.ViewModels.DataTable`.

🧪 **Check it:** open any CMS page with the dev tools' **Console**: no errors, so `admin-site.js` found `DataTable`. **Product Categories** in the sidebar opens an empty page for now.

---

## 🏠 Homework
None this time: the products table repeats the categories table, so it comes after it, in class 9.

## ✅ By the end of the class
- 🧑‍💼 As the admin, `/Administration` opens the CMS with its sidebar and navbar. As a customer, the AccessDenied page. Logged out, the login page
- 🏷️ The current page's item in the sidebar is colored, and its `<li>` has `class="nav-item active"` in the page source
- 📊 The dashboard shows the same numbers as SSMS, and the administrators
- 📋 DataTables 3 and `admin-site.js` load on every CMS page with no errors in the console, and the request view models are ready for the first table

---

🔜 **Next class:** the rest of DataTables and the first table, product categories. Then the CMS starts changing data: create, edit and delete categories, with validation, Post/Redirect/Get and a soft delete.
