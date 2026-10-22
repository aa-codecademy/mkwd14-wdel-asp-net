# 🏠 Class 09 homework: the products in the CMS

> ⬅️ [Class 09](README.md) · 🏠 [Course overview](../README.md)

In the CMS sidebar, **Products** still opens the "not found" page: the area has no `ProductsController` yet. Today we built the categories, and the products work the same way:
- **Part 1:** the table, paged, searched and sorted by the server
- **Part 2:** create, edit and delete a product

Each step below has a twin in today's code: open the categories file next to the one you're writing.

## 🎯 The goal
| Column | Shows | Sorts by |
|---|---|---|
| Name | the product's name | the name |
| Category | the name of its category | the category's name |
| Price | the price, as `$1,711.99`, aligned right | the price |
| (actions) | the ✏️ and 🗑️ icons | can't be sorted |

The search box looks in the product's name. The **+** button opens a form for a new product, ✏️ the same form with the product in it, and 🗑️ an "are you sure?" page. Deleted products are never shown or counted, in the CMS or in the shop.

---

## 📋 Part 1: the table

### 1. 🗄️ The repository: `Lamazon.DataAccess`
`IProductsRepository` and `ProductsRepository` get one more method:
```csharp
Task<PagedResultModel<Product>> GetFilteredAsync(PagedQueryModel query, CancellationToken cancellationToken = default);
```
- start from `ActiveProducts`. It already skips deleted products, and it has `Include(product => product.ProductCategory)`, which the category column needs. Add `AsNoTracking()`: the table only reads
- count them all first: that's `TotalRecords`
- the search: the products whose `Name` contains the search text
- a whitelist, `_sortableColumns`, with three keys: `Name`, `ProductCategory.Name` and `Price`. The keys are the `name`s of the DataTables columns (step 6). The default sort is by `Id`
- end with `SortBy(...)` and `ToPagedResultAsync(...)` from `QueryableExtensions`, like `ProductCategoriesRepository`

### 2. 🔁 The mapping: `ProductMappingConfig`
One line for the paged result, `PagedResultModel<Product>` → `PagedResultViewModel<ProductViewModel>`, like the one in `ProductCategoryMappingConfig`.

`ProductCategoryName` needs nothing new. Mapster fills it from `ProductCategory.Name` by itself: it splits the name into `ProductCategory` + `Name` ("flattening").

### 3. 🏷️ The view model: `ProductViewModel`
The column titles (`@Html.DisplayNameFor`) and the form's labels (`<label asp-for>`) show the property's name, unless it has a `[Display]` attribute. Give these five better names:

| Property | `[Display(Name = "…")]` |
|---|---|
| `ProductCategoryName` | `Category` |
| `ProductCategoryId` | `Category` |
| `ImageUrl` | `Image URL` |
| `IsFeatured` | `Featured` |
| `DiscountPercentage` | `Discount (%)` |

For example:
```csharp
[Display(Name = "Category")]
public string? ProductCategoryName { get; set; }
```
`[Display]` needs `using System.ComponentModel.DataAnnotations;` at the top of the file.

### 4. ⚙️ The service: `Lamazon.Services`
`IProductsService` and `ProductsService` get:
```csharp
Task<PagedResultViewModel<ProductViewModel>> GetFilteredAsync(DataTableRequestViewModel request, CancellationToken cancellationToken = default);
```
It passes `request.ToPagedQuery()` to the repository and maps what comes back, like `ProductCategoriesService.GetFilteredAsync`.

### 5. 🎮 The controller: `Areas/Administration/Controllers/ProductsController.cs`
- it inherits `AdminBaseController`: the area and the admin role come from there
- the constructor takes `IProductsService` and sets `PageName = "Products"` (the navbar and the browser tab)
- `Index()` returns the view
- `[HttpPost] GetProducts(DataTableRequestViewModel request, CancellationToken cancellationToken)` gets the page from the service and returns `Json(pagedResult.ToDataTableResponse(request.Draw))`

The shop has a `ProductsController` too. The namespaces (`Lamazon.Web.Controllers` and `Lamazon.Web.Areas.Administration.Controllers`) and the area keep the two apart, like the two `HomeController`s.

