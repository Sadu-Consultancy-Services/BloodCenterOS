using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class BranchController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public BranchController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Branches";
        ViewBag.ActiveMenu = "Branches";
        var items = new List<Branch>();
        try { var r = await _api.GetBranchesAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Branch";
        ViewBag.ActiveMenu = "Branches";
        return View(new Branch());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Branch branch)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Branch";
        ViewBag.ActiveMenu = "Branches";
        try
        {
            var r = await _api.CreateBranchAsync(branch);
            if (r?.Success == true) { TempData["Success"] = "Branch created"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(branch);
    }

    public async Task<IActionResult> Edit(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Branch";
        ViewBag.ActiveMenu = "Branches";
        try { var r = await _api.GetBranchAsync(id); if (r?.Success == true && r.Data != null) return View(r.Data); } catch { }
        TempData["Error"] = "Branch not found";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(long id, Branch branch)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Branch";
        ViewBag.ActiveMenu = "Branches";
        try
        {
            var r = await _api.UpdateBranchAsync(id, branch);
            if (r?.Success == true) { TempData["Success"] = "Branch updated"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(branch);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        try { await _api.DeleteBranchAsync(id); TempData["Success"] = "Branch deleted"; }
        catch { TempData["Error"] = "Failed to delete"; }
        return RedirectToAction("Index");
    }
}
