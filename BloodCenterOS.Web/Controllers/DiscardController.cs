using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

[Authorize]
public class DiscardController : Controller
{
    private readonly ApiClient _api;
    public DiscardController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index(DateTime? from, DateTime? to, string? reason)
    {
        ViewBag.ActiveMenu = "Discard";
        var result = await _api.GetDiscardRegisterAsync(from, to, reason);
        ViewBag.From = from;
        ViewBag.To = to;
        ViewBag.Reason = reason;
        return View(result?.Data ?? new());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.ActiveMenu = "Discard";
        var result = await _api.GetAvailableComponentsForDiscardAsync();
        ViewBag.AvailableComponents = result?.Data ?? new();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(BulkDiscardRequest req)
    {
        var result = await _api.BulkDiscardAsync(req);
        if (result?.Success == true)
        {
            TempData["Success"] = $"{result.Data?.Count ?? 0} component(s) discarded";
            return RedirectToAction("Index");
        }
        TempData["Error"] = result?.Message ?? "Failed to discard";
        return RedirectToAction("Create");
    }

    public async Task<IActionResult> AutoclaveRegister()
    {
        ViewBag.ActiveMenu = "Discard";
        var result = await _api.GetAutoclaveRegisterAsync();
        return View(result?.Data ?? new());
    }

    public IActionResult SetAutoclave(long discardId)
    {
        ViewBag.ActiveMenu = "Discard";
        ViewBag.DiscardId = discardId;
        return View(new SetAutoclaveRequest());
    }

    [HttpPost]
    public async Task<IActionResult> SetAutoclave(SetAutoclaveRequest req)
    {
        var result = await _api.SetAutoclaveAsync(req);
        if (result?.Success == true)
            TempData["Success"] = "Autoclave times recorded";
        else
            TempData["Error"] = result?.Message ?? "Failed";
        return RedirectToAction("AutoclaveRegister");
    }
}
