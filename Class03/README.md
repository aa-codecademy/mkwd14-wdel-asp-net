# 🛍️ Class 03: The shop: services and views

> 🛍️ The shop · class 3 of 10 · 🏠 [Course overview](../README.md) · ⬅️ [Class 02](../Class02/README.md) · 🗂️ [The domain](../Class01/domain.md) · 🗺️ [Database diagram](../Class01/Lamazon%20DB%20Diagram.png)

From the **database** to the **first shop page**: the home page with the featured products, from SQL Server, through every layer, in the shop's own layout.

## 🗺️ What we do today

1. 🔁 Recap, and three small fixes in the class 2 code
2. 📚 We finish the repository pattern: `BaseRepository<T>` and `ProductsRepository`
3. 🧾 `Lamazon.ViewModels`: `ProductViewModel`, and why views never get entities
4. 🔄 Mapster: entity → view model, declared once, checked at startup
5. ⚙️ `ProductsService`, `NotFoundException` and `AddServices()`
6. 🖥️ `HomeController` and the home page with the featured products
7. ⏳ Async all the way: the action's `CancellationToken` goes down to EF Core
8. 🎨 The layout: `_Layout`, sections and partials. Handout: `site.css`, the logo, `_Navbar` and `_Footer`

⏭️ **Moved to class 4:** the products page and a product's details, the `_ProductItem` and `_ProductPrice` partials and the tag helpers in them, the nested `_LayoutNoFooter`, and the 404 page.

---

## 💡 Used concepts

### 🔧 Three fixes in the class 2 code
| Where | Change | Why |
|---|---|---|
| `Product.DiscountedPrice` | `DiscountPercentage / 100` → `DiscountPercentage / 100m` | `int / int` is integer division: `10 / 100` is `0`, so the discount was never applied. `100m` is a `decimal`, so `10 / 100m` is `0.1` |
| `Order` | delete `public int InvoiceId`, and make the navigation `public Invoice? Invoice { get; set; }` | the foreign key is `Invoices.OrderId` (class 2), so `Orders.InvoiceId` was an extra required column that pointed nowhere. An order has no invoice until an admin accepts it, so the navigation is nullable |
| the repository files | `IProductRepository` → `IProductsRepository`, `ProductRepository` → `ProductsRepository` | one naming style for every repository: `ProductsRepository`, `ProductCategoriesRepository`, `OrdersRepository`, … |

✏️ **Renaming a class:** rename the file in Solution Explorer and Visual Studio offers to rename the class too. Or put the cursor on the name and press **Ctrl + R, Ctrl + R**.

`DiscountedPrice` only has a getter, so EF Core doesn't map it to a column: no migration. Removing `InvoiceId` changes the `Orders` table, so that one needs a migration:
```powershell
# Package Manager Console, Default project: Lamazon.DataAccess
Add-Migration Remove_InvoiceId_From_Orders
Update-Database
```
Or in a terminal, in the solution folder:
```powershell
dotnet ef migrations add Remove_InvoiceId_From_Orders --project Lamazon.DataAccess --startup-project Lamazon.Web
dotnet ef database update --project Lamazon.DataAccess --startup-project Lamazon.Web
```
🔍 **In SSMS:** refresh **dbo.Orders → Columns**. `InvoiceId` is gone.

---

### 📚 The repository pattern
Class 2 ended with this diagram. Today we write the code:
```
IRepository<T>            AddAsync, UpdateAsync: what every repository can do
      ▲
BaseRepository<T>         the Context, its Table, saving: the code they share
      ▲
ProductsRepository        its own queries: GetAllAsync, GetFeaturedAsync, GetByIdAsync
```
```csharp
public abstract class BaseRepository<T> : IRepository<T> where T : class
{
    protected LamazonDbContext Context { get; }

    protected DbSet<T> Table => Context.Set<T>();

    protected BaseRepository(LamazonDbContext dbContext)
    {
        Context = dbContext;
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        Table.Add(entity);
        await Context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        // An entity loaded through a repository is tracked: EF Core already knows exactly what changed.
        // Update() is only needed for a detached entity; on a tracked one it would mark EVERYTHING
        // it is connected to (the order's user, its line items, ...) as modified.
        if (Context.Entry(entity).State == EntityState.Detached)
        {
            Table.Update(entity);
        }

        await Context.SaveChangesAsync(cancellationToken);
    }
}
```
- 🏷️ **`Context` and `Table`** are `protected` properties: the child repositories use them, the rest of the app doesn't see them. Properties are PascalCase. `_camelCase` is only for `private` fields.
- 🧩 **`Context.Set<T>()`** is the `DbSet` of any entity type, so one base class works for products, categories, orders, …
- 🆔 **After `AddAsync`,** `entity.Id` holds the id that SQL Server generated.

