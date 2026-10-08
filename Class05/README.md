# 🛒 Class 05: The cart and orders

> 🛍️ The shop · class 5 of 10 · 🏠 [Course overview](../README.md) · ⬅️ [Class 04](../Class04/README.md) · 🗂️ [The domain](../Class01/domain.md) · 🗺️ [Database diagram](../Class01/Lamazon%20DB%20Diagram.png)

## 🗺️ What we do today

1. 🔁 Recap
2. 🍪 Cookie authentication
3. 🖥️ `UsersController`: Login and Logout, `[Authorize]`, anti-forgery tokens, open redirects
4. ✅ FluentValidation, on the Register page


---

## 💡 Used concepts


### 🍪 Cookie authentication
First the cookie's name. A cookie is an HTTP detail, so it lives in the Web project, not in Domain:
```csharp
// Lamazon.Web/Constants/Cookies.cs
/// <summary>The names of the app's cookies.</summary>
public static class Cookies
{
    /// <summary>The login cookie: the user's claims, encrypted (see AddWeb).</summary>
    public const string Auth = "Lamazon.Auth";
}
```
In class 6 the cart's cookie joins it.

**`AddWeb()`, with the cookie login:**
```csharp
public static IServiceCollection AddWeb(this IServiceCollection services)
{
    services.AddControllersWithViews(options =>
    {
        // Turns NotFoundException into a 404 page (see Filters)
        options.Filters.Add<AppExceptionFilter>();
    });

    // Cookie authentication: after login, the browser keeps an encrypted cookie with the user's claims
    // and sends it with every request
    services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddCookie(options =>
        {
            options.LoginPath = "/Users/Login";                 // not logged in → go here
            options.AccessDeniedPath = "/Users/AccessDenied";   // logged in, but not allowed (e.g. not an admin)
            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.SlidingExpiration = true;                   // every visit extends the hour
            options.Cookie.Name = Cookies.Auth;
            options.Cookie.HttpOnly = true;                     // JavaScript can't read it
            options.Cookie.SameSite = SameSiteMode.Lax;
        });

    services.AddAuthorization();

    return services;
}
```
| Option | What it does |
|---|---|
| `LoginPath` | an anonymous visitor on an `[Authorize]` page is sent here, with `?ReturnUrl=` the page they wanted |
| `AccessDeniedPath` | a logged-in user without the right role is sent here (homework) |
| `ExpireTimeSpan` + `SlidingExpiration` | the login lasts an hour, and every visit in the second half of that hour renews it |
| `Cookie.HttpOnly` | `document.cookie` doesn't show it, so a script injected into a page (XSS, class 6) can't steal it |
| `Cookie.SameSite = Lax` | the browser doesn't send it with a POST that comes from another site |

```csharp
// Program.cs, after UseRouting()
// First "who are you?" (reads the login cookie), then "are you allowed?" ([Authorize])
app.UseAuthentication();
app.UseAuthorization();
```
↕️ **The order matters:** `UseAuthorization` needs to know who the user is, and that's what `UseAuthentication` reads from the cookie.

**Logging in and out**, in `Lamazon.Web/Helpers/AuthHelper.cs`:
```csharp
public static class AuthHelper
{
    /// <summary>
    /// Logs the user in: the claims (facts about the user) are encrypted into the login cookie.
    /// From now on, User in every controller and view is this user.
    /// </summary>
    public static async Task SignInUserAsync(HttpContext httpContext, UserViewModel user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            // [Authorize(Roles = Roles.Admin)] and User.IsInRole(...) read this claim
            new(ClaimTypes.Role, user.RoleKey),
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            new AuthenticationProperties
            {
                // Keep the cookie after the browser is closed (until it expires)
                IsPersistent = true,
            });
    }

    /// <summary>Logs the user out: deletes the login cookie.</summary>
    public static async Task SignOutUserAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
```
| Claim | Value | Read with |
|---|---|---|
| `NameIdentifier` | the user's id | `User.GetUserId()`: who places the order (class 6) |
| `Name` | the full name | `User.DisplayName()`: the navbar |
| `Email` | the email | `User.FindFirstValue(ClaimTypes.Email)` |
| `Role` | the role key: `admin` or `user` | `[Authorize(Roles = Roles.Admin)]`, `User.IsAdmin()` |

