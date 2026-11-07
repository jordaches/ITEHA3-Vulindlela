using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vulindlela.Portal.Clients;
using Vulindlela.Portal.ViewModels;
using Vulindlela.Shared.Contracts;
using Vulindlela.Shared.Security;

namespace Vulindlela.Portal.Controllers;

public class AccountController : Controller
{
    private readonly IBookingApiClient _booking;
    private readonly IClearanceApiClient _clearance;
    public AccountController(IBookingApiClient booking, IClearanceApiClient clearance)
    {
        _booking = booking;
        _clearance = clearance;
    }

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



    [Authorize]
    [HttpGet]
    public async Task<IActionResult> AccessLog(int? id, CancellationToken ct)
    {
        var currentId = User.GetCitizenId();
        if (currentId is null) return Challenge();

        var targetId = id ?? currentId.Value;
        IReadOnlyList<AccessLogEntryDto> bookingLogs = Array.Empty<AccessLogEntryDto>();
        IReadOnlyList<AccessLogEntryDto> clearanceLogs = Array.Empty<AccessLogEntryDto>();
        string? error = null;

        try
        {
            var bTask = _booking.GetAccessLogAsync(targetId, ct);
            var cTask = _clearance.GetAccessLogAsync(targetId, ct);
            await Task.WhenAll(bTask, cTask);
            bookingLogs = await bTask;
            clearanceLogs = await cTask;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            error = "Some access logs are temporarily unavailable.";
        }

        var combined = bookingLogs.Concat(clearanceLogs)
            .OrderByDescending(x => x.AccessedAtUtc)
            .ToList();

        return View(new AccessLogViewModel(targetId, combined, error));
    }
}