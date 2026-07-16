using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class EmailTemplateController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public EmailTemplateController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Email Templates";
        ViewBag.ActiveMenu = "EmailTemplates";
        var items = new List<EmailTemplate>();
        try { var r = await _api.GetEmailTemplatesAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Email Template";
        ViewBag.ActiveMenu = "EmailTemplates";
        return View(new EmailTemplate());
    }

    [HttpPost]
    public async Task<IActionResult> Create(EmailTemplate template)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Email Template";
        ViewBag.ActiveMenu = "EmailTemplates";
        try
        {
            var r = await _api.CreateEmailTemplateAsync(template);
            if (r?.Success == true) { TempData["Success"] = "Template created"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(template);
    }

    public async Task<IActionResult> Edit(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Email Template";
        ViewBag.ActiveMenu = "EmailTemplates";
        try { var r = await _api.GetEmailTemplateAsync(id); if (r?.Success == true && r.Data != null) return View(r.Data); } catch { }
        TempData["Error"] = "Template not found";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(long id, EmailTemplate template)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Email Template";
        ViewBag.ActiveMenu = "EmailTemplates";
        try
        {
            var r = await _api.UpdateEmailTemplateAsync(id, template);
            if (r?.Success == true) { TempData["Success"] = "Template updated"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(template);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        try { await _api.DeleteEmailTemplateAsync(id); TempData["Success"] = "Template deleted"; }
        catch { TempData["Error"] = "Failed"; }
        return RedirectToAction("Index");
    }
}
