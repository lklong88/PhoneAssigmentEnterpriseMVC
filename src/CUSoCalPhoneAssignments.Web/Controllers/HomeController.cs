using CUSoCalPhoneAssignments.Web.Models;
using CUSoCalPhoneAssignments.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace CUSoCalPhoneAssignments.Web.Controllers;

public sealed class HomeController : Controller
{
    private readonly AssignmentService _service;
    private readonly ILogger<HomeController> _logger;
    public HomeController(AssignmentService service, ILogger<HomeController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index() => View(_service.GetDashboard());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Assign(int memberId, int? phoneNumberId)
    {
        if (phoneNumberId is null or <= 0)
        {
            TempData["Error"] = "Select a valid phone number.";
            return RedirectToAction(nameof(Index));
        }
        return Execute(() => _service.Assign(memberId, phoneNumberId.Value, User.Identity?.Name ?? "Local demo"));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Remove(int memberId) =>
        Execute(() => _service.Remove(memberId, User.Identity?.Name ?? "Local demo"));

    private IActionResult Execute(Func<OperationResult> operation)
    {
        try
        {
            var result = operation();
            TempData[result.Success ? "Success" : "Error"] = result.Message;
        }
        catch (IOException exception)
        {
            _logger.LogError(exception, "Unable to persist phone assignment change");
            TempData["Error"] = "Unable to save changes. Please try again later.";
        }
        catch (UnauthorizedAccessException exception)
        {
            _logger.LogError(exception, "Phone assignment storage permission error");
            TempData["Error"] = "Storage is unavailable. Please contact the administrator.";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public IActionResult Error() => View();
}
