using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Models.ViewModels;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class ComponentTypeController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public ComponentTypeController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Component Types";
        ViewBag.ActiveMenu = "ComponentTypes";
        var items = new List<ComponentType>();
        try { var r = await _api.GetComponentTypesAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        var viewModel = items.Select(t => new ComponentTypeItem
        {
            ComponentTypeId = t.ComponentTypeId,
            Code = t.ComponentTypeCode ?? "",
            Name = t.ComponentTypeCode ?? "",
            Description = t.Description ?? ""
        }).ToList();
        return View(viewModel);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Component Type";
        ViewBag.ActiveMenu = "ComponentTypes";
        return View(new ComponentType());
    }

    [HttpPost]
    public async Task<IActionResult> Create(ComponentType item)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Component Type";
        ViewBag.ActiveMenu = "ComponentTypes";
        try
        {
            var r = await _api.CreateComponentTypeAsync(new
            {
                componentTypeCode = item.ComponentTypeCode,
                description = item.Description
            });
            if (r?.Success == true) { TempData["Success"] = "Component type created"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(item);
    }

    public async Task<IActionResult> Edit(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Component Type";
        ViewBag.ActiveMenu = "ComponentTypes";
        try { var r = await _api.GetComponentTypesAsync(); if (r?.Success == true && r.Data != null) { var it = r.Data.FirstOrDefault(x => x.ComponentTypeId == id); if (it != null) return View(it); } } catch { }
        TempData["Error"] = "Component type not found";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(long id, ComponentType item)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Component Type";
        ViewBag.ActiveMenu = "ComponentTypes";
        try
        {
            var r = await _api.UpdateComponentTypeAsync(id, new
            {
                componentTypeCode = item.ComponentTypeCode,
                description = item.Description
            });
            if (r?.Success == true) { TempData["Success"] = "Component type updated"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        try { await _api.DeleteComponentTypeAsync(id); TempData["Success"] = "Component type deleted"; }
        catch { TempData["Error"] = "Failed"; }
        return RedirectToAction("Index");
    }
}