### 6. 🖼️ The view: `Areas/Administration/Views/Products/Index.cshtml`
The HTML is ready, copy it:
```cshtml
@model ProductViewModel
@* The model is only used for the column titles (DisplayNameFor); the rows come from GetProducts as JSON *@

<div class="card mb-4">
    <div class="card-body">
        <div class="d-flex justify-content-between">
            <a class="btn btn-success btn-add-new" asp-action="Create" title="New product"><i class="material-icons">add</i></a>

            <div class="form-inline">
                <input id="inputSearch" type="search" class="form-control txt-search" placeholder="Search products" />
                <button id="btnSearch" class="btn btn-search btn-info" type="button"><i class="material-icons">search</i></button>
            </div>
        </div>
    </div>
</div>

<div class="card">
    <div class="card-body">
        <table id="productsTable" class="table table-striped table-hover">
            <thead>
                <tr>
                    <th>@Html.DisplayNameFor(model => model.Name)</th>
                    <th>@Html.DisplayNameFor(model => model.ProductCategoryName)</th>
                    <th class="text-right">@Html.DisplayNameFor(model => model.Price)</th>
                    <th></th>
                </tr>
            </thead>
        </table>
    </div>
</div>
```
Until you do Part 2, the **+** button and the ✏️ and 🗑️ icons answer with the "not found" page, like the categories' did at the start of today's class.

Under the HTML, write the `@section Scripts { <script> … </script> }` yourself. `ProductCategories/Index.cshtml` shows how:
- `var productsTable = new DataTable('#productsTable', { … });`, and `bindTableSearch(productsTable);` after it
- `ajax`: a POST to `@Url.Action("GetProducts")`, with the search box's text in `search[value]`
- four `columns`:

| Column | `data` | `name` | Also |
|---|---|---|---|
| Name | `'name'` | `'Name'` | `render: DataTable.render.text()` |
| Category | `'productCategoryName'` | `'ProductCategory.Name'` | `render: DataTable.render.text()` |
| Price | `'price'` | `'Price'` | `className: 'text-right'` and `render: DataTable.render.number(',', '.', 2, '$')` |
| the actions | `'id'` | none | `className: 'column-actions'`, `orderable: false`, and a `render` function that returns the ✏️ and 🗑️ `actionLink`s |

`data` is the property in the JSON (camelCase), and `name` is a key of the whitelist from step 1. The text columns get `render.text()` because a product name is text someone typed: remember the XSS demo.

---

## ✏️ Part 2: create, edit and delete

### 7. ✅ The validator: `Lamazon.Services/Validators/ProductViewModelValidator.cs`
Like `ProductCategoryViewModelValidator`. `AddValidatorsFromAssembly` registers it by itself.

| Property | Rule | Message |
|---|---|---|
| `Name` | not empty, at most 255 characters | Please enter the name of the product. |
| `Description` | not empty | Please enter a description. |
| `ImageUrl` | not empty, at most 2000 characters | Please enter the address of the product's image. |
| `ImageUrl` | a full web address | Please enter a full web address, starting with http:// or https://. |
| `ProductCategoryId` | more than 0 | Please select a category. |
| `Price` | more than 0 | The price must be more than 0. |
| `Price` | less than 100,000,000 | The price must be less than 100,000,000. |
| `DiscountPercentage` | from 0 to 100 | The discount must be between 0 and 100 (%). |

- 📏 The limits come from `ProductConfiguration`: `Name` is 255 characters, `ImageUrl` 2000, and `Price` is `decimal(10, 2)`, so at most 8 digits before the decimal point.
- 🌐 **A full web address:** `.Must(BeAWebAddress)`, where `BeAWebAddress(string imageUrl)` is a private static method of the validator that returns `true` when `Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri)` works and `uri.Scheme` is `Uri.UriSchemeHttp` or `Uri.UriSchemeHttps`.

### 8. 🔁 The mapping: from the form to the entity
A second mapping in `ProductMappingConfig`, view model → entity, like the categories':
```csharp
config.NewConfig<ProductViewModel, Product>()
    .Ignore(dest => dest.Id)
    .Ignore(dest => dest.ProductCategory)
    .Ignore(dest => dest.ProductStatusId);
```
The id never comes from the form. The form chooses the category by its id (`ProductCategoryId`), never as an object, and the status is the service's business.

### 9. ⚙️ The service: `CreateAsync`, `UpdateAsync` and `DeleteAsync`
Add the three to `IProductsService` and `ProductsService`. They look like the categories' methods:
- 🔍 a private `FindProductAsync(int id, …)`: the "load it, or 404" (`NotFoundException`). `GetByIdAsync` can use it too
- ➕ **Create** maps a new `Product`, marks it `ProductStatusEnum.Active`, and adds it
- ✏️ **Update** finds the product, maps the form onto it with `_mapper.Map(productViewModel, product)`, and saves it. `ProductsRepository.GetByIdAsync` has no `AsNoTracking`, so EF Core tracks the product and updates only the columns that changed
- 🗑️ **Delete** is a **soft delete**: the status becomes `ProductStatusEnum.Deleted` (255), and `UpdateAsync` saves it. Old orders point to the product, so the row must stay. `ActiveProducts` hides it from the CMS table and from the shop. There's no business rule: a product can always be deleted

