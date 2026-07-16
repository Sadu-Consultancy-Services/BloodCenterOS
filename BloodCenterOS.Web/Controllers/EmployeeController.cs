using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class EmployeeController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public EmployeeController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Employees";
        ViewBag.ActiveMenu = "Employees";
        var items = new List<Employee>();
        try { var r = await _api.GetEmployeesAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public async Task<IActionResult> Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Employee";
        ViewBag.ActiveMenu = "Employees";
        ViewBag.Departments = await GetDepartmentsAsync();
        return View(new Employee());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Employee employee)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Employee";
        ViewBag.ActiveMenu = "Employees";
        ViewBag.Departments = await GetDepartmentsAsync();
        try
        {
            var r = await _api.CreateEmployeeAsync(employee);
            if (r?.Success == true) { TempData["Success"] = "Employee created"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(employee);
    }

    public async Task<IActionResult> Edit(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Employee";
        ViewBag.ActiveMenu = "Employees";
        ViewBag.Departments = await GetDepartmentsAsync();
        try { var r = await _api.GetEmployeeAsync(id); if (r?.Success == true && r.Data != null) return View(r.Data); } catch { }
        TempData["Error"] = "Employee not found";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(long id, Employee employee)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Employee";
        ViewBag.ActiveMenu = "Employees";
        ViewBag.Departments = await GetDepartmentsAsync();
        try
        {
            var r = await _api.UpdateEmployeeAsync(id, employee);
            if (r?.Success == true) { TempData["Success"] = "Employee updated"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(employee);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(long id)
    {
        try { await _api.ToggleEmployeeActiveAsync(id); TempData["Success"] = "Status toggled"; }
        catch { TempData["Error"] = "Failed"; }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        try { await _api.DeleteEmployeeAsync(id); TempData["Success"] = "Employee deleted"; }
        catch { TempData["Error"] = "Failed to delete"; }
        return RedirectToAction("Index");
    }

    private async Task<List<Department>> GetDepartmentsAsync()
    {
        try { var r = await _api.GetDepartmentsAsync(); if (r?.Success == true && r.Data != null) return r.Data; } catch { }
        return new List<Department>();
    }
}
