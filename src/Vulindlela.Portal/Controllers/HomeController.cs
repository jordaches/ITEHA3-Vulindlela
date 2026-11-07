using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Vulindlela.Portal.Clients;
using Vulindlela.Portal.Models;

namespace Vulindlela.Portal.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    private readonly IBookingApiClient _booking;

    public HomeController(ILogger<HomeController> logger, IBookingApiClient booking)
    {
        _logger = logger;
        _booking = booking;
    }

    public async Task<IActionResult> Index(CancellationToken ct)
    {
        ViewData["BookingDown"] = !await _booking.IsAvailableAsync(ct);
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }

}