### 10. 🎮 The controller: six more actions
The constructor also takes `IProductCategoriesService` and `IValidator<ProductViewModel>`.

**The categories for the drop-down.** The form chooses the category from a drop-down, so every action that shows the form must load the categories first. A private method in the controller does it:
```csharp
private async Task SetProductCategoriesAsync(CancellationToken cancellationToken)
{
    var productCategories = await _productCategoriesService.GetAllAsync(cancellationToken);
    ViewBag.ProductCategoryList = new SelectList(productCategories, "Id", "Name");
}
```
`SelectList` needs `using Microsoft.AspNetCore.Mvc.Rendering;`. You don't need to tell it which category is selected: `asp-for="ProductCategoryId"` in the form does that.

The actions, like the ones in `ProductCategoriesController`:

| Action | What it does |
|---|---|
| `Create()` GET | `SetProductCategoriesAsync`, then the form with a `new ProductViewModel()` |
| `Create(ProductViewModel …)` POST | validate. Invalid: `SetProductCategoriesAsync` and the form again. Valid: create, a green "The product "…" was created.", and back to `Index` |
| `Edit(int id)` GET | the product from the service, `SetProductCategoriesAsync`, then the form |
| `Edit(ProductViewModel …)` POST | like Create's POST: update, "The product "…" was saved." |
| `Delete(int id)` GET | the product, on the "are you sure?" page |
| `DeleteConfirmed(int id)` POST, with `[ActionName("Delete")]` | delete, "The product was deleted.", and back to `Index` |

- 🪤 **Don't forget `SetProductCategoriesAsync` before `return View(productViewModel)` in an invalid POST.** `ViewBag` lives for one request only. Without it, the form comes back with an empty drop-down.
- 🛡️ The three POSTs get `[HttpPost]` and `[ValidateAntiForgeryToken]`. The notifications are `AddSuccessNotification`, from `AdminBaseController`.

### 11. 🖼️ The views
- **`_ProductForm.cshtml`**, the form's fields, shared by Create and Edit. It's ready, copy it:
  ```cshtml
  @model ProductViewModel
  @* The form fields, shared by Create and Edit *@

  <div asp-validation-summary="ModelOnly" class="text-danger"></div>

  <div class="row">
      <div class="col-md-6">
          <div class="form-group bmd-form-group">
              <label asp-for="Name" class="bmd-label-floating"></label>
              <input asp-for="Name" class="form-control" />
              <span asp-validation-for="Name" class="text-danger"></span>
          </div>
      </div>

      <div class="col-md-6">
          <div class="form-group bmd-form-group is-filled">
              <label asp-for="ProductCategoryId" class="bmd-label-floating"></label>
              <select asp-for="ProductCategoryId" asp-items="(SelectList)ViewBag.ProductCategoryList" class="form-control ddl-search">
                  <option value="">Select category</option>
              </select>
              <span asp-validation-for="ProductCategoryId" class="text-danger"></span>
          </div>
      </div>
  </div>

  <div class="row">
      <div class="col-md-12">
          <div class="form-group bmd-form-group">
              <label asp-for="Description" class="bmd-label-floating"></label>
              <textarea asp-for="Description" class="form-control" rows="4"></textarea>
              <span asp-validation-for="Description" class="text-danger"></span>
          </div>
      </div>
  </div>

  <div class="row">
      <div class="col-md-6">
          <div class="form-group bmd-form-group">
              <label asp-for="ImageUrl" class="bmd-label-floating"></label>
              <input asp-for="ImageUrl" class="form-control" type="url" />
              <span asp-validation-for="ImageUrl" class="text-danger"></span>
          </div>
      </div>

      <div class="col-md-6">
          <div class="form-group bmd-form-group">
              <label asp-for="Price" class="bmd-label-floating"></label>
              <input asp-for="Price" class="form-control" />
              <span asp-validation-for="Price" class="text-danger"></span>
          </div>
      </div>
  </div>

  <div class="row">
      <div class="col-md-6">
          <div class="form-group bmd-form-group">
              <label asp-for="DiscountPercentage" class="bmd-label-floating"></label>
              <input asp-for="DiscountPercentage" class="form-control" />
              <span asp-validation-for="DiscountPercentage" class="text-danger"></span>
          </div>
      </div>

      <div class="col-md-6">
          <div class="form-group bmd-form-group is-filled">
              <label asp-for="IsFeatured" class="bmd-label-floating"></label>
              <input asp-for="IsFeatured" class="form-check-input product-featured-checkbox" />
          </div>
      </div>
  </div>
  ```
  `asp-items` adds the categories after the `<option>` that's already there, so "Select category" stays first, with an empty value.
