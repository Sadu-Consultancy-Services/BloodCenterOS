using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class ExpenseController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public ExpenseController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index(DateTime? fromDate, DateTime? toDate)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Expenses";
        ViewBag.ActiveMenu = "Expenses";
        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;
        var items = new List<Expense>();
        try { var r = await _api.GetExpensesAsync(fromDate, toDate); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Expense";
        ViewBag.ActiveMenu = "Expenses";
        return View(new Expense());
    }

    [HttpPost]
    public async Task<IActionResult> Create(string? category, decimal amount, string? notes)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Expense";
        ViewBag.ActiveMenu = "Expenses";
        try
        {
            var r = await _api.CreateExpenseAsync(new { category, amount, notes });
            if (r?.Success == true) { TempData["Success"] = "Expense created"; return RedirectToAction("Index"); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(new Expense { Category = category, Amount = amount, Notes = notes });
    }
}