```csharp
public interface IProductsRepository : IRepository<Product>
{
    Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<Product>> GetFeaturedAsync(CancellationToken cancellationToken = default);

    Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
```
```csharp
public class ProductsRepository : BaseRepository<Product>, IProductsRepository
{
    public ProductsRepository(LamazonDbContext dbContext) : base(dbContext)
    {
    }

    // Soft delete: a deleted product stays in the table (old orders point to it), but the app never shows it
    private IQueryable<Product> ActiveProducts => Table
        .Where(product => product.ProductStatusId != (int)ProductStatusEnum.Deleted)
        .Include(product => product.ProductCategory);

    public async Task<List<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await ActiveProducts
            .AsNoTracking()
            .OrderBy(product => product.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Product>> GetFeaturedAsync(CancellationToken cancellationToken = default)
    {
        return await ActiveProducts
            .AsNoTracking()
            .Where(product => product.IsFeatured)
            .OrderBy(product => product.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<Product?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        // Tracked on purpose: the product is often changed and saved afterwards
        return await ActiveProducts.FirstOrDefaultAsync(product => product.Id == id, cancellationToken);
    }
}
```
- 🗑️ **`ActiveProducts`:** every query starts here, so no page can forget the soft-delete filter.
- 🔗 **`Include`** loads the related entity in the same query (a SQL `JOIN`). Without it, `product.ProductCategory` is `null`.
- 🧮 **`IQueryable`** only builds the SQL. Nothing is sent to the database until `ToListAsync()` or `FirstOrDefaultAsync()`.
- 👀 **`AsNoTracking()`** is for data we only show: EF Core doesn't keep a copy to watch for changes, so it's faster. `GetByIdAsync` is tracked on purpose, for the admin's Edit and Delete (class 8).
- ❓ **`Product?`:** "not found" is a normal answer for a repository. Whether it's an error is the service's decision.

### ♻️ Scoped: one per request
```csharp
// Lamazon.DataAccess/DependencyInjection.cs, in AddDataAccess()
// Repositories: Scoped = one instance per HTTP request, the same lifetime as the DbContext
services.AddScoped<IProductsRepository, ProductsRepository>();
```
| Lifetime | One instance per… |
|---|---|
| `AddSingleton` | the whole app |
| **`AddScoped`** | **HTTP request** |
| `AddTransient` | every class that asks for it |

`AddDbContext` registers the `DbContext` as scoped. The repositories are scoped too, so every repository in one request shares the same `DbContext`.

---

### 🧾 View models: what a page shows
An **entity** is the shape of a table. A **view model** is the shape of a page.
- 🎯 The product card needs the category's **name**, not the whole `ProductCategory` with its list of products.
- 🔐 An entity can carry things a page must never show. `User` has a `PasswordHash` (class 4).
- 🧱 `Lamazon.ViewModels` has **no reference to `Lamazon.Domain`**, so a view model can't contain an entity, not even by mistake.

```csharp
public class ProductViewModel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int ProductCategoryId { get; set; }
    public string? ProductCategoryName { get; set; }   // Product.ProductCategory.Name (see Mapster)
    public decimal Price { get; set; }
    public bool IsFeatured { get; set; }
    public int DiscountPercentage { get; set; }
    public decimal DiscountedPrice { get; set; }       // Product.DiscountedPrice, already calculated
    public bool IsAddedToCart { get; set; }            // for the cart buttons (class 5)
}
```
- 💰 **`DiscountedPrice`** is a plain property here. The calculation stays in the entity, and Mapster copies the result.
- 🛒 **`IsAddedToCart`** has no source in `Product`: Mapster leaves it `false`. In class 5 the controller sets it for the products in the cart.

