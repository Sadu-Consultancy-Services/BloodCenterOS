using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class DepartmentController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public DepartmentController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Departments";
        ViewBag.ActiveMenu = "Departments";
        var items = new List<Department>();
        try { var r = await _api.GetDepartmentsAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Department";
        ViewBag.ActiveMenu = "Departments";
        return View(new Department());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Department department)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Department";
        ViewBag.ActiveMenu = "Departments";
        try
        {
            var r = await _api.CreateDepartmentAsync(department);
            if (r?.Success == true) { TempData["Success"] = "Department created"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(department);
    }

    public async Task<IActionResult> Edit(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Department";
        ViewBag.ActiveMenu = "Departments";
        try { var r = await _api.GetDepartmentAsync(id); if (r?.Success == true && r.Data != null) return View(r.Data); } catch { }
        TempData["Error"] = "Department not found";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(long id, Department department)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Department";
        ViewBag.ActiveMenu = "Departments";
        try
        {
            var r = await _api.UpdateDepartmentAsync(id, department);
            if (r?.Success == true) { TempData["Success"] = "Department updated"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(department);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        try { await _api.DeleteDepartmentAsync(id); TempData["Success"] = "Department deleted"; }
        catch { TempData["Error"] = "Failed to delete"; }
        return RedirectToAction("Index");
    }
}
