using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class CampOrganizerController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public CampOrganizerController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Camp Organizers";
        ViewBag.ActiveMenu = "CampOrganizers";
        var items = new List<CampOrganizer>();
        try { var r = await _api.GetCampOrganizersAsync(); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Camp Organizer";
        ViewBag.ActiveMenu = "CampOrganizers";
        return View(new CampOrganizer());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CampOrganizer organizer)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Camp Organizer";
        ViewBag.ActiveMenu = "CampOrganizers";
        try
        {
            var r = await _api.CreateCampOrganizerAsync(new
            {
                organizerName = organizer.OrganizerName,
                contactPerson = organizer.ContactPerson,
                phone = organizer.Phone,
                email = organizer.Email,
                address = organizer.Address
            });
            if (r?.Success == true) { TempData["Success"] = "Organizer created"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(organizer);
    }

    public async Task<IActionResult> Edit(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Camp Organizer";
        ViewBag.ActiveMenu = "CampOrganizers";
        try { var r = await _api.GetCampOrganizerAsync(id); if (r?.Success == true && r.Data != null) return View(r.Data); } catch { }
        TempData["Error"] = "Organizer not found";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(long id, CampOrganizer organizer)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Camp Organizer";
        ViewBag.ActiveMenu = "CampOrganizers";
        try
        {
            var r = await _api.UpdateCampOrganizerAsync(id, new
            {
                organizerName = organizer.OrganizerName,
                contactPerson = organizer.ContactPerson,
                phone = organizer.Phone,
                email = organizer.Email,
                address = organizer.Address
            });
            if (r?.Success == true) { TempData["Success"] = "Organizer updated"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(organizer);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        try { await _api.DeleteCampOrganizerAsync(id); TempData["Success"] = "Organizer deleted"; } catch { TempData["Error"] = "Delete failed"; }
        return RedirectToAction("Index");
    }
}
