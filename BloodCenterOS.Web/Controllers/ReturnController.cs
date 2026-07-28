using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class ReturnController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public ReturnController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Returns";
        ViewBag.ActiveMenu = "Returns";
        var items = new List<ReturnRecord>();
        try { var r = await _api.GetReturnsAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Return";
        ViewBag.ActiveMenu = "Returns";
        return View(new ReturnRecord());
    }

    [HttpPost]
    public async Task<IActionResult> Create(long issueRecordId, long componentId, string? reason)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Return";
        ViewBag.ActiveMenu = "Returns";
        try
        {
            var r = await _api.CreateReturnAsync(new { issueRecordId, componentId, reason });
            if (r?.Success == true) { TempData["Success"] = "Return created"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(new ReturnRecord { IssueRecordId = issueRecordId, ComponentId = componentId, Reason = reason });
    }
}
