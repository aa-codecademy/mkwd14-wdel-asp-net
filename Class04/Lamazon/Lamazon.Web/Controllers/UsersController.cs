using Lamazon.Services.Abstractions;
using Lamazon.ViewModels.Models;
using Microsoft.AspNetCore.Mvc;

namespace Lamazon.Web.Controllers;

public class UsersController : Controller
{
    private readonly IUsersService _usersService;

    public UsersController(IUsersService usersService)
    {
        _usersService = usersService;
    }

    [HttpGet]
    public IActionResult Login() // returnUrl
    {
        return View(new UserCredentialsViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Login(UserCredentialsViewModel credentials, CancellationToken cancellationToken) // returnUrl
    {
        // Login logic

        return null;
    }

    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    public IActionResult Register(RegisterViewModel registerViewModel, CancellationToken cancellationToken)
    {
        // Register logic

        return null;
    }

    public async Task<IActionResult> Logout()
    {
        return null;
    }
}
