using BloodCenterOS.Web.Models.ViewModels;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class BloodGroupController : Controller
{
    private readonly IWebAuthService _auth;

    public BloodGroupController(IWebAuthService auth)
    {
        _auth = auth;
    }

    public IActionResult Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Blood Groups";
        ViewBag.ActiveMenu = "BloodGroups";

        var groups = new List<BloodGroupItem>
        {
            new() { Code = "A+",  Description = "A Positive",  CanDonateTo = "A+, AB+", CanReceiveFrom = "A+, A-, O+, O-" },
            new() { Code = "A-",  Description = "A Negative",  CanDonateTo = "A+, A-, AB+, AB-", CanReceiveFrom = "A-, O-" },
            new() { Code = "B+",  Description = "B Positive",  CanDonateTo = "B+, AB+", CanReceiveFrom = "B+, B-, O+, O-" },
            new() { Code = "B-",  Description = "B Negative",  CanDonateTo = "B+, B-, AB+, AB-", CanReceiveFrom = "B-, O-" },
            new() { Code = "AB+", Description = "AB Positive", CanDonateTo = "AB+", CanReceiveFrom = "All Blood Groups" },
            new() { Code = "AB-", Description = "AB Negative", CanDonateTo = "AB+, AB-", CanReceiveFrom = "A-, B-, AB-, O-" },
            new() { Code = "O+",  Description = "O Positive",  CanDonateTo = "A+, B+, AB+, O+", CanReceiveFrom = "O+, O-" },
            new() { Code = "O-",  Description = "O Negative",  CanDonateTo = "All Blood Groups", CanReceiveFrom = "O-" }
        };

        return View(groups);
    }
}


