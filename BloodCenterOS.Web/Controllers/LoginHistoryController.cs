using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class LoginHistoryController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public LoginHistoryController(ApiClient api, IWebAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> Index(long? userId, DateTime? fromDate, DateTime? toDate, int limit = 200)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Login History";
        ViewBag.ActiveMenu = "LoginHistory";
        ViewBag.FilterUserId = userId;
        ViewBag.FromDate = fromDate;
        ViewBag.ToDate = toDate;
        ViewBag.Limit = limit;

        try
        {
            var resp = await _api.GetLoginHistoryAsync(userId, fromDate, toDate, limit);
            return View(resp?.Data ?? new List<LoginHistory>());
        }
        catch
        {
            return View(new List<LoginHistory>());
        }
    }
}
