using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

[Authorize]
public class StoreInventoryController : Controller
{
    private readonly ApiClient _api;
    public StoreInventoryController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewBag.ActiveMenu = "StoreInventory";
        var items = await _api.GetStoreItemsAsync();
        var summary = await _api.GetStoreStockSummaryAsync();
        ViewBag.Summary = summary?.Data ?? new();
        return View(items?.Data ?? new());
    }

    public IActionResult CreateItem() => View(new InvItem());

    [HttpPost]
    public async Task<IActionResult> CreateItem(InvItem item)
    {
        var result = await _api.UpsertStoreItemAsync(item);
        if (result?.Success == true) { TempData["Success"] = "Item created"; return RedirectToAction("Index"); }
        TempData["Error"] = result?.Message ?? "Failed";
        return View(item);
    }

    public async Task<IActionResult> EditItem(long id)
    {
        var result = await _api.GetStoreItemAsync(id);
        if (result?.Data == null) return NotFound();
        return View(result.Data);
    }

    [HttpPost]
    public async Task<IActionResult> EditItem(InvItem item)
    {
        var result = await _api.UpsertStoreItemAsync(item);
        if (result?.Success == true) { TempData["Success"] = "Item updated"; return RedirectToAction("Index"); }
        TempData["Error"] = result?.Message ?? "Failed";
        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteItem(long id)
    {
        await _api.DeleteStoreItemAsync(id);
        TempData["Success"] = "Item deactivated";
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Inward()
    {
        ViewBag.ActiveMenu = "StoreInventory";
        var items = await _api.GetActiveStoreItemsAsync();
        ViewBag.Items = items?.Data ?? new();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Inward(InwardRequest req)
    {
        var result = await _api.InwardStockAsync(req);
        if (result?.Success == true) { TempData["Success"] = "Stock received"; return RedirectToAction("Index"); }
        TempData["Error"] = result?.Message ?? "Failed";
        var items = await _api.GetActiveStoreItemsAsync();
        ViewBag.Items = items?.Data ?? new();
        return View(req);
    }

    public async Task<IActionResult> Outward()
    {
        ViewBag.ActiveMenu = "StoreInventory";
        var items = await _api.GetActiveStoreItemsAsync();
        ViewBag.Items = items?.Data ?? new();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Outward(OutwardRequest req)
    {
        var result = await _api.OutwardStockAsync(req);
        if (result?.Success == true) { TempData["Success"] = "Stock issued"; return RedirectToAction("Index"); }
        TempData["Error"] = result?.Message ?? "Failed";
        var items = await _api.GetActiveStoreItemsAsync();
        ViewBag.Items = items?.Data ?? new();
        return View(req);
    }

    public async Task<IActionResult> Transactions(long itemId, DateTime? from, DateTime? to)
    {
        ViewBag.ActiveMenu = "StoreInventory";
        var txns = await _api.GetStoreTransactionsAsync(itemId, from, to);
        var items = await _api.GetStoreItemsAsync();
        ViewBag.Items = items?.Data ?? new();
        ViewBag.SelectedItemId = itemId;
        ViewBag.From = from;
        ViewBag.To = to;
        return View(txns?.Data ?? new());
    }
}
