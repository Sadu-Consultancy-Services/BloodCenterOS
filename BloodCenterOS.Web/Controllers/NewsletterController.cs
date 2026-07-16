using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class NewsletterController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public NewsletterController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Newsletter Subscriptions";
        ViewBag.ActiveMenu = "Newsletter";
        var items = new List<NewsletterSubscription>();
        try { var r = await _api.GetNewsletterSubscriptionsAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Subscription";
        ViewBag.ActiveMenu = "Newsletter";
        return View(new NewsletterSubscription());
    }

    [HttpPost]
    public async Task<IActionResult> Create(NewsletterSubscription model)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Subscription";
        ViewBag.ActiveMenu = "Newsletter";
        try
        {
            var r = await _api.CreateNewsletterSubscriptionAsync(new { email = model.Email });
            if (r?.Success == true) { TempData["Success"] = "Subscription added"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleActive(long id)
    {
        try { await _api.ToggleNewsletterActiveAsync(id); TempData["Success"] = "Status toggled"; }
        catch { TempData["Error"] = "Failed"; }
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        try { await _api.DeleteNewsletterSubscriptionAsync(id); TempData["Success"] = "Subscription deleted"; }
        catch { TempData["Error"] = "Failed to delete"; }
        return RedirectToAction("Index");
    }
}
