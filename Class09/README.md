# 🗂️ Class 09: Categories and orders in the CMS

> 🧑‍💼 The CMS · class 9 of 10 · 🏠 [Course overview](../README.md) · ⬅️ [Class 08](../Class08/README.md) · 🗂️ [The domain](../Class01/domain.md) · 🗺️ [Database diagram](../Class01/Lamazon%20DB%20Diagram.png)

Class 8 got DataTables ready: the contract, the shared defaults, and the request as view models. Today the server answers, and the CMS gets its first table: the **product categories**, paged, searched and sorted by the server. Then the CMS starts **changing data**: an admin creates, edits and deletes categories, with validation, **Post/Redirect/Get**, and a **soft delete** that a business rule can refuse. Last, the **orders**: their table, and the page of each order.

📂 This page explains the ideas behind today's code, with small pieces of it. Every file is whole in the `Lamazon` folder next to this README: open it next to the explanation.

## 🗺️ What we do today

1. 🔁 Recap of class 8
2. 📋 DataTables, the rest: the answer, the repository side, sorting and paging
3. 🗂️ The first table: product categories
4. ✏️ Create and edit a category
5. 🗑️ Delete a category: a soft delete and a business rule
6. 📦 The orders table
7. 👁️ An order's page

---

## 💡 Used concepts

### 🔁 From class 8
The CMS is an **area**, `/Administration`, for admins only: `AdminBaseController` carries `[Area]` and `[Authorize(Roles = Roles.Admin)]`, and every admin controller inherits them. `ActiveRouteTagHelper` colors the current page in the sidebar. The dashboard counts with one `GROUP BY` per table. DataTables 3 and `admin-site.js` are on every CMS page, and the four request view models (`DataTableRequestViewModel` and its parts) are in `Lamazon.ViewModels/DataTable`.

🧪 **Check it first:** log in as `admin@lamazon.com`, open the CMS, and compare the dashboard with SSMS. **Product Categories** in the sidebar is still an empty page: today it gets its table.

---

### 📋 DataTables, the rest
A reminder of the contract: for every page, search or click on a column, DataTables POSTs `draw`, `start`, `length`, `search[value]`, `order[0][column]`, `order[0][dir]` and `columns[i][name]`, and expects `{ "draw", "recordsTotal", "recordsFiltered", "data" }` back. Class 8 built the request side. Today the rest:

**How the request travels through the layers:**
```
DataTables (browser)
   │  POST draw, start, length, search[value], order[0][…], columns[i][name]
   ▼
ProductCategoriesController.GetProductCategories(DataTableRequestViewModel request)    Web
   ▼
ProductCategoriesService.GetFilteredAsync: request.ToPagedQuery() → PagedQueryModel     Services
   ▼
ProductCategoriesRepository.GetFilteredAsync: Where → SortBy → Skip/Take               DataAccess
   ▼  PagedResultModel<ProductCategory>
Mapster → PagedResultViewModel<ProductCategoryViewModel>                               Services
   ▼
pagedResult.ToDataTableResponse(request.Draw) → JSON                                   Web
```
The repository can't take a `DataTableRequestViewModel`: `Lamazon.DataAccess` doesn't reference `Lamazon.ViewModels` (class 1). So the request becomes a `PagedQueryModel` in `Lamazon.Domain`, which every layer sees.

**The answer** has the same four properties as the JSON: `DataTableResponseViewModel<T>` with `Draw`, `RecordsTotal`, `RecordsFiltered` and `Data`, which `Json(...)` writes in camelCase. The service returns a `PagedResultViewModel<T>`: one page of rows (`Items`), plus `TotalRecords` (all rows) and `TotalDisplayRecords` (the rows that match). Its `ToDataTableResponse(draw)` turns it into that answer. Both classes go into `Lamazon.ViewModels/DataTable`, next to the request.

**The same, for the repositories.** `Lamazon.Domain/Models` gets two plain classes: `PagedQueryModel` (`Start`, `Length`, `SearchValue`, `SortColumn`, `IsAscending`) and `PagedResultModel<T>` (`Items`, `TotalRecords`, `TotalDisplayRecords`). The extension method `ToPagedQuery()` copies a `DataTableRequestViewModel` into a `PagedQueryModel` and trims the search text. It lives in `Lamazon.Services/Extensions`, because Services is the layer that sees both projects.