📄 **Handout:** shortcuts for reading the claims, in `Lamazon.Web/Extensions/ClaimsPrincipalExtensions.cs`:
```csharp
public static class ClaimsPrincipalExtensions
{
    public static bool IsAdmin(this ClaimsPrincipal user)
    {
        return user.IsInRole(Roles.Admin);
    }

    public static string DisplayName(this ClaimsPrincipal user)
    {
        return user.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
    }
}
```
`GetUserId()` joins them in class 6, with the orders. Add `@using Lamazon.Web.Extensions` to `Views/_ViewImports.cshtml`, so the views can call them.

🔍 **In the browser's dev tools** (Application → Cookies), the value of `Lamazon.Auth` is unreadable: ASP.NET Core encrypts and signs it. Change one character and you're logged out. The role is in the cookie too, so nobody can make themselves an admin by editing it.

---

### 🖥️ UsersController: Login and Logout
In class 4 we created `UsersController` with empty actions. Now we fill them in. In class 6 it inherits `BaseController`, like the other shop controllers.
```csharp
public IActionResult Login(string? returnUrl)
{
    ViewData["ReturnUrl"] = returnUrl;
    return View(new UserCredentialsViewModel());
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Login(UserCredentialsViewModel credentials, string? returnUrl, CancellationToken cancellationToken)
{
    ViewData["ReturnUrl"] = returnUrl;

    // Homework: validate the form first, like Register (UserCredentialsViewModelValidator)

    UserViewModel? user = await _usersService.ValidateCredentialsAsync(credentials, cancellationToken);
    if (user is null)
    {
        // The same message for "no such email" and "wrong password"
        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(credentials);
    }

    await AuthHelper.SignInUserAsync(HttpContext, user);

    // Only redirect to our own pages. ?returnUrl=https://evil.example would be an "open redirect".
    return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Home");
}

// POST, not GET: see "Logout is a POST" below
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Logout()
{
    await AuthHelper.SignOutUserAsync(HttpContext);
    return RedirectToAction("Index", "Home");
}
```
- 📮 **GET and POST with the same name:** the GET shows the empty form, the `[HttpPost]` one receives it.
- 🔁 **A wrong login returns the same view with the same model:** the email stays filled, and the message appears at the top. The password field comes back empty: the tag helper never writes a password into the page.

**Where `returnUrl` comes from:**
```
/Privacy  →  [Authorize]: nobody is logged in  →  302 /Users/Login?ReturnUrl=%2FPrivacy
          →  GET Login(returnUrl)  →  ViewData  →  <form action="/Users/Login?returnUrl=%2FPrivacy" method="post">
          →  POST Login(..., returnUrl)  →  logged in  →  LocalRedirect("/Privacy")
```
- 🍪 **The cookie login writes it:** it sends the visitor to `LoginPath` and adds the page they wanted as `?ReturnUrl=`. Model binding ignores case, so it fills `returnUrl`.
- 🧳 **The form carries it to the POST:** HTTP forgets everything between two requests, so the GET puts it into `ViewData`, and `asp-route-returnUrl` writes it into the form's address.
- 📮 **`method="post"` matters:** a GET form replaces the query string of its address with the form's fields, and `returnUrl` would be lost (and the password would be in the address bar).

📄 **Handout:** `Views/Users/Login.cshtml`. The parts that matter:
```cshtml
@model UserCredentialsViewModel

@* asp-route-returnUrl keeps the page the user wanted to open, to go back to it after the login *@
<form asp-controller="Users" asp-action="Login" asp-route-returnUrl="@ViewData["ReturnUrl"]" method="post">
    @* Errors that belong to no field, e.g. "Invalid email or password." *@
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>

    <label asp-for="Email" class="form-label"></label>
    <input asp-for="Email" class="form-control" type="email" autocomplete="username" />
    <span asp-validation-for="Email" class="text-danger"></span>

    <label asp-for="Password" class="form-label"></label>
    <input asp-for="Password" class="form-control" autocomplete="current-password" />
    <span asp-validation-for="Password" class="text-danger"></span>

    <button type="submit" class="btn btn-success">Login</button>
</form>
```
| Tag helper | What it writes |
|---|---|
| `<label asp-for="Email">` | `<label for="Email">Email</label>`, with the text from `[Display]` if there is one |
| `<input asp-for="Email">` | `id="Email" name="Email" value="…"`: the `name` is what model binding reads; the `type` comes from the property (`[DataType(DataType.Password)]` → `password`) |
| `<span asp-validation-for="Email">` | the first error of that field from `ModelState`, or nothing |
| `<div asp-validation-summary="ModelOnly">` | the errors added with an empty key: `AddModelError(string.Empty, …)` |
| `<form method="post" …>` | the `action` URL, **and a hidden `__RequestVerificationToken` field** (see below) |

