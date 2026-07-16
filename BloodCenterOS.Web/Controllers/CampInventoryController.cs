using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class CampInventoryController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public CampInventoryController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index(long? campId)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Camp Inventory";
        ViewBag.ActiveMenu = "CampInventory";
        ViewBag.CampId = campId;
        var items = new List<CampInventory>();
        try { var r = await _api.GetCampInventoryAsync(campId); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create(long? campId)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Inventory Item";
        ViewBag.ActiveMenu = "CampInventory";
        ViewBag.CampId = campId;
        return View(new CampInventory { CampId = campId ?? 0 });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CampInventory item)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Inventory Item";
        ViewBag.ActiveMenu = "CampInventory";
        try
        {
            var r = await _api.CreateCampInventoryAsync(new
            {
                campId = item.CampId,
                itemName = item.ItemName,
                quantity = item.Quantity,
                unit = item.Unit
            });
            if (r?.Success == true) { TempData["Success"] = "Item added"; return RedirectToAction("Index", new { campId = item.CampId }); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(item);
    }

    public async Task<IActionResult> Edit(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Inventory Item";
        ViewBag.ActiveMenu = "CampInventory";
        try { var r = await _api.GetCampInventoryAsync(); if (r?.Success == true && r.Data != null) { var it = r.Data.FirstOrDefault(x => x.CampInventoryId == id); if (it != null) return View(it); } } catch { }
        TempData["Error"] = "Item not found";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(long id, CampInventory item)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Inventory Item";
        ViewBag.ActiveMenu = "CampInventory";
        try
        {
            var r = await _api.UpdateCampInventoryAsync(id, new
            {
                itemName = item.ItemName,
                quantity = item.Quantity,
                unit = item.Unit
            });
            if (r?.Success == true) { TempData["Success"] = "Item updated"; return RedirectToAction("Index", new { campId = item.CampId }); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id, long? campId)
    {
        try { await _api.DeleteCampInventoryAsync(id); TempData["Success"] = "Item deleted"; }
        catch { TempData["Error"] = "Failed"; }
        return RedirectToAction("Index", new { campId });
    }
}
