using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class NotificationController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public NotificationController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Notifications";
        ViewBag.ActiveMenu = "Notifications";
        var items = new List<Notification>();
        try { var r = await _api.GetNotificationsAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Create Notification";
        ViewBag.ActiveMenu = "Notifications";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(string type, string title, string body, string audience)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Create Notification";
        ViewBag.ActiveMenu = "Notifications";
        try
        {
            var r = await _api.CreateNotificationAsync(new { notificationType = type, title, body, targetAudience = audience });
            if (r?.Success == true) { TempData["Success"] = "Notification created"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View();
    }
}