- **`Create.cshtml` and `Edit.cshtml`:** copy the categories' and change the model to `ProductViewModel`, the titles to "New product" and "Edit product", and the partial to `_ProductForm`. Edit keeps the hidden `Id` field.
- **`Delete.cshtml`:** copy the categories' too. Ask "Are you sure you want to delete this product?", and show the product's name, its category (`ProductCategoryName`) and its price (`$@Model.Price.ToString("N2")`), so the admin sees what they're deleting.

### 12. 🔢 Numbers in the form: two settings
- 🌍 **Prices read and written as `1,234.56`.** ASP.NET Core reads and writes numbers in the language of the computer it runs on. On a Windows set to Macedonian, the decimal point is a comma and the dot separates thousands, so a price typed as `49.99` isn't read as 49.99. The amounts on the dashboard and the order's page show as `1.711,99` there too. One line in `Program.cs`, right before `app.UseRouting();`, makes every request use English (US) numbers:
  ```csharp
  app.UseRequestLocalization("en-US");
  ```
- 📭 **A number field sent empty.** If the drop-down is left on "Select category", or the price is emptied, MVC can't put an empty value into an `int` or a `decimal`. It writes its own error, "The value '' is invalid.", before our validator runs. In `AddWeb()` (`Lamazon.Web/Extensions/DependencyInjection.cs`), in `AddControllersWithViews(options => { … })`, give it a better message:
  ```csharp
  options.ModelBindingMessageProvider.SetValueMustNotBeNullAccessor(_ => "This field is required.");
  ```

---

## ⭐ Bonus: a category filter
A drop-down next to the search box. Choosing a category reloads the table with only that category's products.
- `Lamazon.ViewModels/DataTable/ProductsDataTableRequestViewModel.cs`: it inherits `DataTableRequestViewModel` and adds `public int? CategoryId { get; set; }`. `GetProducts` and the service take it instead of `DataTableRequestViewModel`
- the repository gets an `int? categoryId` parameter, and filters by `ProductCategoryId` only when it has a value. Count `TotalRecords` before this filter, so DataTables can say "(filtered from 6 rows)"
- `Index` calls `SetProductCategoriesAsync` from Part 2 before it returns the view
- the view: the drop-down goes into `form-inline`, before the search box:
  ```cshtml
  <select id="categoryFilter" asp-items="(SelectList)ViewBag.ProductCategoryList" class="form-control ddl-search mr-2">
      <option value="">All categories</option>
  </select>
  ```
- the script: in `ajax.data`, `data.categoryId = $('#categoryFilter').val();`. And when the drop-down changes, reload the table:
  ```js
  $('#categoryFilter').on('change', function () {
      productsTable.ajax.reload();
  });
  ```

---

## ✅ Check it
**Part 1, the table:**
| Do | You should see |
|---|---|
| **Products** in the CMS sidebar | the six products with their category and price, and **Products** active in the sidebar |
| click **Price** twice | the most expensive first: Lenovo ThinkPad T14s Gen 6-2024, $1,711.99 |
| click **Category** | Books, Books, Books, Computers, Computers, Software |
| search `code` | Clean Code |
| dev tools → **Network** → `GetProducts` → **Response** | each row has `productCategoryName` |

**Part 2, create, edit and delete:**
| Do | You should see |
|---|---|
| the **+** button, then **Save** without filling anything | an error under each field you skipped, and the drop-down still lists the categories |
| `lamazon` as the image URL | "Please enter a full web address, starting with http:// or https://." |
| `150` as the discount | "The discount must be between 0 and 100 (%)." |
| a full product: a name, a category, a description, the image address of a product from the shop, and the price `49.99` | the table, with a green "The product "…" was created.", and the price $49.99. The product is in the shop too |
| its ✏️ icon | the form with its values, and its category selected |
| the price `1,234.56` → **Save** | "The product "…" was saved.", and $1,234.56 in the table |
| its 🗑️ icon, then **Delete** | "The product was deleted.", and it's gone from the table and from the shop |
| in SSMS: `SELECT Id, Name, ProductStatusId FROM Products;` | the deleted product is still there, with status `255` |
| `/Administration/Products/Edit/9999` | the "not found" page |

**⭐ The bonus:**
| Do | You should see |
|---|---|
| the filter → Computers | the two laptops, "(filtered from 6 rows)" |

🆘 **Stuck?** After the next class, compare your files with the same files in the `Class10` folder.