---

### 🔄 Mapster: entity → view model
Copying `Name`, `Price`, `ImageUrl`, … by hand in every service is boring, and easy to forget when a property is added. Mapster copies the properties with the same name, and we declare each mapping once.

| Package | Project | Why |
|---|---|---|
| `Mapster` | `Lamazon.Services` | the mapping itself: `TypeAdapterConfig`, `IRegister` |
| `Mapster.DependencyInjection` | `Lamazon.Services` | `IMapper` through dependency injection (`ServiceMapper`) |

Both are version `10.0.13` in `Directory.Packages.props`. Install them with **Manage NuGet Packages** and Visual Studio writes the version there for you (Central Package Management, class 1).

`Lamazon.Services` also needs project references: `Lamazon.DataAccess` (the repositories, and through it the entities) and `Lamazon.ViewModels`.
```xml
<ItemGroup>
  <PackageReference Include="Mapster" />
  <PackageReference Include="Mapster.DependencyInjection" />
</ItemGroup>

<ItemGroup>
  <ProjectReference Include="..\Lamazon.DataAccess\Lamazon.DataAccess.csproj" />
  <ProjectReference Include="..\Lamazon.ViewModels\Lamazon.ViewModels.csproj" />
</ItemGroup>
```

**One config class per entity**, in `Lamazon.Services/Mappings`:
```csharp
public class ProductMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Entity → view model. ProductCategoryName is filled from ProductCategory.Name automatically
        // ("flattening": Mapster splits the name into ProductCategory + Name).
        config.NewConfig<Product, ProductViewModel>();
    }
}
```
| `ProductViewModel` | comes from `Product` |
|---|---|
| `Id`, `Name`, `Description`, `ImageUrl`, `Price`, … | the property with the same name |
| `DiscountedPrice` | the computed `DiscountedPrice` getter |
| `ProductCategoryName` | `ProductCategory.Name` (flattening) |

⚠️ **Flattening only reads what was loaded.** Without `.Include(product => product.ProductCategory)` in the repository, `ProductCategoryName` is `null` (no exception, just an empty category on the page).

🤔 **Why not `.Map(dest => dest.ProductCategoryName, src => src.ProductCategory?.Name ?? "N/A")`?**
- It doesn't compile: `.Map()` takes an *expression tree*, and C# doesn't allow `?.` inside one (`CS8072`).
- Without the `?.`, it compiles but throws a `NullReferenceException` when the category isn't loaded.
- Flattening already does the job, safely. A placeholder like "N/A" is display text: it belongs in the view, `@(Model.ProductCategoryName ?? "N/A")`.

**Registering Mapster**, in `Lamazon.Services/DependencyInjection.cs`:
```csharp
private static void AddMappers(this IServiceCollection services)
{
    var config = TypeAdapterConfig.GlobalSettings;

    // Every mapping must be declared in a config class (see the Mappings folder):
    // mapping two types nobody configured is an error, not a silent guess
    config.RequireExplicitMapping = true;

    // Finds every class in this project that implements IRegister (ProductMappingConfig, ...)
    config.Scan(typeof(DependencyInjection).Assembly);

    // Builds every declared mapping now, at startup, instead of on the first request
    config.Compile();

    services.AddSingleton(config);
    services.AddScoped<IMapper, ServiceMapper>();
}
```
| Setting | What it does |
|---|---|
| `RequireExplicitMapping = true` | a mapping nobody declared fails with a clear error the first time the code uses it: `Implicit mapping is not allowed (check GlobalSettings.RequireExplicitMapping) and no configuration exists`. Without it, Mapster quietly guesses |
| `Scan(assembly)` | a new config class is picked up by itself: nothing else to register |
| `Compile()` | builds every **declared** mapping at startup: a declared mapping that can't be built stops the app before the first visitor sees it |

🧪 **Try it:** comment out `config.NewConfig<Product, ProductViewModel>();` and start the app. It starts, because `Compile()` only checks the mappings that are declared. The home page then fails with `Implicit mapping is not allowed…`. Put the line back.

