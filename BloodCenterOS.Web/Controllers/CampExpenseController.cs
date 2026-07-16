using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class CampExpenseController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;
    public CampExpenseController(ApiClient api, IWebAuthService auth) { _api = api; _auth = auth; }

    public async Task<IActionResult> Index(long? campId)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Camp Expenses";
        ViewBag.ActiveMenu = "CampExpenses";
        ViewBag.CampId = campId;
        var items = new List<CampExpense>();
        try { var r = await _api.GetCampExpensesAsync(campId); if (r?.Success == true && r.Data != null) items = r.Data; } catch { }
        return View(items);
    }

    public IActionResult Create(long? campId)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Camp Expense";
        ViewBag.ActiveMenu = "CampExpenses";
        ViewBag.CampId = campId;
        return View(new CampExpense { CampId = campId ?? 0 });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CampExpense item)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Camp Expense";
        ViewBag.ActiveMenu = "CampExpenses";
        try
        {
            var r = await _api.CreateCampExpenseAsync(new
            {
                campId = item.CampId,
                expenseCategory = item.ExpenseCategory,
                amount = item.Amount,
                notes = item.Notes
            });
            if (r?.Success == true) { TempData["Success"] = "Expense recorded"; return RedirectToAction("Index", new { campId = item.CampId }); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(item);
    }

    public async Task<IActionResult> Edit(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Camp Expense";
        ViewBag.ActiveMenu = "CampExpenses";
        try { var r = await _api.GetCampExpensesAsync(); if (r?.Success == true && r.Data != null) { var it = r.Data.FirstOrDefault(x => x.CampExpenseId == id); if (it != null) return View(it); } } catch { }
        TempData["Error"] = "Expense not found";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(long id, CampExpense item)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Edit Camp Expense";
        ViewBag.ActiveMenu = "CampExpenses";
        try
        {
            var r = await _api.UpdateCampExpenseAsync(id, new
            {
                expenseCategory = item.ExpenseCategory,
                amount = item.Amount,
                notes = item.Notes
            });
            if (r?.Success == true) { TempData["Success"] = "Expense updated"; return RedirectToAction("Index", new { campId = item.CampId }); }
            ModelState.AddModelError("", r?.Message ?? "Failed");
        }
        catch { ModelState.AddModelError("", "API unavailable"); }
        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(long id, long? campId)
    {
        try { await _api.DeleteCampExpenseAsync(id); TempData["Success"] = "Expense deleted"; }
        catch { TempData["Error"] = "Failed"; }
        return RedirectToAction("Index", new { campId });
    }
}