**The navbar** knows who is logged in. In `_Navbar.cshtml`, replace the Login and Register items:
```cshtml
@if (User.Identity?.IsAuthenticated == true)
{
    <li class="nav-item dropdown">
        <a class="nav-link dropdown-toggle text-dark" href="#" role="button" data-bs-toggle="dropdown" aria-expanded="false">
            <i class="material-icons align-middle">person</i> @User.DisplayName()
        </a>
        <ul class="dropdown-menu dropdown-menu-end">
            @if (User.IsAdmin())
            {
                <li><a class="dropdown-item">Lamazon CMS</a></li>
            }
            <li>
                @* Logout is a POST (with an anti-forgery token), so another site can't log you out with a link *@
                <form asp-area="" asp-controller="Users" asp-action="Logout" method="post">
                    <button type="submit" class="dropdown-item">Logout</button>
                </form>
            </li>
        </ul>
    </li>
}
else
{
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="" asp-controller="Users" asp-action="Login">Login</a>
    </li>
    <li class="nav-item">
        <a class="nav-link text-dark" asp-area="" asp-controller="Users" asp-action="Register">Register</a>
    </li>
}
```
Only an admin sees "Lamazon CMS". It gets its link in class 7, with the CMS.

### 🚪 [Authorize]
| Attribute | Who gets in |
|---|---|
| (none) | everyone |
| `[Authorize]` | any logged-in user. Anyone else → the login page |
| `[Authorize(Roles = Roles.Admin)]` | only admins. A logged-in customer → the AccessDenied page (homework) |
| `[AllowAnonymous]` | everyone, even inside an `[Authorize]` controller |

It goes on an action, or on the whole controller. In class 6, `OrdersController` gets it: only a logged-in customer can order.

🧪 **Try it:** put `[Authorize]` on `HomeController.Privacy` (`using Microsoft.AspNetCore.Authorization;`), log out and open `/Privacy`.
1. You land on `/Users/Login?ReturnUrl=%2FPrivacy`
2. Log in, and you're back on `/Privacy`

Remove the attribute afterwards.

### 🛡️ Anti-forgery tokens (CSRF)
**The attack:** you're logged in to Lamazon. Another site has a hidden form that POSTs to `https://localhost:…/Users/Logout` (or, later, to "place an order"). Browsers attach cookies to requests by themselves, so the request would carry **your** login cookie and look like yours. This is cross-site request forgery (**CSRF**).

`SameSite = Lax` already stops most of this in modern browsers, but it's a second line of defense, not the fix: an older browser ignores it, and a page on another subdomain of the same site (`evil.lamazon.com`) still counts as "the same site".

**The fix:** every form also sends a secret token that's written into **our** page. The other site can't read our pages, so it can't know the token.
- 🏷️ **The form tag helper** adds `<input type="hidden" name="__RequestVerificationToken" …>` to every `method="post"` form, by itself.
- ✅ **`[ValidateAntiForgeryToken]`** on the POST action checks it. A missing or wrong token → `400 Bad Request`.
- 🛒 **AJAX** (the cart, class 6) sends the same token in a header. That's what `@Html.AntiForgeryToken()` at the top of `_Layout` is for.

🧪 **Try it:** open the Login page, delete the hidden `__RequestVerificationToken` input in the browser's dev tools (Elements), and submit. The answer is `400`.

🚪 **Logout is a POST.** A GET must never change anything: browsers and other sites trigger GETs freely (a link, a prefetch, an `<img src>`). Logging out changes something, so it's a POST form with a token.