In a service, `IMapper` comes through the constructor. A list maps as long as its element type has a mapping:
```csharp
return _mapper.Map<List<ProductViewModel>>(products);
```

---

### ⚙️ The service and NotFoundException
The service is the middle layer. It asks the repository for entities, applies the business rules, and gives the controller **view models**.

First, our own exception, in `Lamazon.Domain/Exceptions`:
```csharp
/// Base class for the errors our app throws on purpose.
public abstract class AppException : Exception
{
    protected AppException(string message) : base(message)
    {
    }
}

/// The requested item doesn't exist (or was deleted).
public class NotFoundException : AppException
{
    public NotFoundException(string entityName, object id)
        : base($"{entityName} with id {id} was not found.")
    {
    }
}
```
- 🧱 **Why in Domain?** It's the bottom layer, so every layer can use it. The service throws it **without knowing anything about HTTP**. In class 4, the Web layer decides that it means a 404 page.
- 👪 **Why `AppException`?** In class 8, `BusinessRuleException` joins it ("a category with products can't be deleted").

```csharp
public interface IProductsService
{
    Task<List<ProductViewModel>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<ProductViewModel>> GetFeaturedAsync(CancellationToken cancellationToken = default);

    /// <summary>Throws NotFoundException if the product doesn't exist or was deleted.</summary>
    Task<ProductViewModel> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
```
```csharp
public class ProductsService : IProductsService
{
    private readonly IProductsRepository _productsRepository;
    private readonly IMapper _mapper;

    public ProductsService(IProductsRepository productsRepository, IMapper mapper)
    {
        _productsRepository = productsRepository;
        _mapper = mapper;
    }

    public async Task<List<ProductViewModel>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        List<Product> products = await _productsRepository.GetAllAsync(cancellationToken);
        return _mapper.Map<List<ProductViewModel>>(products);
    }

    public async Task<List<ProductViewModel>> GetFeaturedAsync(CancellationToken cancellationToken = default)
    {
        List<Product> products = await _productsRepository.GetFeaturedAsync(cancellationToken);
        return _mapper.Map<List<ProductViewModel>>(products);
    }

    public async Task<ProductViewModel> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        Product? product = await _productsRepository.GetByIdAsync(id, cancellationToken);
        if (product is null)
        {
            throw new NotFoundException(nameof(Product), id);
        }

        return _mapper.Map<ProductViewModel>(product);
    }
}
```
- ↩️ **`null` → `NotFoundException`:** the repository says "nothing there", and the service decides that it's an error.
- 🖥️ **Until class 4,** nothing catches the exception: in Development you see the developer error page with the message `Product with id 999 was not found.`

### 💉 AddServices()
Like `AddDataAccess()` in class 2, every layer registers its own classes:
```csharp
// Lamazon.Services/DependencyInjection.cs
public static IServiceCollection AddServices(this IServiceCollection services)
{
    // Services: Scoped, like the repositories they use
    services.AddScoped<IProductsService, ProductsService>();

    services.AddMappers();

    return services;
}
```
```csharp
// Lamazon.Web/Program.cs
builder.Services
    .AddDataAccess(builder.Configuration)   // DbContext, repositories
    .AddServices()                          // business logic, Mapster
    .AddControllersWithViews();
```
In class 4, `AddControllersWithViews()` moves into our own `AddWeb()`, together with the exception filter and the cookie login.

---

### 🖥️ HomeController and the home page
```csharp
public class HomeController : Controller
{
    private readonly IProductsService _productsService;

    public HomeController(IProductsService productsService)
    {
        _productsService = productsService;
    }

    // CancellationToken: MVC fills it for us. If the visitor closes the tab, the database query is cancelled too.
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        List<ProductViewModel> featuredProducts = await _productsService.GetFeaturedAsync(cancellationToken);
        return View(featuredProducts);
    }

    // Privacy() and Error(): still the template's
}
```
The controller only knows the **service** and the **view models**. It never sees a repository, an entity or the `DbContext`.

