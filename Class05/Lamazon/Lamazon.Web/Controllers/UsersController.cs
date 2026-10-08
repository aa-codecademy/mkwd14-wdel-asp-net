using System.Net;
using FluentValidation;
using FluentValidation.Results;
using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.Models;
using Lamazon.Web.Extensions;
using Lamazon.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Lamazon.Web.Controllers;

public class UsersController : Controller
{
    private readonly IUsersService _usersService;
    private readonly IValidator<RegisterUserViewModel> _registerValidator;

    public UsersController(
        IUsersService usersService,
        IValidator<RegisterUserViewModel> registerValidator
    )
    {
        _usersService = usersService;
        _registerValidator = registerValidator;
    }

    // admin@lamazon.com
    // Admin123!
    [HttpGet]
    public IActionResult Login(string? returnUrl) 
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new UserCredentialsViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    // ValidateAntiForgeryToken is used to prevent Cross-Site Request Forgery (CSRF) attacks.
    // It ensures that the request is coming from the same site and not from a malicious source.
    // The token is generated and included in the form when the page is rendered, and it must be sent back with the form submission.
    public async Task<IActionResult> Login(UserCredentialsViewModel credentials, string? returnUrl, CancellationToken cancellationToken)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(credentials);
        }

        UserViewModel? user = await _usersService.ValidateCredentialsAsync(credentials, cancellationToken);
        if (user is null)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(credentials);
        }

        await AuthHelper.SignInUserAsync(HttpContext, user);

        // Check if the returnUrl is a local URL to prevent open redirect vulnerabilities.
        return Url.IsLocalUrl(returnUrl) ? LocalRedirect(returnUrl) : RedirectToAction("Index", "Home");
    }

    public IActionResult Register()
    {
        return View(new RegisterUserViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterUserViewModel registerUserViewModel, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _registerValidator.ValidateAsync(registerUserViewModel, cancellationToken);
        validationResult.AddToModelState(ModelState);

        // ModelState => Represents the state of the model and holds validation information.
        // It is used to check if the model is valid or not.
        if (!ModelState.IsValid)
        {
            return View(registerUserViewModel);
        }

        var user = await _usersService.RegisterAsync(registerUserViewModel, cancellationToken);

        await AuthHelper.SignInUserAsync(HttpContext, user);

        return RedirectToAction("Index", "Home");
    }

    // POST, not GET: a link or an <img src="/Users/Logout"> on another site can't log the user out
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await AuthHelper.SignOutUserAsync(HttpContext);
        return RedirectToAction("Index", "Home");
    }
}
