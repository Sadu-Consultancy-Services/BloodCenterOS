using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

[Authorize]
public class IssueStorageController : Controller
{
    private readonly ApiClient _api;
    public IssueStorageController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index(long? storageId, DateTime? from, DateTime? to)
    {
        ViewBag.ActiveMenu = "IssueStorage";
        var records = await _api.GetIssueStorageRecordsAsync(storageId, from, to);
        var invoices = await _api.GetIssueStorageInvoicesAsync(storageId, from, to);
        var storages = await _api.GetStoragesAsync();
        ViewBag.Invoices = invoices?.Data ?? new();
        ViewBag.Storages = storages?.Data ?? new();
        ViewBag.StorageId = storageId;
        ViewBag.From = from;
        ViewBag.To = to;
        return View(records?.Data ?? new());
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.ActiveMenu = "IssueStorage";
        var components = await _api.GetAvailableComponentsForStorageAsync();
        var storages = await _api.GetStoragesAsync();
        ViewBag.AvailableComponents = components?.Data ?? new();
        ViewBag.Storages = storages?.Data ?? new();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(IssueToStorageRequest req)
    {
        var result = await _api.CreateIssueToStorageAsync(req);
        if (result?.Success == true)
        {
            TempData["Success"] = $"Issued to storage. Invoice #{result.Data}";
            return RedirectToAction("Index");
        }
        TempData["Error"] = result?.Message ?? "Failed";
        return RedirectToAction("Create");
    }
}
