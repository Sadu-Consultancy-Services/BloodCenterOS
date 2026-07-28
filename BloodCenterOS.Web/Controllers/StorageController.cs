using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

[Authorize]
public class StorageController : Controller
{
    private readonly ApiClient _api;
    public StorageController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewBag.ActiveMenu = "Storages";
        var result = await _api.GetStoragesAsync();
        return View(result?.Data ?? new());
    }

    public IActionResult Create() => View(new StorageMaster());

    [HttpPost]
    public async Task<IActionResult> Create(StorageMaster item)
    {
        var result = await _api.UpsertStorageAsync(item);
        if (result?.Success == true) { TempData["Success"] = "Storage saved"; return RedirectToAction("Index"); }
        TempData["Error"] = result?.Message ?? "Failed";
        return View(item);
    }

    public async Task<IActionResult> Edit(long id)
    {
        var result = await _api.GetStorageAsync(id);
        if (result?.Data == null) return NotFound();
        return View(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(StorageMaster item)
    {
        var result = await _api.UpsertStorageAsync(item);
        if (result?.Success == true) { TempData["Success"] = "Storage updated"; return RedirectToAction("Index"); }
        TempData["Error"] = result?.Message ?? "Failed";
        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id)
    {
        await _api.DeleteStorageAsync(id);
        TempData["Success"] = "Storage deactivated";
        return RedirectToAction("Index");
    }
}
