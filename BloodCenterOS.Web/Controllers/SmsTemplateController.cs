using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class SmsTemplateController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public SmsTemplateController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "SMS Templates";
        ViewBag.ActiveMenu = "SmsTemplates";
        var items = new List<SmsTemplate>();
        try { var r = await _api.GetSmsTemplatesAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add SMS Template";
        ViewBag.ActiveMenu = "SmsTemplates";
        return View(new SmsTemplate());
    }

    [HttpPost]
    public async Task<IActionResult> Create(SmsTemplate template)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add SMS Template";
        ViewBag.ActiveMenu = "SmsTemplates";
        try
        {
            var r = await _api.CreateSmsTemplateAsync(template);
            if (r?.Success == true) { TempData["Success"] = "Template created"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(template);
    }

    public async Task<IActionResult> Edit(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit SMS Template";
        ViewBag.ActiveMenu = "SmsTemplates";
        try { var r = await _api.GetSmsTemplateAsync(id); if (r?.Success == true && r.Data != null) return View(r.Data); } catch { }
        TempData["Error"] = "Template not found";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(long id, SmsTemplate template)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit SMS Template";
        ViewBag.ActiveMenu = "SmsTemplates";
        try
        {
            var r = await _api.UpdateSmsTemplateAsync(id, template);
            if (r?.Success == true) { TempData["Success"] = "Template updated"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(template);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        try { await _api.DeleteSmsTemplateAsync(id); TempData["Success"] = "Template deleted"; }
        catch { TempData["Error"] = "Failed"; }
        return RedirectToAction("Index");
    }
}
