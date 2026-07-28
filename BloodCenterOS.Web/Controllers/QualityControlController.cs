using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

[Authorize]
public class QualityControlController : Controller
{
    private readonly ApiClient _api;
    public QualityControlController(ApiClient api) => _api = api;

    private static readonly string[] QcTypes = { "PoolCell", "Anticera", "Saline", "CopperSulphate", "CoombsAHG", "BSA" };

    public async Task<IActionResult> Index(string? type, DateTime? from, DateTime? to)
    {
        ViewBag.ActiveMenu = "QualityControl";
        var result = await _api.GetQcRecordsAsync(type, from, to);
        ViewBag.QcTypes = QcTypes;
        ViewBag.SelectedType = type;
        ViewBag.From = from;
        ViewBag.To = to;
        return View(result?.Data ?? new());
    }

    public IActionResult Create(string type = "PoolCell")
    {
        ViewBag.ActiveMenu = "QualityControl";
        ViewBag.QcTypes = QcTypes;
        ViewBag.SelectedType = type;
        return View(new CreateQcRequest { QCType = type, QCDate = DateTime.Today });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateQcRequest req)
    {
        var result = await _api.CreateQcRecordAsync(req);
        if (result?.Success == true)
        {
            TempData["Success"] = "QC record saved";
            return RedirectToAction("Index", new { type = req.QCType });
        }
        TempData["Error"] = result?.Message ?? "Failed";
        ViewBag.QcTypes = new[] { "PoolCell", "Anticera", "Saline", "CopperSulphate", "CoombsAHG", "BSA" };
        return View(req);
    }

    public async Task<IActionResult> Details(long id)
    {
        ViewBag.ActiveMenu = "QualityControl";
        var result = await _api.GetQcRecordAsync(id);
        if (result?.Data == null) return NotFound();
        return View(result.Data);
    }
}