### ↪️ Open redirects
`returnUrl` comes from the address bar, so anyone can write anything in it:
```
https://localhost:7038/Users/Login?returnUrl=https://evil.example/login
```
The link is ours, so the victim trusts it and logs in. With `Redirect(returnUrl)` they would land on a copy of our login page that says "Wrong password, try again", and type their password into **someone else's** site.
- ✅ **`Url.IsLocalUrl(returnUrl)`** is true only for a path on our own site (`/Privacy`), not for `https://…` or `//evil.example`.
- 🧱 **`LocalRedirect`** throws if the URL isn't local, so it's a second check.

🧪 **Try it:** open the link above (with your own port, from `launchSettings.json`) and log in. You land on the home page, not on `evil.example`.

---

### ✅ FluentValidation
**Without it**, the Register form takes anything: a one-letter password, two passwords that don't match, and an email that's already taken, which crashes on the unique index from class 2. The rules of a form go into C#, in their own class:
| Package | Project | Version |
|---|---|---|
| `FluentValidation` | `Lamazon.Services` | `12.1.1` |
| `FluentValidation.DependencyInjectionExtensions` | `Lamazon.Services` | `12.1.1` |

```csharp
// Lamazon.Services/Validators/RegisterUserViewModelValidator.cs
public class RegisterUserViewModelValidator : AbstractValidator<RegisterUserViewModel>
{
    // A validator is a normal class: it can receive services through its constructor (dependency injection)
    public RegisterUserViewModelValidator(IUsersRepository usersRepository)
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(500);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255)
            // An async rule that asks the database. It only runs when the rules above passed (CascadeMode.Stop).
            .MustAsync(async (email, cancellationToken) => !await usersRepository.EmailExistsAsync(email, cancellationToken))
            .WithMessage("An account with this email already exists.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(200);

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("The passwords don't match.");
    }
}
```
| Rule | Checks |
|---|---|
| `NotEmpty()` | not `null`, not `""`, not only spaces |
| `EmailAddress()` | looks like an email: something `@` something |
| `MaximumLength(255)` | the same limits as the columns (class 2), so the database never gets a value that's too long |
| `MustAsync(...)` | our own rule, with a question for the database: is the email still free? |
| `Equal(x => x.Password)` | the same value as another property |
| `.WithMessage("...")` | the text next to the field |

```csharp
// Lamazon.Services/DependencyInjection.cs
private static void AddValidators(this IServiceCollection services)
{
    // Finds every AbstractValidator<T> in this project and registers it as IValidator<T>.
    // Scoped: some validators use repositories.
    services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

    // Stop at the first failing rule of a property: "Email is required", not also "Invalid email format"
    ValidatorOptions.Global.DefaultRuleLevelCascadeMode = CascadeMode.Stop;
}
```
Call it from `AddServices()`, next to `services.AddMappers();`.

**Explicit validation:** the controller runs the validator itself.
```
POST /Users/Register
  → ValidateAsync(model)              FluentValidation: the rules, the database check
  → AddToModelState(ModelState)       its errors become MVC's errors
  → ModelState.IsValid?
       no  → return View(model)       the same form, with the messages next to the fields
       yes → the service
```
🤔 **Why not automatic?** MVC's built-in validation is synchronous, so it can't run `MustAsync`, and FluentValidation no longer recommends its automatic ASP.NET integration. Explicit validation is one line more per action, and we can see what happens.

**FluentValidation's errors → `ModelState`**, in `Lamazon.Web/Extensions/ValidationResultExtensions.cs`:
```csharp
public static class ValidationResultExtensions
{
    /// <summary>
    /// Copies FluentValidation's errors into ModelState, so the view shows them
    /// next to the fields (asp-validation-for) and ModelState.IsValid becomes false.
    /// </summary>
    public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState)
    {
        foreach (var error in result.Errors)
        {
            // A field that already has an error (e.g. "This field is required." from model binding) keeps only that one
            if (modelState.TryGetValue(error.PropertyName, out var entry) && entry.Errors.Count > 0)
            {
                continue;
            }

            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}
```

