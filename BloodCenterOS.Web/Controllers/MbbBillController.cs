using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

[Authorize]
public class MbbBillController : Controller
{
    private readonly ApiClient _api;
    public MbbBillController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewBag.ActiveMenu = "MbbBills";
        var result = await _api.GetMbbBillsAsync();
        return View(result?.Data ?? new());
    }

    public IActionResult Create()
    {
        ViewBag.ActiveMenu = "MbbBills";
        return View(new CreateMbbBillRequest());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateMbbBillRequest req)
    {
        ViewBag.ActiveMenu = "MbbBills";
        if (string.IsNullOrWhiteSpace(req.BillNumber))
        {
            ModelState.AddModelError("BillNumber", "Bill number is required");
            return View(req);
        }
        var result = await _api.CreateMbbBillAsync(req);
        if (result?.Success == true)
        {
            TempData["Success"] = "MBB bill created";
            return RedirectToAction("Index");
        }
        ModelState.AddModelError("", result?.Message ?? "Failed");
        return View(req);
    }

    public async Task<IActionResult> Details(long id)
    {
        ViewBag.ActiveMenu = "MbbBills";
        var result = await _api.GetMbbBillAsync(id);
        if (result?.Data == null) return NotFound();
        return View(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Pay(long id, decimal amount, string mode)
    {
        var result = await _api.PayMbbBillAsync(id, amount, mode);
        if (result?.Success == true)
            TempData["Success"] = "Payment recorded";
        else
            TempData["Error"] = result?.Message ?? "Failed";
        return RedirectToAction("Details", new { id });
    }
}
