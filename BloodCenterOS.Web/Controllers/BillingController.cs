using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class BillingController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public BillingController(ApiClient api, IWebAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Billing";
        ViewBag.ActiveMenu = "Billing";

        var items = new List<Billing>();
        try
        {
            var result = await _api.GetBillingsAsync();
            if (result?.Success == true && result.Data != null)
                items = result.Data;
        }
        catch { }

        if (!items.Any())
        {
            items = new List<Billing>
            {
                new() { BillingTransactionId = 1, InvoiceNumber = "INV-2026-001", PatientId = 1, TotalAmount = 3500, TaxAmount = 315, Discount = 0, PaymentStatus = "Paid", PaymentMode = "Cash", InvoiceDate = DateTime.Now.AddDays(-5) },
                new() { BillingTransactionId = 2, InvoiceNumber = "INV-2026-002", PatientId = 2, TotalAmount = 5200, TaxAmount = 468, Discount = 500, PaymentStatus = "Partial", PaymentMode = "Card", InvoiceDate = DateTime.Now.AddDays(-3) },
                new() { BillingTransactionId = 3, InvoiceNumber = "INV-2026-003", PatientId = 3, TotalAmount = 1800, TaxAmount = 162, Discount = 0, PaymentStatus = "Unpaid", PaymentMode = "", InvoiceDate = DateTime.Now.AddDays(-1) },
            };
        }

        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Billing";
        ViewBag.ActiveMenu = "Billing";
        return View(new Billing());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Billing billing)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Billing";
        ViewBag.ActiveMenu = "Billing";

        if (string.IsNullOrWhiteSpace(billing.InvoiceNumber))
        {
            ModelState.AddModelError("InvoiceNumber", "Invoice number is required");
            return View(billing);
        }

        try
        {
            var result = await _api.CreateBillingAsync(billing);
            if (result?.Success == true)
            {
                TempData["Success"] = "Billing created successfully";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result?.Message ?? "Failed to create billing");
        }
        catch { ModelState.AddModelError("", "API unavailable."); }

        return View(billing);
    }

    public async Task<IActionResult> Payment(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Payment";
        ViewBag.ActiveMenu = "Billing";
        ViewBag.BillingId = id;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Payment(long id, decimal amount, string mode, string? reference)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");

        try
        {
            var result = await _api.AddPaymentAsync(id, amount, mode, reference);
            if (result?.Success == true)
            {
                TempData["Success"] = "Payment recorded successfully";
                return RedirectToAction("Index");
            }
            TempData["Error"] = result?.Message ?? "Failed to record payment";
        }
        catch { TempData["Error"] = "API unavailable."; }

        return RedirectToAction("Payment", new { id });
    }
}
