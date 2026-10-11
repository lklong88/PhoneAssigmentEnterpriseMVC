using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace CUSoCalPhoneAssignments.Web.Controllers;

[AllowAnonymous]
public sealed class AccountController : Controller
{
    private readonly IConfiguration _configuration;
    public AccountController(IConfiguration configuration) => _configuration = configuration;

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(string? password)
    {
        var expected = _configuration["DEMO_PASSWORD"];
        if (string.IsNullOrWhiteSpace(expected)) return RedirectToAction("Index", "Home");
        var providedHash = SHA256.HashData(Encoding.UTF8.GetBytes(password ?? ""));
        var expectedHash = SHA256.HashData(Encoding.UTF8.GetBytes(expected));
        if (!CryptographicOperations.FixedTimeEquals(providedHash, expectedHash))
        {
            ModelState.AddModelError(string.Empty, "Incorrect access password.");
            return View();
        }
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "Demo reviewer") },
            CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}