```cshtml
@* Views/Home/Index.cshtml *@
@model List<ProductViewModel>
@{
    ViewData["Title"] = "Featured products";
}

<h2>Featured products</h2>

<div class="row">
    @foreach (var product in Model)
    {
        <partial name="_ProductItem" model="product" />
    }
</div>
```
Add `@using Lamazon.ViewModels.Models` to `Views/_ViewImports.cshtml`, so every view can write `@model ProductViewModel` without the namespace.

📦 **Before class 4:** the home page draws each product with the `_ProductItem` partial. Copy `_ProductItem.cshtml`, `_ProductPrice.cshtml` and `_CartButtons.cshtml` from `Class03/Lamazon/Lamazon.Web/Views/Shared` into your `Views/Shared`. We go through them at the start of class 4.

---

### ⏳ Async all the way + CancellationToken
```
Browser ──► HomeController.Index(cancellationToken)
               └─► ProductsService.GetFeaturedAsync(cancellationToken)
                      └─► ProductsRepository.GetFeaturedAsync(cancellationToken)
                             └─► ToListAsync(cancellationToken) ──► SQL Server
```
- 🎁 **MVC fills a `CancellationToken` action parameter by itself.** It's cancelled when the visitor closes the tab, presses Esc, or clicks another link before the page arrives.
- 📨 **Every layer passes it on,** down to EF Core, which cancels the query. Nobody waits for an answer that no one will read.
- 🧵 **`await` frees the thread** while SQL Server works, so the same thread can serve other requests.
- ❔ **`= default`** in the services and the repositories: a caller that has no token can leave it out.

🧪 **Try it:** add a temporary delay as the first line of `ProductsService.GetFeaturedAsync`, and put a breakpoint on the line after it:
```csharp
await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);   // temporary!
```
Open the home page and close the tab within 5 seconds. The breakpoint is never hit: the request stopped at the `Delay`, and the query never ran. If Visual Studio stops on a `TaskCanceledException`, that's the cancellation itself: press **Continue**. Delete the line afterwards.

---

### 🎨 The layout
📄 **Handout:**
| File | What it is |
|---|---|
| `wwwroot/css/site.css` | the shop's styles: the navbar, the product cards, the details page, the footer (replace the template's file) |
| `wwwroot/images/lamazon.svg` | the logo |
| `Views/Shared/_Navbar.cshtml` | the navbar: the logo, Home, Products, the cart icon, Login and Register |
| `Views/Shared/_Footer.cshtml` | the footer: the Privacy link and the © line |

🗑️ **Delete:**
| File | Why |
|---|---|
| `Views/Shared/_Layout.cshtml.css` | its styles are in the new `site.css`. Remove the `Lamazon.Web.styles.css` link from `_Layout` too |
| `wwwroot/lib/jquery-validation`, `wwwroot/lib/jquery-validation-unobtrusive` and `Views/Shared/_ValidationScriptsPartial.cshtml` | we validate on the server with FluentValidation (class 4), which has no client-side rules |

Then **rebuild**: `MapStaticAssets()` builds its list of the files in `wwwroot` at build time.

**`_Layout.cshtml`**, the frame around every page:
```cshtml
<body class="d-flex flex-column min-vh-100">
    <header>
        <partial name="_Navbar" />
    </header>

    <div class="container flex-grow-1">
        <main role="main" class="pb-3">
            @RenderBody()
        </main>
    </div>

    @* A page (or a nested layout, class 4) can replace the footer with its own "Footer" section *@
    @if (IsSectionDefined("Footer"))
    {
        @RenderSection("Footer")
    }
    else
    {
        <partial name="_Footer" />
    }

    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
```
| | What it does |
|---|---|
| `@RenderBody()` | where the view's HTML goes |
| `@RenderSection("Footer")` + `IsSectionDefined("Footer")` | a named slot that a view **may** fill. If it doesn't, we show the default footer |
| `@await RenderSectionAsync("Scripts", required: false)` | a page's own scripts, after jQuery and Bootstrap |
| `<partial name="_Navbar" />` | a piece of the layout in its own file: `_Navbar` and `_Footer` |
| `asp-append-version="true"` | adds `?v=<hash of the file>`, so browsers download `site.css` again after it changes |

