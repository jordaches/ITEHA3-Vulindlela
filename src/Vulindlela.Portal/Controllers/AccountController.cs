using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Vulindlela.Portal.Clients;

namespace Vulindlela.Portal.Controllers;

public class AccountController : Controller
{
    private readonly IBookingApiClient _booking;
    public AccountController(IBookingApiClient booking) => _booking = booking;

    [HttpGet]
    public IActionResult SignIn(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SignIn(string idNumber, string password, string? returnUrl, CancellationToken ct)
    {
        var result = await _booking.VerifyPasswordAsync(idNumber, password, ct);
        if (result is null)
        {
            ModelState.AddModelError("", "Invalid ID number or password.");
            return View();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.CitizenId.ToString()),
            new(ClaimTypes.Name, result.FullName),
            new(ClaimTypes.Role, result.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

        return RedirectToLocal(returnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public new async Task<IActionResult> SignOut()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private IActionResult RedirectToLocal(string? returnUrl) =>
        !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? Redirect(returnUrl)
            : RedirectToAction("Index", "Home");
}