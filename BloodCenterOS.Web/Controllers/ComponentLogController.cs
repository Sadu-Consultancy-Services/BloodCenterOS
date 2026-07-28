using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class ComponentLogController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public ComponentLogController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public IActionResult Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Component Log";
        ViewBag.ActiveMenu = "ComponentLog";
        return View();
    }

    public IActionResult Store(long componentId)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Store Component";
        ViewBag.ActiveMenu = "ComponentLog";
        ViewBag.ComponentId = componentId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Store(long componentId, long fridgeId, string? location, string? notes)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Store Component";
        ViewBag.ActiveMenu = "ComponentLog";
        try
        {
            var r = await _api.StoreComponentAsync(componentId, new { fridgeId, location, notes });
            if (r?.Success == true) { TempData["Success"] = "Component stored"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View();
    }

    public IActionResult Transfer(long componentId)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Transfer Component";
        ViewBag.ActiveMenu = "ComponentLog";
        ViewBag.ComponentId = componentId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Transfer(long componentId, long toCenterId, string? transportDetails)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Transfer Component";
        ViewBag.ActiveMenu = "ComponentLog";
        try
        {
            var r = await _api.TransferComponentAsync(componentId, new { toCenterId, transportDetails });
            if (r?.Success == true) { TempData["Success"] = "Component transferred"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View();
    }

    public IActionResult Discard(long componentId)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Discard Component";
        ViewBag.ActiveMenu = "ComponentLog";
        ViewBag.ComponentId = componentId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Discard(long componentId, long bagId, string reason, string? notes)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Discard Component";
        ViewBag.ActiveMenu = "ComponentLog";
        try
        {
            var r = await _api.DiscardComponentAsync(componentId, new { bagId, reason, notes });
            if (r?.Success == true) { TempData["Success"] = "Component discarded"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View();
    }
}