**Sorting and paging, once for every table**, in `Lamazon.DataAccess/Extensions/QueryableExtensions.cs`. `SortBy` sorts only by a column from a **whitelist**:
```csharp
var keySelector = column is not null && sortableColumns.TryGetValue(column, out var selector)
    ? selector
    : defaultSort;

return ascending ? query.OrderBy(keySelector) : query.OrderByDescending(keySelector);
```
`ToPagedResultAsync` counts the rows that match, then loads one page of them:
```csharp
var page = filteredQuery.Skip(Math.Max(start, 0));

// DataTables sends length = -1 when the user picks "All"
if (length > 0)
{
    page = page.Take(length);
}
```
| Piece | Why |
|---|---|
| a **whitelist** of sortable columns | the column name comes from the browser, and anyone can change it. Only the columns we listed can sort the table; anything else sorts by the default. Never build a sort from a name the browser sent |
| `Expression<Func<T, object>>` | the whitelist's values. Not a function that runs in C#, but a description of `category => category.Name` that EF Core turns into `ORDER BY [Name]` |
| `Skip` + `Take` | in SQL: `OFFSET 10 ROWS FETCH NEXT 10 ROWS ONLY`. Only one page leaves the database |
| `Math.Max(start, 0)` | a negative `start` from a hand-made request would make `Skip` throw |
| two counts | `TotalRecords` before the search (the repository counts it), `TotalDisplayRecords` after it |

---

### 🗂️ The first table: product categories
With that in place, a table is one small piece in each layer.

**The repository** lists what the table may sort by. The keys are the `name`s of the DataTables columns:
```csharp
private static readonly Dictionary<string, Expression<Func<ProductCategory, object>>> _sortableColumns = new(StringComparer.OrdinalIgnoreCase)
{
    ["Name"] = category => category.Name,
};
```
`GetFilteredAsync(PagedQueryModel query)` starts from `ActiveCategories`, so a deleted category is never shown or counted. It counts them all (`TotalRecords`), filters by the search text, then sorts and pages:
```csharp
if (!string.IsNullOrWhiteSpace(searchValue))
{
    categories = categories.Where(category => category.Name.Contains(searchValue));
}

return await categories
    .SortBy(query.SortColumn, query.IsAscending, _sortableColumns, category => category.Id)
    .ToPagedResultAsync(totalRecords, query.Start, query.Length, cancellationToken);
```
`Contains` becomes `LIKE N'%boo%'` in SQL. SQL Server ignores upper and lower case by default, so `boo` finds `Books`.

**The mapping:** one more line in `ProductCategoryMappingConfig`. Mapster maps `Items` with the category mapping that's already there, and copies the two counts by name:
```csharp
config.NewConfig<PagedResultModel<ProductCategory>, PagedResultViewModel<ProductCategoryViewModel>>();
```

**The service:** `GetFilteredAsync(DataTableRequestViewModel request)` passes `request.ToPagedQuery()` to the repository, and maps what comes back.

**The controller:** the CMS's `ProductCategoriesController` sets `PageName = "Product Categories"` in its constructor (the navbar and the browser tab), and has two actions. `Index` returns the page with an empty table, and DataTables then calls the second one for the rows:
```csharp
[HttpPost]
public async Task<IActionResult> GetProductCategories(DataTableRequestViewModel request, CancellationToken cancellationToken)
{
    var pagedResult = await _productCategoriesService.GetFilteredAsync(request, cancellationToken);
    return Json(pagedResult.ToDataTableResponse(request.Draw));
}
```
- 📬 **`[HttpPost]`:** DataTables sends about ten fields for every column. As a GET, they'd make a very long URL.
- 🛡️ **No `[ValidateAntiForgeryToken]`:** the action only reads, it never changes data, so a forged request (CSRF, class 5) can't do any harm. Every action that changes data keeps the token.
- 🔒 **`[Authorize]`** comes from `AdminBaseController`: the JSON is for admins only too.
- 🐫 **`Json(...)`** writes camelCase: `draw`, `recordsTotal`, `data`, and in each row `id` and `name`.

