# 🔑 Class 04: Accounts and cookie authentication

> 🛍️ The shop · class 4 of 10 · 🏠 [Course overview](../README.md) · ⬅️ [Class 03](../Class03/README.md) · 🗂️ [The domain](../Class01/domain.md) · 🗺️ [Database diagram](../Class01/Lamazon%20DB%20Diagram.png)

## 🗺️ What we do today

1. 🔁 Recap
2. 🛍️ From class 3: `ProductsController`, its views, the `_ProductItem` and `_ProductPrice` partials, and the tag helpers in them
3. 🪆 From class 3: the nested layout `_LayoutNoFooter`
4. 🚫 From class 3: errors. `/Products/Details/999` → `NotFoundException` → the 404 page; `AddWeb()`
5. 🔐 Authentication vs authorization; a cookie vs a token; claims
6. 👤 `UsersRepository`, the user view models and `UserMappingConfig`
7. 🧂 Password hashing with `IPasswordHasher<User>`
8. ⚙️ `UsersService`: register, and check a login

⏭️ **Moved to class 5:** FluentValidation, cookie authentication, and `UsersController` with the Login, Register and Logout pages (`[Authorize]`, anti-forgery tokens, open redirects).

---

## 💡 Used concepts

### 🛍️ The products page
```csharp
public class ProductsController : Controller
{
    private readonly IProductsService _productsService;

    public ProductsController(IProductsService productsService)
    {
        _productsService = productsService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        List<ProductViewModel> products = await _productsService.GetAllAsync(cancellationToken);
        return View(products);
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        // A product that doesn't exist → NotFoundException → the 404 page (see Errors)
        ProductViewModel product = await _productsService.GetByIdAsync(id, cancellationToken);
        return View(product);
    }
}
```
`Views/Products/Index.cshtml` is the home page's view with another title: `ViewData["Title"] = "Products"` and `<h2>Products</h2>`. Same list, same partial.

**One product card**, used by the home page and the products page:
```cshtml
@* Views/Shared/_ProductItem.cshtml *@
@model ProductViewModel

<div class="col-md-4">
    <div class="product-container">
        <img class="product-image" src="@Model.ImageUrl" alt="@Model.Name" />
        <div class="product-content">
            <a asp-controller="Products" asp-action="Details" asp-route-id="@Model.Id">
                <p class="product-name" title="@Model.Name">@Model.Name</p>
            </a>
            @* Flexbox: the category on the left, the price on the right, on one line *@
            <div class="d-flex justify-content-between align-items-baseline gap-2">
                <p class="product-category">@Model.ProductCategoryName</p>
                <partial name="_ProductPrice" model="Model" />
            </div>
            <hr />
            <div class="product-description">
                <p>@Model.Description</p>
            </div>
        </div>
        <div class="product-actions">
            <partial name="_CartButtons" model="Model" />
        </div>
    </div>
</div>
```
**The price**, used by the card and by the details page. With a discount, the old price is crossed out:
```cshtml
@* Views/Shared/_ProductPrice.cshtml *@
@model ProductViewModel

@if (Model.DiscountPercentage > 0)
{
    <p class="product-price"><del>$@Model.Price.ToString("N2")</del> $@Model.DiscountedPrice.ToString("N2")</p>
}
else
{
    <p class="product-price">$@Model.Price.ToString("N2")</p>
}
```
`_CartButtons` shows "Add to cart" or "Remove from cart", depending on `IsAddedToCart`. The buttons start working in class 5.

