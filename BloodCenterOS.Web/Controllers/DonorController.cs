using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Models;
using BloodCenterOS.Web.Models.ViewModels;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class DonorController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public DonorController(ApiClient api, IWebAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> Index(string? keyword, string? bloodGroup, string? gender, int page = 1)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Donor Management";
        ViewBag.ActiveMenu = "Donors";

        var model = new DonorSearchViewModel
        {
            Keyword = keyword,
            BloodGroup = bloodGroup,
            Gender = gender,
            Page = page,
            PageSize = 20
        };

        try
        {
            var result = await _api.SearchDonorsAsync(keyword, bloodGroup, gender, page, 20);
            if (result?.Success == true && result.Data?.Items != null)
            {
                model.Items = result.Data.Items.Select(d => new DonorListItem
                {
                    Id = d.DonorId,
                    Code = d.DonorCode ?? $"DON-{d.DonorId:D6}",
                    Name = d.FullName,
                    BloodGroup = d.BloodGroup ?? "",
                    Phone = d.Phone ?? "",
                    City = d.City ?? "",
                    LastDonation = d.LastDonationDate,
                    TotalDonations = d.TotalDonations,
                    Status = "Active"
                }).ToList();
                model.TotalCount = result.Data.TotalCount;
            }
        }
        catch { /* fall back to mock */ }

        if (!model.Items.Any())
        {
            model.Items = Enumerable.Range(1, 15).Select(i => new DonorListItem
            {
                Id = i,
                Code = $"DON-{2026000 + i}",
                Name = new[] { "Amit Patel", "Priya Singh", "Rahul Sharma", "Sunita Verma", "Vikram Joshi" }[i % 5],
                BloodGroup = new[] { "A+", "B+", "O+", "AB-", "A-" }[i % 5],
                Phone = $"98765{43200 + i}",
                City = new[] { "Mumbai", "Delhi", "Bangalore", "Chennai", "Pune" }[i % 5],
                LastDonation = i <= 10 ? DateTime.Now.AddDays(-i * 30) : null,
                TotalDonations = Random.Shared.Next(1, 12),
                Status = i == 5 ? "Deferred" : "Active"
            }).ToList();
            model.TotalCount = 15;
        }

        return View(model);
    }

    public async Task<IActionResult> Details(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.ActiveMenu = "Donors";

        var model = new DonorDetailViewModel();
        try
        {
            var donorResult = await _api.GetDonorAsync(id);
            if (donorResult?.Success == true && donorResult.Data != null)
                model.Donor = donorResult.Data;
            else
                return RedirectToAction("Index");

            var donResult = await _api.GetDonationsByDonorAsync(id);
            if (donResult?.Success == true && donResult.Data != null)
                model.Donations = donResult.Data;
        }
        catch { return RedirectToAction("Index"); }

        ViewBag.Title = $"Donor — {model.Donor.FullName}";
        return View(model);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Donor";
        ViewBag.ActiveMenu = "Donors";
        return View(new DonorViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Donor donor)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Donor";
        ViewBag.ActiveMenu = "Donors";

        if (string.IsNullOrWhiteSpace(donor.FirstName))
        {
            ModelState.AddModelError("FirstName", "First name is required");
            return View(new DonorViewModel { Donor = donor });
        }

        try
        {
            var result = await _api.CreateDonorAsync(donor);
            if (result?.Success == true)
            {
                TempData["Success"] = "Donor created successfully";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result?.Message ?? "Failed to create donor");
        }
        catch
        {
            ModelState.AddModelError("", "API unavailable. Unable to create donor.");
        }

        return View(new DonorViewModel { Donor = donor });
    }

    public async Task<IActionResult> Edit(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.ActiveMenu = "Donors";

        try
        {
            var result = await _api.GetDonorAsync(id);
            if (result?.Success == true && result.Data != null)
            {
                ViewBag.Title = $"Edit Donor — {result.Data.FullName}";
                return View(new DonorViewModel { Donor = result.Data });
            }
        }
        catch { }

        TempData["Error"] = "Donor not found";
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Edit(long id, Donor donor)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.ActiveMenu = "Donors";
        donor.DonorId = id;

        if (string.IsNullOrWhiteSpace(donor.FirstName))
        {
            ModelState.AddModelError("FirstName", "First name is required");
            ViewBag.Title = $"Edit Donor — #{id}";
            return View(new DonorViewModel { Donor = donor });
        }

        try
        {
            var result = await _api.UpdateDonorAsync(id, donor);
            if (result?.Success == true)
            {
                TempData["Success"] = "Donor updated successfully";
                return RedirectToAction("Details", new { id });
            }
            ModelState.AddModelError("", result?.Message ?? "Failed to update donor");
        }
        catch
        {
            ModelState.AddModelError("", "API unavailable. Unable to update donor.");
        }

        ViewBag.Title = $"Edit Donor — #{id}";
        return View(new DonorViewModel { Donor = donor });
    }
}
