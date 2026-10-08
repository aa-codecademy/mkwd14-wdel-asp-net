using System.Net;
using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.Models;
using Lamazon.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Lamazon.Web.Controllers;

public class UsersController : Controller
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
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
        if (!ModelState.IsValid)
        {
            return View(registerUserViewModel);
        }

        var user = await _usersService.RegisterAsync(registerUserViewModel, cancellationToken);

        await AuthHelper.SignInUserAsync(HttpContext, user);

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        await AuthHelper.SignOutUserAsync(HttpContext);
        return RedirectToAction("Index", "Home");
    }
}
