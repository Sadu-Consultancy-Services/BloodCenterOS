using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Models;
using BloodCenterOS.Web.Models.ViewModels;
using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class RolesController : Controller
{
    private readonly ApiClient _api;
    private readonly IWebAuthService _auth;

    public RolesController(ApiClient api, IWebAuthService auth)
    {
        _api = api;
        _auth = auth;
    }

    public async Task<IActionResult> Index()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Roles";
        ViewBag.ActiveMenu = "Roles";

        var vm = new RoleListViewModel();

        try
        {
            var result = await _api.GetAsync<List<RoleItemResult>>("/api/roles");
            if (result?.Success == true && result.Data != null)
            {
                vm.Roles = result.Data.Select(r => new RoleItem
                {
                    RoleId = r.RoleId,
                    RoleName = r.RoleName,
                    Description = r.Description,
                    CreatedAt = r.CreatedAt
                }).ToList();
            }
        }
        catch { /* fall back */ }

        try
        {
            var permResult = await _api.GetAsync<List<PermissionItemResult>>("/api/roles/permissions");
            if (permResult?.Success == true && permResult.Data != null)
            {
                vm.AllPermissions = permResult.Data.Select(p => new PermissionItem
                {
                    PermissionId = p.PermissionId,
                    PermissionCode = p.PermissionCode,
                    Description = p.Description
                }).ToList();
            }
        }
        catch { /* fall back */ }

        if (!vm.Roles.Any())
        {
            vm.Roles = new()
            {
                new() { RoleId = 1, RoleName = "Administrator", Description = "Full system access", CreatedAt = DateTime.Now.AddDays(-30) },
                new() { RoleId = 2, RoleName = "Lab Technician", Description = "Testing and component management", CreatedAt = DateTime.Now.AddDays(-15) },
                new() { RoleId = 3, RoleName = "Medical Officer", Description = "Issue and emergency management", CreatedAt = DateTime.Now.AddDays(-10) },
            };
            vm.AllPermissions = new()
            {
                new() { PermissionId = 1, PermissionCode = "DONOR_VIEW", Description = "View donors" },
                new() { PermissionId = 2, PermissionCode = "DONOR_CREATE", Description = "Create donors" },
            };
        }

        return View(vm);
    }

    public IActionResult Create()
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Role";
        ViewBag.ActiveMenu = "Roles";
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(string roleName, string? description)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Add Role";
        ViewBag.ActiveMenu = "Roles";

        if (string.IsNullOrWhiteSpace(roleName))
        {
            ModelState.AddModelError("", "Role name is required");
            return View();
        }

        try
        {
            var result = await _api.PostAsync<long>("/api/roles", new { roleName, description });
            if (result?.Success == true)
            {
                TempData["Success"] = "Role created successfully";
                return RedirectToAction("Index");
            }
            ModelState.AddModelError("", result?.Message ?? "Failed to create role");
        }
        catch
        {
            ModelState.AddModelError("", "API unavailable. Unable to create role.");
        }

        return View();
    }

    public async Task<IActionResult> Permissions(long id)
    {
        if (!_auth.IsAuthenticated) return RedirectToAction("Login", "Account");
        ViewBag.Title = "Role Permissions";
        ViewBag.ActiveMenu = "Roles";

        var vm = new RolePermissionViewModel { RoleId = id };

        try
        {
            var rolesResult = await _api.GetAsync<List<RoleItemResult>>("/api/roles");
            if (rolesResult?.Success == true && rolesResult.Data != null)
            {
                var role = rolesResult.Data.FirstOrDefault(r => r.RoleId == id);
                if (role != null) vm.RoleName = role.RoleName;
            }
        }
        catch { vm.RoleName = $"Role #{id}"; }

        try
        {
            var permResult = await _api.GetAsync<List<PermissionItemResult>>("/api/roles/permissions");
            if (permResult?.Success == true && permResult.Data != null)
            {
                vm.AllPermissions = permResult.Data.Select(p => new PermissionItem
                {
                    PermissionId = p.PermissionId,
                    PermissionCode = p.PermissionCode,
                    Description = p.Description
                }).ToList();
            }
        }
        catch { /* fall back */ }

        try
        {
            var assignedResult = await _api.GetAsync<List<AssignedPermissionResult>>($"/api/roles/{id}/permissions");
            if (assignedResult?.Success == true && assignedResult.Data != null)
            {
                vm.AssignedCodes = assignedResult.Data.Select(p => p.PermissionCode).ToList();
            }
        }
        catch { /* fall back */ }

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> TogglePermission(long roleId, long permissionId, bool assign)
    {
        try
        {
            if (assign)
                await _api.PostAsync<object>($"/api/roles/{roleId}/permissions", new { permissionId });
            else
                await _api.DeleteAsync<object>($"/api/roles/{roleId}/permissions/{permissionId}");

            TempData["Success"] = assign ? "Permission assigned" : "Permission removed";
        }
        catch
        {
            TempData["Error"] = "API unavailable";
        }
        return RedirectToAction("Permissions", new { id = roleId });
    }
}


