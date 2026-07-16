using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class FridgeController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public FridgeController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Fridges";
        ViewBag.ActiveMenu = "Fridges";
        var items = new List<Fridge>();
        try { var r = await _api.GetFridgesAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Fridge";
        ViewBag.ActiveMenu = "Fridges";
        return View(new Fridge());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Fridge fridge)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Fridge";
        ViewBag.ActiveMenu = "Fridges";
        try
        {
            var r = await _api.CreateFridgeAsync(fridge);
            if (r?.Success == true) { TempData["Success"] = "Fridge created"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(fridge);
    }

    public async Task<IActionResult> Edit(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Fridge";
        ViewBag.ActiveMenu = "Fridges";
        try { var r = await _api.GetFridgeAsync(id); if (r?.Success == true && r.Data != null) return View(r.Data); } catch { }
        TempData["Error"] = "Fridge not found";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(long id, Fridge fridge)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Fridge";
        ViewBag.ActiveMenu = "Fridges";
        try
        {
            var r = await _api.UpdateFridgeAsync(id, fridge);
            if (r?.Success == true) { TempData["Success"] = "Fridge updated"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(fridge);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        try { await _api.DeleteFridgeAsync(id); TempData["Success"] = "Fridge deleted"; }
        catch { TempData["Error"] = "Failed"; }
        return RedirectToAction("Index");
    }
}