**One product's page:**
```cshtml
@* Views/Products/Details.cshtml *@
@model ProductViewModel
@{
    ViewData["Title"] = Model.Name;
}

<div class="row">
    <div class="col-md-12">
        <a asp-controller="Products" asp-action="Index">&larr; Back to products</a>
    </div>
</div>
<hr />
<div class="row">
    <div class="col-md-6">
        <img class="product-details-image" src="@Model.ImageUrl" alt="@Model.Name" />
    </div>
    <div class="col-md-6">
        <p class="product-details-name">@Model.Name</p>
        <p class="product-details-category">Category: @Model.ProductCategoryName</p>
        <div class="product-details-price">
            <partial name="_ProductPrice" model="Model" />
        </div>
        <div class="product-details-actions mt-3">
            <partial name="_CartButtons" model="Model" />
        </div>
    </div>
</div>
<div class="row">
    <div class="col-md-12">
        <p class="product-details-description">@Model.Description</p>
    </div>
</div>
```

| You write | The browser gets |
|---|---|
| `<a asp-controller="Products" asp-action="Details" asp-route-id="3">` | `<a href="/Products/Details/3">` |
| `<a asp-controller="Home" asp-action="Index">` | `<a href="/">` (the route's default values are left out) |
| `<partial name="_ProductPrice" model="Model" />` | the HTML of `_ProductPrice.cshtml`, with `Model` as its model |

- 🔗 **Tag helpers build the URL from the routes.** Change a route, and every link follows. A hand-written `href` would break.
- 📁 **Where partials are found:** first in the controller's own folder (`Views/Products/`), then in `Views/Shared/`. A partial that several controllers use goes in `Shared`.
- 🧩 **DRY:** the card exists once, and the price exists once. The home page and the products page differ only in their title and their list.

---

### 🪆 A nested layout: _LayoutNoFooter
Remember the `Footer` section in `_Layout` (class 3)? `_LayoutNoFooter` uses `_Layout` for everything and only replaces the footer with nothing:
```cshtml
@* Views/Shared/_LayoutNoFooter.cshtml *@
@{
    Layout = "_Layout";
}

@RenderBody()

@section Footer {
}

@* Pass the view's scripts on to _Layout *@
@section Scripts {
    @await RenderSectionAsync("Scripts", required: false)
}
```
The Privacy page picks it with `Layout = "_LayoutNoFooter";`.
- 🧱 **No copy of the layout:** a change in `_Layout` reaches every page, with or without the footer.
- 📜 **Why the `Scripts` section in the middle?** A section that a view defines must be rendered by its layout. Without it, a view with its own scripts would fail.

---

<a id="errors"></a>
### 🚫 Errors: the 404 page
```
/Products/Details/999
  → ProductsService throws NotFoundException
  → AppExceptionFilter turns it into a 404 (no body)
  → UseStatusCodePagesWithReExecute runs /Home/Error?statusCode=404
  → Error.cshtml: "Page not found"
```
An unknown address, like `/does-not-exist`, is a 404 from routing, and ends on the same page.

**An MVC exception filter** runs when an action throws. We register it once, for every controller, so the controllers need no `try`/`catch`:
```csharp
// Lamazon.Web/Filters/AppExceptionFilter.cs
public class AppExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        switch (context.Exception)
        {
            case NotFoundException:
                // → 404, and UseStatusCodePagesWithReExecute shows the "not found" page
                context.Result = new NotFoundResult();
                context.ExceptionHandled = true;
                break;
        }
    }
}
```
- 🔀 **A `switch` with one `case`?** In class 8, `BusinessRuleException` gets its own `case`.
- 🐞 **Any other exception is a bug.** The filter doesn't touch it, and it goes on to the error page.

**`AddWeb()`:** like `AddDataAccess()` and `AddServices()`, the Web layer gets its own registration method. In class 5 the cookie login goes in here too:
```csharp
// Lamazon.Web/Extensions/ServiceCollectionExtensions.cs
public static IServiceCollection AddWeb(this IServiceCollection services)
{
    services.AddControllersWithViews(options =>
    {
        // Turns NotFoundException into a 404 page (see Filters)
        options.Filters.Add<AppExceptionFilter>();
    });

    return services;
}
```
```csharp
// Program.cs
builder.Services
    .AddDataAccess(builder.Configuration)   // DbContext, repositories
    .AddServices()                          // business logic, Mapster
    .AddWeb();                              // MVC, the exception filter

// ...

if (!app.Environment.IsDevelopment())
{
    // Production: an exception nobody handled → the friendly error page
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// A 404 (or another error status code) without a body → the friendly error page, in every environment
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");
```
- 🔁 **Re-execute, not redirect:** the error page runs inside the same request. The address stays `/Products/Details/999` and the answer stays a real `404`. A redirect would answer `302` and then `200 OK`, as if the page existed.
- 🧑‍💻 **In Development,** an exception nobody handled shows the detailed developer page instead, with the stack trace. The 404 page shows in Development too, because the filter handles the `NotFoundException`.

**The error page** handles both cases:
```csharp
// Lamazon.Web/Models/ErrorViewModel.cs
public class ErrorViewModel
{
    public int StatusCode { get; set; }

    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public bool IsNotFound => StatusCode == StatusCodes.Status404NotFound;
}
```
```csharp
// HomeController
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public IActionResult Error(int? statusCode)
{
    return View(new ErrorViewModel
    {
        // No statusCode: UseExceptionHandler sent us here because of an exception
        StatusCode = statusCode ?? StatusCodes.Status500InternalServerError,
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
    });
}
```
```cshtml
@* Views/Shared/Error.cshtml *@
@model ErrorViewModel
@{
    ViewData["Title"] = Model.IsNotFound ? "Page not found" : "Error";
}

@if (Model.IsNotFound)
{
    <h1 class="text-danger">Page not found</h1>
    <p>The page or the item you're looking for doesn't exist, or it was deleted.</p>
}
else
{
    <h1 class="text-danger">Something went wrong</h1>
    <p>An error occurred while processing your request. Please try again later.</p>

    @if (Model.ShowRequestId)
    {
        <p>
            <strong>Request ID:</strong> <code>@Model.RequestId</code>
        </p>
    }
}

<a class="btn btn-primary" asp-area="" asp-controller="Home" asp-action="Index">Back to the home page</a>
```

---

### 🔐 Authentication vs authorization
| | The question | In Lamazon |
|---|---|---|
| **Authentication** | **Who are you?** | the login: email + password → a login cookie |
| **Authorization** | **Are you allowed?** | `[Authorize]`: only logged-in users; `[Authorize(Roles = Roles.Admin)]`: only admins (class 7) |

HTTP has no memory: every request arrives on its own. After the login, something has to come with every request and say "it's me again":
| | Who sends it | Where it lives | Used by |
|---|---|---|---|
| 🍪 **A cookie** | the **browser**, by itself, with every request to our site | the browser's cookie jar | apps whose pages the server renders: Lamazon |
| 🎫 **A token** (JWT) | **the app's own code**, in a header: `Authorization: Bearer …` | wherever the app keeps it | an Angular app that calls a Web API |

**Claims** are facts about the user, written into the cookie at login: the id, the name, the email, the role. On every request ASP.NET Core reads them back, so `User` in a controller or a view knows who is logged in, without asking the database.

---

### 👤 Users: the repository and the view models
```csharp
public interface IUsersRepository : IRepository<User>
{
    /// <summary>The user with their role.</summary>
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>The user with their role.</summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
```
```csharp
public class UsersRepository : BaseRepository<User>, IUsersRepository
{
    public UsersRepository(LamazonDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await Table
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.Id == id, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await Table
            .Include(user => user.Role)
            .FirstOrDefaultAsync(user => user.Email == email, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        // AnyAsync → SQL "EXISTS": true or false, without loading the user
        return await Table.AnyAsync(user => user.Email == email, cancellationToken);
    }
}
```
- 🔗 **`Include(user => user.Role)`:** the view model shows the role's name (flattening, like `ProductCategoryName`).
- 👀 **Tracked on purpose:** the login may save a new hash (see `SuccessRehashNeeded` below), and in the Users tab (the class 9 homework) the admin changes a user's role.
- 💉 Register it in `AddDataAccess()`: `services.AddScoped<IUsersRepository, UsersRepository>();`

📄 **Handout:** the three user view models, in `Lamazon.ViewModels/Models`:
```csharp
/// <summary>The login form.</summary>
public class UserCredentialsViewModel
{
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
```
```csharp
/// <summary>The registration form.</summary>
public class RegisterUserViewModel
{
    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Confirm password")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = string.Empty;
}
```
```csharp
/// <summary>A user as the app shows it. There is no password here, not even the hash.</summary>
public class UserViewModel
{
    public int Id { get; set; }

    [Display(Name = "Full name")]
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    [Display(Name = "Role")]
    public string RoleKey { get; set; } = string.Empty;

    // Filled by Mapster from User.Role.Name ("flattening")
    [Display(Name = "Role")]
    public string? RoleName { get; set; }
}
```
| Attribute | What it changes in the view |
|---|---|
| `[Display(Name = "Full name")]` | the text of `<label asp-for="FullName">`: "Full name" instead of "FullName" |
| `[DataType(DataType.Password)]` | `<input asp-for="Password">` becomes `type="password"`: dots instead of letters, and the value is never written back into the page |

🔐 **Remember class 3: views never get entities.** `User` has a `PasswordHash`. `UserViewModel` doesn't, so no page can show it, not even by mistake.

**The mappings**, in `Lamazon.Services/Mappings/UserMappingConfig.cs`:
```csharp
public class UserMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // RoleName is filled from Role.Name automatically ("flattening")
        config.NewConfig<User, UserViewModel>();

        // Registration: the role and the password hash are set by UsersService, never by the form
        config.NewConfig<RegisterUserViewModel, User>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.RoleKey)
            .Ignore(dest => dest.PasswordHash);
    }
}
```
- ↔️ **The first view model → entity mapping.** A form becomes a row in the database.
- 🚫 **`.Ignore(...)`:** these properties never come from the form. Even if someone adds `RoleKey=admin` to the POST by hand, it isn't copied.

---

### 🧂 Password hashing
❌ **Never store the password itself.** Whoever reads the `Users` table (a backup, a leaked copy, a curious colleague) would know every customer's password, and people use the same password on many sites.

✅ **Store a hash:** a one-way function. `password → hash` is easy, and `hash → password` is impossible. At login we hash what the user typed and compare the hashes.

But not just any hash:
| | Why it matters |
|---|---|
| 🧂 **A random salt** for every password | the same password gives a different hash for every user. A list of hashes of common passwords (a "rainbow table") is useless, and two users with the password `123456` can't be spotted |
| 🐢 **Slow on purpose:** many iterations | SHA-256 by itself is fast: a graphics card tries billions of passwords a second. 100,000 iterations make every guess 100,000 times slower, for the attacker too |

We don't write this ourselves. **`PasswordHasher<TUser>`** is the hasher that ASP.NET Core Identity uses: **PBKDF2** with HMAC-SHA512, a 128-bit random salt and **100,000** iterations.

| Package | Project | Version |
|---|---|---|
| `Microsoft.Extensions.Identity.Core` | `Lamazon.Services` | `10.0.12` |
```csharp
// Lamazon.Services/DependencyInjection.cs, in AddServices()
// The same password hashing that ASP.NET Core Identity uses (PBKDF2 with a random salt)
services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
```
| Method | What it does |
|---|---|
| `HashPassword(user, password)` | a new random salt + PBKDF2 → one Base64 string, the salt included |
| `VerifyHashedPassword(user, hash, password)` | reads the salt and the settings from the hash, hashes the typed password the same way, and compares |

| `VerifyHashedPassword` returns | Meaning |
|---|---|
| `Failed` | wrong password |
| `Success` | right password |
| `SuccessRehashNeeded` | right password, but hashed with older, weaker settings: hash it again and save it |

🔍 **The seed data from class 2 now makes sense.** `admin@lamazon.com`'s hash starts with `AQAAAAIAAYagAAAAE…`. In bytes: `01` (the format version), `00 00 00 02` (HMAC-SHA512), `00 01 86 A0` (100,000 iterations), `00 00 00 10` (a 16-byte salt), then the salt and the hash. The salt is random, so hashing `Admin123!` again gives another string, and both are correct.

---

### ⚙️ UsersService
```csharp
public interface IUsersService
{
    /// <summary>Creates a user with the "user" role and returns it.</summary>
    Task<UserViewModel> RegisterAsync(RegisterUserViewModel registerUserViewModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// The user, if the email and password are correct; otherwise null.
    /// On purpose it doesn't say WHICH one was wrong.
    /// </summary>
    Task<UserViewModel?> ValidateCredentialsAsync(UserCredentialsViewModel credentials, CancellationToken cancellationToken = default);

    /// <summary>Throws NotFoundException if the user doesn't exist.</summary>
    Task<UserViewModel> GetByIdAsync(int id, CancellationToken cancellationToken = default);
}
```
```csharp
public class UsersService : IUsersService
{
    private readonly IUsersRepository _usersRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IMapper _mapper;

    public UsersService(IUsersRepository usersRepository, IPasswordHasher<User> passwordHasher, IMapper mapper)
    {
        _usersRepository = usersRepository;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
    }

    public async Task<UserViewModel> RegisterAsync(RegisterUserViewModel registerUserViewModel, CancellationToken cancellationToken = default)
    {
        User user = _mapper.Map<User>(registerUserViewModel);

        // Everyone who registers is a customer. Only an admin can make someone an admin.
        user.RoleKey = Roles.User;

        // Never store the password itself: only a salted hash
        user.PasswordHash = _passwordHasher.HashPassword(user, registerUserViewModel.Password);

        await _usersRepository.AddAsync(user, cancellationToken);

        // Load it again together with its role, so the view model is complete (RoleName)
        return await GetByIdAsync(user.Id, cancellationToken);
    }

    public async Task<UserViewModel?> ValidateCredentialsAsync(UserCredentialsViewModel credentials, CancellationToken cancellationToken = default)
    {
        User? user = await _usersRepository.GetByEmailAsync(credentials.Email, cancellationToken);
        if (user is null)
        {
            return null;
        }

        PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, credentials.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        // The password is right, but it was hashed with older (weaker) settings: hash it again with the current ones
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.HashPassword(user, credentials.Password);
            await _usersRepository.UpdateAsync(user, cancellationToken);
        }

        return _mapper.Map<UserViewModel>(user);
    }

    public async Task<UserViewModel> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        User? user = await _usersRepository.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException(nameof(User), id);
        }

        return _mapper.Map<UserViewModel>(user);
    }
}
```
- 👤 **`RoleKey = Roles.User`:** the service decides the role, not the form.
- 🕵️ **The same answer, `null`, for a wrong email and a wrong password.** If the login said "no account with this email", anyone could test a list of emails and learn who shops at Lamazon ("email enumeration"). The page says only "Invalid email or password."
- 💉 Register it in `AddServices()`: `services.AddScoped<IUsersService, UsersService>();`

---

## 🏠 Homework
None this time. The login form's validator and the AccessDenied page need FluentValidation and the cookie login, so they're part of the class 5 homework.

## ✅ By the end of the class
- 🛍️ `/Products` shows all 6 products, and `/Products/Details/1` shows one
- 🚫 `/Products/Details/999` and `/does-not-exist` show the "Page not found" page
- 📄 `/Privacy` has no footer (the nested layout)
- 👤 `UsersRepository`, `UserMappingConfig` and `UsersService` are registered, and the solution builds. No page uses them yet
- 🚧 **Not finished yet:** `UsersController` has empty actions, and the Login and Register pages are unfinished. We finish them at the start of class 5

---

🔜 **Next class:** first the rest of class 4: FluentValidation, the cookie login, and the Login, Register and Logout pages. Then the cart: Add to cart and Remove from cart without reloading the page.