**The view** has a search box, a button, and a `<table>` with only its column titles. The script tells DataTables where to ask for the rows, and what each column shows:
```js
var productCategoriesTable = new DataTable('#productCategoriesTable', {
    ajax: {
        url: '@Url.Action("GetProductCategories")',
        type: 'POST',
        data: function (data) {
            data.search.value = $('#inputSearch').val();
        }
    },

    columns: [
        {
            data: 'name',
            name: 'Name',
            render: DataTable.render.text()
        },
        {
            data: 'id',
            className: 'column-actions',
            orderable: false,
            render: function (id) {
                return actionLink('@Url.Action("Edit")/' + id, 'edit', 'text-warning')
                     + actionLink('@Url.Action("Delete")/' + id, 'delete', 'text-danger');
            }
        }
    ]
});

bindTableSearch(productCategoriesTable);
```
| Piece | Why |
|---|---|
| `@Url.Action("GetProductCategories")` | `/Administration/ProductCategories/GetProductCategories`: inside the area, `Url.Action` stays in the area |
| `ajax.data` | changes the request before it's sent: our own search box's text goes into `search[value]` |
| `data: 'name'` | which property of each JSON row this column shows |
| `name: 'Name'` | what the server sorts by when this column is clicked: a key of `_sortableColumns` |
| `render: DataTable.render.text()` | writes the value as text, never as HTML (see the XSS demo below) |
| `orderable: false` | the actions column can't be sorted |
| the Edit and Delete icons, and the **+** button | their pages come later today. Until then they answer with the "not found" page |

🧪 **Try it.** The seed data has five categories, so first add fifteen more in SSMS to see the pages:
```sql
INSERT INTO ProductCategories (Name, ProductCategoryStatusId)
VALUES ('Test category 01', 1), ('Test category 02', 1), ('Test category 03', 1), ('Test category 04', 1), ('Test category 05', 1),
       ('Test category 06', 1), ('Test category 07', 1), ('Test category 08', 1), ('Test category 09', 1), ('Test category 10', 1),
       ('Test category 11', 1), ('Test category 12', 1), ('Test category 13', 1), ('Test category 14', 1), ('Test category 15', 1);
```
| Do | You should see |
|---|---|
| **Product Categories** in the sidebar | "Showing 1 to 10 of 20 rows", sorted by name, and the item active in the sidebar |
| the pager → **2** | "Showing 11 to 20 of 20 rows" |
| click the **Name** title | the order turns around: Test category 15 first. Click again: A to Z |
| type `oo` and press **Enter** | Books and Food, "(filtered from 20 rows)" |
| empty the box and click the 🔍 button | all 20 rows again |
| "Show 25 rows" | everything on one page |
| dev tools → **Network** → `GetProductCategories` | the request's **Payload** and the JSON **Response** |

🕵️ **Try to sort by something else.** The column name comes from the browser, so ask for a column that isn't in the list. In the browser's console, on the categories page:
```js
fetch('/Administration/ProductCategories/GetProductCategories', {
    method: 'POST',
    body: new URLSearchParams({ draw: 1, 'columns[0][name]': 'PasswordHash', 'order[0][column]': 0, start: 0, length: 3 })
}).then(response => response.json()).then(console.log);
```
The answer is sorted by `Id`: the default. An unknown name is never turned into a sort.

🔓 **XSS in a table.** A category name is text that someone typed. Add a nasty one in SSMS:
```sql
INSERT INTO ProductCategories (Name, ProductCategoryStatusId) VALUES (N'<img src=x onerror=alert(''XSS'')>', 1);
```
Reload the table: the name is shown as text. Now comment out `render: DataTable.render.text()` (and the `,` before it), rebuild and reload: an **alert** pops up. DataTables puts each value into the page as HTML, so the `<img>` became a real image whose `onerror` ran our code. In the CMS, that code would run in an **admin's** browser. `render.text()` writes the value as text, like Razor's `@` does. Put it back.

---

### ✏️ Create and edit a category
The first CMS pages that **change** data. Each one is a pair of actions with the same name: a **GET** shows the form, a **POST** receives it.

**Notifications in the CMS.** After a save, a green message on the next page. `AdminBaseController` gets the same method the shop's `BaseController` has (class 6):
```csharp
protected void AddSuccessNotification(string message)
{
    TempData[Notifications.MessageKey] = message;
    TempData[Notifications.TypeKey] = "success";
}
```
`TempData` survives exactly one redirect, and the CMS layout renders the same `_Notification` partial as the shop.

**The validator**, like the register form's (class 5): FluentValidation, in `Lamazon.Services/Validators`. `AddValidatorsFromAssembly` registers it by itself:
```csharp
RuleFor(x => x.Name)
    .NotEmpty().WithMessage("Please enter the name of the category.")
    .MaximumLength(255);
```
255 is the length of the `Name` column (`ProductCategoryConfiguration`): the validator stops a longer name before the database would.

