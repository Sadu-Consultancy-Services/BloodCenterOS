using BloodCenterOS.Web.Models.ViewModels;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class ComponentTypeController : Controller
{
    private readonly IWebAuthService _auth;

    public ComponentTypeController(IWebAuthService auth)
    {
        _auth = auth;
    }

    public IActionResult Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Component Types";
        ViewBag.ActiveMenu = "ComponentTypes";

        var types = new List<ComponentTypeItem>
        {
            new() { Code = "Whole Blood", Name = "Whole Blood", Description = "Unseparated blood containing all components", ShelfLife = "35 days" },
            new() { Code = "PRBC", Name = "Packed Red Blood Cells", Description = "Red cells for anemia and blood loss", ShelfLife = "42 days" },
            new() { Code = "FFP", Name = "Fresh Frozen Plasma", Description = "Plasma for clotting factor deficiencies", ShelfLife = "12 months" },
            new() { Code = "Platelet", Name = "Platelet Concentrate", Description = "Platelets for thrombocytopenia", ShelfLife = "5 days" },
            new() { Code = "Cryo", Name = "Cryoprecipitate", Description = "Clotting factors for hemophilia", ShelfLife = "12 months" }
        };

        return View(types);
    }
}
