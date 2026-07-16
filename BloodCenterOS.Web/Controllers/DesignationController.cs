using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class DesignationController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public DesignationController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Designations";
        ViewBag.ActiveMenu = "Designations";
        var items = new List<Designation>();
        try { var r = await _api.GetDesignationsAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Designation";
        ViewBag.ActiveMenu = "Designations";
        return View(new Designation());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Designation designation)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Designation";
        ViewBag.ActiveMenu = "Designations";
        try
        {
            var r = await _api.CreateDesignationAsync(designation);
            if (r?.Success == true) { TempData["Success"] = "Designation created"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(designation);
    }

    public async Task<IActionResult> Edit(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Designation";
        ViewBag.ActiveMenu = "Designations";
        try { var r = await _api.GetDesignationAsync(id); if (r?.Success == true && r.Data != null) return View(r.Data); } catch { }
        TempData["Error"] = "Designation not found";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(long id, Designation designation)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Designation";
        ViewBag.ActiveMenu = "Designations";
        try
        {
            var r = await _api.UpdateDesignationAsync(id, designation);
            if (r?.Success == true) { TempData["Success"] = "Designation updated"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(designation);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        try { await _api.DeleteDesignationAsync(id); TempData["Success"] = "Designation deleted"; }
        catch { TempData["Error"] = "Failed to delete"; }
        return RedirectToAction("Index");
    }
}
