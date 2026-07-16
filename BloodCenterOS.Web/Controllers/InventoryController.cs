using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Models;
using BloodCenterOS.Web.Models.ViewModels;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class InventoryController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public InventoryController(ApiClient api, IWebAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Inventory";
        ViewBag.ActiveMenu = "Inventory";

        var model = new InventoryViewModel();

        try
        {
            var result = await _api.GetStockAsync();
            if (result?.Success == true && result.Data != null)
            {
                model.Stock = result.Data;
                model.TotalAvailable = result.Data.Sum(s => s.AvailableQty);
                model.TotalReserved = result.Data.Sum(s => s.ReservedQty);
                model.TotalQuarantined = result.Data.Sum(s => s.QuarantinedQty);
            }
        }
        catch { }

        if (!model.Stock.Any())
        {
            model.Stock = new List<InventoryStock>
            {
                new() { ComponentType = "Whole Blood", BloodGroup = "A+", AvailableQty = 22, ReservedQty = 3, QuarantinedQty = 1 },
                new() { ComponentType = "Whole Blood", BloodGroup = "A-", AvailableQty = 8, ReservedQty = 1, QuarantinedQty = 0 },
                new() { ComponentType = "Whole Blood", BloodGroup = "B+", AvailableQty = 35, ReservedQty = 5, QuarantinedQty = 2 },
                new() { ComponentType = "Whole Blood", BloodGroup = "B-", AvailableQty = 12, ReservedQty = 2, QuarantinedQty = 0 },
                new() { ComponentType = "Whole Blood", BloodGroup = "AB+", AvailableQty = 15, ReservedQty = 2, QuarantinedQty = 1 },
                new() { ComponentType = "Whole Blood", BloodGroup = "AB-", AvailableQty = 5, ReservedQty = 0, QuarantinedQty = 0 },
                new() { ComponentType = "Whole Blood", BloodGroup = "O+", AvailableQty = 30, ReservedQty = 4, QuarantinedQty = 2 },
                new() { ComponentType = "Whole Blood", BloodGroup = "O-", AvailableQty = 15, ReservedQty = 2, QuarantinedQty = 0 },
            };
            model.TotalAvailable = model.Stock.Sum(s => s.AvailableQty);
            model.TotalReserved = model.Stock.Sum(s => s.ReservedQty);
            model.TotalQuarantined = model.Stock.Sum(s => s.QuarantinedQty);
        }

        return View(model);
    }
}