**The POST, with Post/Redirect/Get:**
```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(ProductCategoryViewModel productCategoryViewModel, CancellationToken cancellationToken)
{
    var validationResult = await _validator.ValidateAsync(productCategoryViewModel, cancellationToken);
    validationResult.AddToModelState(ModelState);
    if (!ModelState.IsValid)
    {
        return View(productCategoryViewModel);
    }

    await _productCategoriesService.CreateAsync(productCategoryViewModel, cancellationToken);
    AddSuccessNotification($"The category \"{productCategoryViewModel.Name}\" was created.");

    return RedirectToAction(nameof(Index));
}
```
| Piece | Why |
|---|---|
| validate → `AddToModelState` → `ModelState.IsValid` | the explicit pattern of the login and register forms (class 5): the errors show next to the fields (`asp-validation-for`) |
| `return View(productCategoryViewModel)` | invalid: the same form again, with what the admin typed and the errors |
| `RedirectToAction(nameof(Index))` | **Post/Redirect/Get:** after a save, the browser goes to the table with a GET. Refresh (F5) repeats that GET, not the POST, so the category isn't created twice |
| `[ValidateAntiForgeryToken]` | every action that changes data checks the token. The `<form>` tag helper adds it to the form by itself |

`Edit` has the same shape: the GET loads the category into the form, the POST validates, saves, notifies and redirects.

**One form for both pages.** `Create.cshtml` and `Edit.cshtml` are a card each, and both render the same fields from the partial `_ProductCategoryForm.cshtml` (a handout):
```cshtml
<partial name="_ProductCategoryForm" model="Model" />
```
`Edit` also sends the id back, in a hidden field, so the POST knows which category to change:
```cshtml
<input type="hidden" asp-for="Id" />
```

**From the form to the entity.** A new mapping, view model → entity, in `ProductCategoryMappingConfig`:
```csharp
config.NewConfig<ProductCategoryViewModel, ProductCategory>()
    .Ignore(dest => dest.Id);
```
The id never comes from the form: on Create the database gives it, and on Edit we already loaded the category by its id.

**Create and update, in the service.** Create maps a new entity, marks it **Active**, and adds it. Update **loads the tracked category, maps the form onto it, and saves**:
```csharp
public async Task UpdateAsync(ProductCategoryViewModel productCategoryViewModel, CancellationToken cancellationToken = default)
{
    var category = await FindCategoryAsync(productCategoryViewModel.Id, cancellationToken);
    _mapper.Map(productCategoryViewModel, category);

    await _productCategoriesRepository.UpdateAsync(category, cancellationToken);
}
```
- 🔁 **`_mapper.Map(source, destination)`** copies onto the object we already have, instead of making a new one. EF Core is tracking it since `GetByIdAsync`, so `SaveChanges` updates only the columns that changed: here, `UPDATE … SET [Name] = …`.
- 🔍 **`FindCategoryAsync`**, a private method, is the "load it, or 404" that `GetByIdAsync`, `UpdateAsync` and `DeleteAsync` all need: a missing (or deleted) category throws `NotFoundException`, which `AppExceptionFilter` turns into the 404 page.

🧪 **Try it:**
| Do | You should see |
|---|---|
| the **+** button, then **Save** with an empty name | "Please enter the name of the category." next to the field |
| a name with more than 255 characters | the length error, and the text you typed is still there |
| `Garden` → **Save** | back on the table, a green "The category "Garden" was created.", and Garden in the table |
| F5 on the table | nothing is created again |
| the ✏️ icon of Garden → `Garden & Outdoors` → **Save** | "…was saved." and the new name |
| `/Administration/ProductCategories/Edit/9999` | the "not found" page |

---

### 🗑️ Delete a category: a soft delete and a business rule
**A confirmation page.** The 🗑️ icon is a link, so it's a GET, and a GET must never change data: a browser that prefetches links, a crawler, or an `<img src="…/Delete/3">` on another site would delete it. So the GET only asks "are you sure?" (`Delete.cshtml`, a handout), and the form on that page POSTs, with its anti-forgery token.

The two actions want the same name and the same parameter, `Delete(int id)`, which C# doesn't allow. The POST gets another C# name, and `[ActionName]` gives it the same URL:
```csharp
[HttpPost]
[ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
```

