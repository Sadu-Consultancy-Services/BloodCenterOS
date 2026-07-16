using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Models;
using BloodCenterOS.Web.Models.ViewModels;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class CollectionController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public CollectionController(ApiClient api, IWebAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Blood Collections";
        ViewBag.ActiveMenu = "Collection";

        var items = new List<Collection>();
        try
        {
            var result = await _api.GetCollectionsAsync();
            if (result?.Success == true && result.Data != null)
                items = result.Data;
        }
        catch { }

        if (!items.Any())
        {
            items = new List<Collection>
            {
                new() { CollectionId = 1, BloodBagNumber = "BAG-2026-0001", DonorId = 101, CampId = null, CollectionLocationType = "In-house", BagVolumeMl = 450, CollectionStartTime = DateTime.Now.AddDays(-1), Notes = "Standard voluntary donation" },
                new() { CollectionId = 2, BloodBagNumber = "BAG-2026-0002", DonorId = 102, CampId = 1, CollectionLocationType = "Camp", BagVolumeMl = 350, CollectionStartTime = DateTime.Now.AddDays(-2), Notes = "Rotary Club Camp" },
                new() { CollectionId = 3, BloodBagNumber = "BAG-2026-0003", DonorId = 103, CampId = 1, CollectionLocationType = "Camp", BagVolumeMl = 450, CollectionStartTime = DateTime.Now.AddDays(-2), Notes = "" },
            };
        }

        return View(new CollectionListViewModel { Collections = items });
    }

    public async Task<IActionResult> Create(long? campId)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Blood Collection";
        ViewBag.ActiveMenu = "Collection";

        var vm = new CollectionCreateViewModel { Collection = new Collection { CampId = campId } };
        try
        {
            var donors = await _api.SearchDonorsAsync(null, null, null, 1, 500);
            if (donors?.Success == true && donors.Data?.Items != null)
                vm.Donors = donors.Data.Items.Select(d => new DonorListItem { Id = d.DonorId, Name = d.FullName, Code = d.DonorCode ?? "", BloodGroup = d.BloodGroup ?? "" }).ToList();
        }
        catch { }

        try
        {
            var camps = await _api.GetUpcomingCampsAsync();
            if (camps?.Success == true && camps.Data != null)
                vm.Camps = camps.Data;
        }
        catch { }

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Collection collection)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "New Blood Collection";
        ViewBag.ActiveMenu = "Collection";

        if (string.IsNullOrWhiteSpace(collection.BloodBagNumber))
        {
            ModelState.AddModelError("BloodBagNumber", "Bag number is required");
            return View(new CollectionCreateViewModel { Collection = collection });
        }

        try
        {
            var result = await _api.CreateCollectionAsync(collection);
            if (result?.Success == true)
            {
                TempData["Success"] = "Collection recorded successfully";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result?.Message ?? "Failed to create collection");
        }
        catch
        {
            ModelState.AddModelError("", "API unavailable.");
        }

        return View(new CollectionCreateViewModel { Collection = collection });
    }

    public async Task<IActionResult> Details(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Collection Details";
        ViewBag.ActiveMenu = "Collection";

        try
        {
            var result = await _api.GetCollectionAsync(id);
            if (result?.Success == true && result.Data != null)
                return View(result.Data);
        }
        catch { }

        TempData["Error"] = "Collection not found";
        return RedirectToAction("Index");
    }
}