The `<title>` uses the view's `ViewData["Title"]`: `@(ViewData["Title"] is string title ? $"{title} - Lamazon" : "Lamazon")`.

📌 **The sticky footer:** `d-flex flex-column min-vh-100` stacks the body's children and makes the body at least as tall as the window. `flex-grow-1` lets the content take all the free height, so the footer sits at the bottom even on a short page.

🛒 **Ready for later:** the layout also has `@Html.AntiForgeryToken()` at the top of `<body>`, and two commented-out lines for the notifications. Both are for the cart in class 5.

---

## 🏠 Homework
**The product categories, through every layer we have now.** It's the same pattern as the products, one layer at a time. The CMS uses all of it in classes 7 and 8.

### 1. 📚 `IProductCategoriesRepository` + `ProductCategoriesRepository` (`Lamazon.DataAccess`)
```csharp
public interface IProductCategoriesRepository : IRepository<ProductCategory>
{
    Task<List<ProductCategory>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ProductCategory?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
```
- an `ActiveCategories` property that skips `ProductCategoryStatusEnum.Deleted`, like `ActiveProducts` (no `Include` needed: a category's own columns are enough)
- `GetAllAsync`: every category that isn't deleted, **sorted by name**, without tracking
- `GetByIdAsync`: one category that isn't deleted, or `null`. Tracked, like the product's
- register it in `AddDataAccess()` as scoped

### 2. 🧾 `ProductCategoryViewModel` (`Lamazon.ViewModels`)
`Id` and `Name`.

### 3. 🔄 `ProductCategoryMappingConfig` (`Lamazon.Services/Mappings`)
`ProductCategory` → `ProductCategoryViewModel`. `Scan()` finds the class by itself, so there's nothing to register.

### 4. ⚙️ `IProductCategoriesService` + `ProductCategoriesService` (`Lamazon.Services`)
```csharp
public interface IProductCategoriesService
{
    Task<List<ProductCategoryViewModel>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Throws NotFoundException if the category doesn't exist or was deleted.</summary>
    Task<ProductCategoryViewModel> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
```
- `GetByIdAsync`: when the repository returns `null`, throw `NotFoundException`, like `ProductsService.GetByIdAsync`
- register it in `AddServices()`

### ✅ Check it
No page shows the categories yet, so add two **temporary** actions to `HomeController`. `[FromServices]` takes a parameter straight from dependency injection, so the constructor stays as it is:
```csharp
// Temporary: delete both actions after the check
public async Task<IActionResult> Categories([FromServices] IProductCategoriesService productCategoriesService, CancellationToken cancellationToken)
{
    return Ok(await productCategoriesService.GetAllAsync(cancellationToken));
}

public async Task<IActionResult> Category(int id, [FromServices] IProductCategoriesService productCategoriesService, CancellationToken cancellationToken)
{
    return Ok(await productCategoriesService.GetByIdAsync(id, cancellationToken));
}
```
| Open | You should see |
|---|---|
| `/Home/Categories` | the 5 categories as JSON, sorted by name: Books, Computers, Drinks, Food, Software |
| `/Home/Category/3` | `{"id":3,"name":"Books"}` |
| `/Home/Category/999` | the developer error page: `NotFoundException: ProductCategory with id 999 was not found.` (in class 4 it becomes the 404 page) |

❌ **`Implicit mapping is not allowed…`?** Your `ProductCategoryMappingConfig` is missing, or it doesn't implement `IRegister`.

🆘 **Stuck?** After the next class, compare your files with the same files in the `Class04` folder.

## ✅ By the end of the class
- 🏠 `/` shows the 2 featured products in the shop's layout. Clean Code shows ~~$55.00~~ $49.50
- 🗄️ the `Orders` table has no `InvoiceId` column
- 🚧 **Links that lead nowhere yet:** Products and the product names (class 4), Login and Register (class 4), the cart icon and the Add to cart buttons (class 5). For now, they answer with a 404.

---

🔜 **Next class:** first the rest of class 3: the products page, the nested layout and the 404 page. Then accounts and cookie authentication: register, log in and log out, with hashed passwords, FluentValidation and anti-forgery tokens.