**A soft delete.** The row stays in the table: its status becomes **Deleted** (255). The repository's `ActiveCategories` skips it, so it's gone from the table and from `GetByIdAsync` (Edit and Delete answer "not found"), while products and orders that point to it still work.

**A business rule.** A category that still has products can't be deleted: every product must have a category the admin can see and choose. The service checks it before it changes anything:
```csharp
// A product must always have a category the admin can see and select
if (await _productCategoriesRepository.HasActiveProductsAsync(id, cancellationToken))
{
    throw new BusinessRuleException($"The category \"{category.Name}\" still has products. Move or delete them first.");
}

category.ProductCategoryStatusId = (int)ProductCategoryStatusEnum.Deleted;
await _productCategoriesRepository.UpdateAsync(category, cancellationToken);
```
- 🧮 **`HasActiveProductsAsync`** is one `AnyAsync` in the repository: `SELECT CASE WHEN EXISTS (…)`. The database answers yes or no, and no product is loaded.
- 🚦 **`BusinessRuleException`** goes to `AppExceptionFilter`, as the empty cart did in class 7: a red notification, back on the page the admin came from. The controller doesn't need a `try`/`catch`.

🧪 **Try it:**
| Do | You should see |
|---|---|
| the 🗑️ icon of Test category 15 | "Are you sure you want to delete the category **Test category 15**?" |
| **Delete** | "The category was deleted.", and it's gone from the table |
| in SSMS: `SELECT Id, Name, ProductCategoryStatusId FROM ProductCategories WHERE Name = 'Test category 15';` | the row is still there, with status `255` |
| the 🗑️ icon of **Books**, then **Delete** | back on the "Are you sure?" page (the `Referer`), with a red "The category "Books" still has products. Move or delete them first.". Books is still in the table |

🧹 **Clean up** in SSMS (the soft-deleted test category too):
```sql
DELETE FROM ProductCategories WHERE Name LIKE 'Test category%' OR Name LIKE '<img%' OR Name LIKE 'Garden%';
```

---

### 📦 The orders table
Customers place orders since class 6: each one starts **Pending**, with a number like `3/2026`, and since class 7 with a country and its flag, when ip-api.com answers. Now the admin sees them. It's the categories table again, one piece per layer. What's new:

**The status, as an enum.** The database stores the status as a number (`OrderStatusId`), and code reads better with names. `Lamazon.ViewModels` doesn't reference `Lamazon.Domain`, so it gets its own copy of the enum, with the same ids: `Lamazon.ViewModels/Enums/OrderStatusEnum.cs`. The new `OrderViewModel` (a handout) has the status as that enum, plus its name as text for the table:
```csharp
public OrderStatusEnum OrderStatus { get; set; }

public string OrderStatusName => OrderStatus.ToString();
```

**The mapping** tells Mapster the two things it can't guess: the id becomes the enum, and the date is shown in local time. It's stored in UTC (`DateTime.UtcNow` when the order is created):
```csharp
config.NewConfig<Order, OrderViewModel>()
    .Map(dest => dest.OrderStatus, src => (OrderStatusViewModelEnum)src.OrderStatusId)
    .Map(dest => dest.OrderDate, src => src.OrderDate.ToLocalTime());
```
`OrderStatusViewModelEnum` is an alias at the top of the file, `using OrderStatusViewModelEnum = Lamazon.ViewModels.Enums.OrderStatusEnum;`: its name says which of the two enums it is. The customer needs nothing new: `User` → `UserViewModel` is already in `UserMappingConfig`.

**The repository.** The whitelist has two surprises:
```csharp
// "10/2026" comes before "9/2026" as text, so the order number sorts by id (the same order, as numbers)
["OrderNumber"] = order => order.Id,
["User.FullName"] = order => order.User.FullName,
["CountryCode"] = order => order.CountryCode!,
["TotalAmount"] = order => order.TotalAmount,
// OrderStatus is a related entity: SQL can only sort by a column, so we sort by its id
["OrderStatus"] = order => order.OrderStatusId,
```
`GetFilteredAsync` starts from every order (orders are never deleted), with `Include(order => order.User)` for the customer column, and searches in two columns:
```csharp
orders = orders.Where(order => order.OrderNumber.Contains(searchValue) || order.User.FullName.Contains(searchValue));
```
In SQL: a `JOIN` to `Users` for the `Include`, and two `LIKE`s with an `OR` between them.

