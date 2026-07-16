using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class DeviceController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public DeviceController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Devices";
        ViewBag.ActiveMenu = "Devices";
        var items = new List<Device>();
        try { var r = await _api.GetDevicesAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Device";
        ViewBag.ActiveMenu = "Devices";
        return View(new Device());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Device device)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Device";
        ViewBag.ActiveMenu = "Devices";
        try
        {
            var r = await _api.CreateDeviceAsync(device);
            if (r?.Success == true) { TempData["Success"] = "Device created"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(device);
    }

    public async Task<IActionResult> Edit(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Device";
        ViewBag.ActiveMenu = "Devices";
        try { var r = await _api.GetDeviceAsync(id); if (r?.Success == true && r.Data != null) return View(r.Data); } catch { }
        TempData["Error"] = "Device not found";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(long id, Device device)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Device";
        ViewBag.ActiveMenu = "Devices";
        try
        {
            var r = await _api.UpdateDeviceAsync(id, device);
            if (r?.Success == true) { TempData["Success"] = "Device updated"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(device);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        try { await _api.DeleteDeviceAsync(id); TempData["Success"] = "Device deleted"; }
        catch { TempData["Error"] = "Failed"; }
        return RedirectToAction("Index");
    }
}
