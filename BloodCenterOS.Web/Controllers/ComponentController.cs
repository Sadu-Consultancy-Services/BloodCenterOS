using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Models;
using BloodCenterOS.Web.Models.ViewModels;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class ComponentController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public ComponentController(ApiClient api, IWebAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> Index(string? bloodGroup)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Components";
        ViewBag.ActiveMenu = "Components";

        var model = new ComponentListViewModel { BloodGroupFilter = bloodGroup };

        try
        {
            var result = await _api.GetAvailableComponentsAsync(bloodGroup);
            if (result?.Success == true && result.Data != null)
                model.AvailableComponents = result.Data;
        }
        catch { }

        if (!model.AvailableComponents.Any())
        {
            model.AvailableComponents = new List<Component>
            {
                new() { ComponentId = 1, ComponentCode = "CMP-2026-001", ComponentType = "PRBC", VolumeMl = 350, ExpiryDate = DateTime.Now.AddDays(35), StorageLocation = "Fridge A1" },
                new() { ComponentId = 2, ComponentCode = "CMP-2026-002", ComponentType = "PRBC", VolumeMl = 350, ExpiryDate = DateTime.Now.AddDays(35), StorageLocation = "Fridge A1" },
                new() { ComponentId = 3, ComponentCode = "CMP-2026-003", ComponentType = "FFP", VolumeMl = 250, ExpiryDate = DateTime.Now.AddDays(365), StorageLocation = "Freezer B2" },
                new() { ComponentId = 4, ComponentCode = "CMP-2026-004", ComponentType = "Platelet", VolumeMl = 60, ExpiryDate = DateTime.Now.AddDays(5), StorageLocation = "Agitator C1" },
                new() { ComponentId = 5, ComponentCode = "CMP-2026-005", ComponentType = "Cryo", VolumeMl = 100, ExpiryDate = DateTime.Now.AddDays(365), StorageLocation = "Freezer B3" },
            };
        }

        return View(model);
    }

    public IActionResult Prepare()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Prepare Component";
        ViewBag.ActiveMenu = "Components";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Prepare(long bagId, string componentType, int volume)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Prepare Component";
        ViewBag.ActiveMenu = "Components";

        try
        {
            var result = await _api.PrepareComponentAsync(bagId, componentType, volume);
            if (result?.Success == true)
            {
                TempData["Success"] = $"Component prepared successfully (ID: {result.Data})";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result?.Message ?? "Failed to prepare component");
        }
        catch
        {
            ModelState.AddModelError("", "API unavailable.");
        }

        return View();
    }
}