**The controller:** the area's `OrdersController` sets `PageName = "Orders"` and has `Index` and `GetOrders`, the same as `GetProductCategories`.

**The view** (a handout) is the categories view, with new columns:
```js
// Newest orders first (column 0, descending)
order: [[0, 'desc']],
```
| Piece | Why |
|---|---|
| `order: [[0, 'desc']]` | the sort before any click: column 0, the number, newest first |
| `data: 'user.fullName'` | a property of a property: each JSON row has `user: { fullName: … }` |
| the flag column's `render` function | returns HTML: an `<img>` with the flag's URL, or nothing when the order has no country. HTML is fine here only because the URL isn't text a user typed: `GetCountryFlagUrl` builds it |
| `render: DataTable.render.number(',', '.', 2, '$')` | `1711.99` → `$1,711.99`: the thousands separator, the decimal point, 2 decimals, and a prefix |
| the 👁️ icon | the order's page, `Details`: next section |

🧪 **Try it.** First, some orders: in a private window (the admin stays logged in in the other one), log in as `user@lamazon.com` / `User123!` and place **three orders**, with different products.
| Do | You should see |
|---|---|
| **Orders** in the CMS sidebar | the orders, newest first, with Jane Doe, the country and its flag, the total and **Pending** |
| click **Total amount** | the cheapest first |
| search `jane` | Jane Doe's orders: the search looks at the customer too |
| dev tools → **Network** → `GetOrders` → **Response** | each row has `user: { … }` and `orderStatusName: "Pending"` |

---

### 👁️ An order's page
The 👁️ icon opens `Details` (a handout view): the order's number and date, the customer, the total, the country, and the lines.

**The repository's `GetByIdAsync`** loads the order with its lines and its customer:
```csharp
return await Table
    .Include(order => order.OrderLineItems)
    .Include(order => order.User)
    .FirstOrDefaultAsync(order => order.Id == id, cancellationToken);
```
There's no `AsNoTracking`, so EF Core **tracks** the order. Next class, Accept and Reject change it and save it.

**The service:** `GetByIdAsync` maps the order to an `OrderViewModel`, and its lines with the `OrderLineItem` → `OrderLineItemViewModel` mapping that's already in `OrderMappingConfig`. `FindOrderAsync` is the "load it, or 404" of the categories again: a missing order throws `NotFoundException`, and the 404 page shows.

**The controller:** `Details(int id)` gets the order from the service and returns the view with it.

**The view** shows the **Reject** and **Accept** buttons only while the order is Pending:
```cshtml
@if (Model.OrderStatus == OrderStatusEnum.Pending)
```
That's why the area's `_ViewImports` gets `@using Lamazon.ViewModels.Enums`. The buttons don't do anything yet. Their confirmation, and what Accept and Reject do, come next class.

🧪 **Try it:**
| Do | You should see |
|---|---|
| the 👁️ icon of one of Jane's orders | its number, the date in local time, Jane Doe and her email, the total, the country, the lines, and **Reject** and **Accept** |
| in SSMS: `SELECT OrderNumber, OrderDate, OrderStatusId FROM Orders;` | the same date in UTC: two hours earlier (one hour after the clocks go back on 25 October) |
| `/Administration/Orders/Details/9999` | the "not found" page |
| **Back** | the orders table |

---

## 🏠 Homework: the products in the CMS
The sidebar's **Products** page, like today's categories: the table, then create, edit and delete. 📝 [HOMEWORK.md](HOMEWORK.md) lists what to build in each layer, and gives you the HTML of the table page and of the form to start from.

## ✅ By the end of the class
- 🗂️ The categories table pages, searches and sorts on the server, and a name with HTML in it shows as text
- ✏️ An admin creates and edits categories: an invalid form shows its errors, a saved one shows a green notification, and F5 doesn't save it twice
- 🗑️ Deleting a category hides it (its status becomes 255), and a category with products can't be deleted: a red notification
- 📦 The orders table shows every order, newest first, with its customer, flag, total and status
- 👁️ An order's page shows its customer and its lines, with the date in local time

---

🔜 **Next class, the last one:** the orders' **Accept** and **Reject**. Accepting an order creates its invoice, and the two are saved together in one **transaction**, or not at all. Then the invoices: their table and their page, paid or cancelled, with the same kind of rules as the orders. Then a look back at the whole app.