**In `UsersController`**, the validator arrives through the constructor, like the service:
```csharp
private readonly IUsersService _usersService;
private readonly IValidator<RegisterUserViewModel> _registerValidator;

public UsersController(IUsersService usersService, IValidator<RegisterUserViewModel> registerValidator)
{
    _usersService = usersService;
    _registerValidator = registerValidator;
}

public IActionResult Register()
{
    return View(new RegisterUserViewModel());
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Register(RegisterUserViewModel registerUserViewModel, CancellationToken cancellationToken)
{
    // FluentValidation: run the validator ourselves and copy its errors into ModelState
    var validationResult = await _registerValidator.ValidateAsync(registerUserViewModel, cancellationToken);
    validationResult.AddToModelState(ModelState);
    if (!ModelState.IsValid)
    {
        return View(registerUserViewModel);
    }

    // A new account is logged in right away
    UserViewModel user = await _usersService.RegisterAsync(registerUserViewModel, cancellationToken);
    await AuthHelper.SignInUserAsync(HttpContext, user);

    return RedirectToAction("Index", "Home");
}
```
- 📄 **Handout:** `Views/Users/Register.cshtml`, the same pattern as the Login form, with four fields: `FullName`, `Email`, `Password` and `ConfirmPassword`.
- 👋 **The "Welcome to Lamazon" message** after Register comes in class 6, with the notifications.

🧪 **Try it:**
| Send | You should see |
|---|---|
| `admin@lamazon.com` as the email | "An account with this email already exists." (and no crash) |
| `1234567` as the password | "Password must be at least 8 characters long." |
| two different passwords | "The passwords don't match." |
| a real account | you're logged in, and your name is in the navbar. In SSMS, the new row in `dbo.Users` has a hash in `PasswordHash`, and the role `user` |

⚠️ **An empty field still shows MVC's own message.** With `<Nullable>enable</Nullable>`, MVC treats every non-nullable `string` as `[Required]`, and adds "The Full name field is required." before our validator runs. `AddToModelState` keeps the first error of a field, so that's the one you see. At the start of class 6 we turn MVC's version off in `AddWeb()`, and then the validator's "Full name is required." shows:
```csharp
// FluentValidation checks our forms. Without this line MVC would ALSO treat every
// non-nullable string as [Required] and add its own "The X field is required." errors.
options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
```

---

## 🏠 Homework
Both are from class 4: they needed FluentValidation and the cookie login.

### 1. ✅ `UserCredentialsViewModelValidator` (`Lamazon.Services/Validators`)
The login form's rules, like `RegisterUserViewModelValidator`:
| Field | Rules and messages |
|---|---|
| `Email` | required: "Email is required."; a valid email: "Invalid email format." |
| `Password` | required: "Password is required." |

- 🚫 **No database rule:** whether the account exists is the service's answer, and it says the same thing for a wrong email and a wrong password.
- 🔎 `AddValidatorsFromAssembly` finds the new class by itself.
- 🖥️ **Then use it in `UsersController`:** receive an `IValidator<UserCredentialsViewModel>` in the constructor, and in the `Login` POST validate first, exactly like `Register` (`ValidateAsync` → `AddToModelState` → `ModelState.IsValid`).

| Send | You should see |
|---|---|
| an empty form | "The Email field is required." and "The Password field is required." next to the fields: MVC's own messages, until class 6 (see ⚠️ above). Then "Email is required." and "Password is required." |
| `abc` as the email | "Invalid email format." The browser checks `type="email"` first: to see the server's message, add the `novalidate` attribute to the `<form>` in the dev tools |
| a valid email with a wrong password | "Invalid email or password." at the top (from the service, not from the validator) |

### 2. 🚫 The `AccessDenied` page
`AddWeb()` already sends a logged-in user without the right role to `/Users/AccessDenied`. Today that address answers with a 404.
- an `AccessDenied()` action in `UsersController`: a plain GET that returns the view
- `Views/Users/AccessDenied.cshtml`: the title "Access denied", a sentence ("You are logged in, but you are not allowed to open this page.") and a button back to the home page, like `Error.cshtml`

✅ **Check it:** put `[Authorize(Roles = Roles.Admin)]` on `HomeController.Privacy` (`using Lamazon.Domain.Constants;`).
| Logged in as | `/Privacy` shows |
|---|---|
| `user@lamazon.com` / `User123!` | your AccessDenied page |
| `admin@lamazon.com` / `Admin123!` | the Privacy page |
| nobody | the login page |

Remove the attribute afterwards. The CMS uses the AccessDenied page in class 7.

---

🔜 **Next class:** pop-up notifications and XSS, the cart in a cookie with Add to cart and Remove from cart without reloading the page, the cart page, and placing an order with the prices from the database.
