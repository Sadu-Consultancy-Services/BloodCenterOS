using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

[Authorize]
public class BillingController : Controller
{
    private readonly ApiClient _api;
    public BillingController(ApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        ViewBag.ActiveMenu = "Billing";
        var result = await _api.GetBillingsAsync();
        return View(result?.Data ?? new());
    }

    public IActionResult Create()
    {
        ViewBag.ActiveMenu = "Billing";
        return View(new Billing());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Billing billing)
    {
        ViewBag.ActiveMenu = "Billing";
        if (string.IsNullOrWhiteSpace(billing.InvoiceNumber))
        {
            ModelState.AddModelError("InvoiceNumber", "Invoice number is required");
            return View(billing);
        }
        var result = await _api.CreateBillingAsync(billing);
        if (result?.Success == true)
        {
            TempData["Success"] = "Invoice created";
            return RedirectToAction("Index");
        }
        ModelState.AddModelError("", result?.Message ?? "Failed");
        return View(billing);
    }

    public async Task<IActionResult> Details(long id)
    {
        ViewBag.ActiveMenu = "Billing";
        var result = await _api.GetInvoiceAsync(id);
        if (result?.Data == null) return NotFound();
        return View(result.Data);
    }

    public async Task<IActionResult> Payment(long id)
    {
        ViewBag.ActiveMenu = "Billing";
        var inv = await _api.GetInvoiceAsync(id);
        ViewBag.Invoice = inv?.Data;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Payment(long id, decimal amount, string mode, string? reference)
    {
        var result = await _api.AddPaymentAsync(id, amount, mode, reference);
        if (result?.Success == true)
        {
            TempData["Success"] = "Payment recorded";
            return RedirectToAction("Details", new { id });
        }
        TempData["Error"] = result?.Message ?? "Failed";
        return RedirectToAction("Payment", new { id });
    }

    public async Task<IActionResult> Dues(string? keyword)
    {
        ViewBag.ActiveMenu = "Billing";
        var result = await _api.GetDuesAsync(keyword);
        ViewBag.Keyword = keyword;
        return View(result?.Data ?? new());
    }

    public IActionResult CreditNote(long originalInvoiceId)
    {
        ViewBag.ActiveMenu = "Billing";
        ViewBag.OriginalInvoiceId = originalInvoiceId;
        return View(new CreditNoteRequest());
    }

    [HttpPost]
    public async Task<IActionResult> CreditNote(CreditNoteRequest req)
    {
        var result = await _api.CreateCreditNoteAsync(req);
        if (result?.Success == true)
        {
            TempData["Success"] = "Credit note created";
            return RedirectToAction("Index");
        }
        TempData["Error"] = result?.Message ?? "Failed";
        return View(req);
    }
}
