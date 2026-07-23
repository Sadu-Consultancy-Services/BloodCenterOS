using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class AuditLogController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public AuditLogController(ApiClient api, IWebAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> Index(string? tableName, int limit = 200)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Audit Logs";
        ViewBag.ActiveMenu = "AuditLogs";
        ViewBag.SelectedTable = tableName;
        ViewBag.Limit = limit;

        try
        {
            var resp = await _api.GetAuditLogsAsync(tableName: tableName, limit: limit);
            return View(resp?.Data ?? new List<AuditLog>());
        }
        catch
        {
            return View(new List<AuditLog>());
        }
    }
}
