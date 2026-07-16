using System.Security.Claims;
using BloodCenterOS.Core.Models;
using BloodCenterOS.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/roles")]
public class RoleController : ControllerBase
{
    private readonly IRoleRepository _roleRepo;
    private readonly IPermissionRepository _permRepo;

    public RoleController(IRoleRepository roleRepo, IPermissionRepository permRepo)
    {
        _roleRepo = roleRepo;
        _permRepo = permRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var centerId = GetCenterId();
        var roles = await _roleRepo.GetAllAsync(centerId);
        return Ok(ApiResponse<IEnumerable<Role>>.Ok(roles));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoleRequest request)
    {
        var centerId = GetCenterId();
        var userId = GetUserId();
        var id = await _roleRepo.CreateAsync(centerId, request.RoleName, request.Description, userId);
        return Ok(ApiResponse<long>.Ok(id, "Role created"));
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> GetAllPermissions()
    {
        var perms = await _permRepo.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<Permission>>.Ok(perms));
    }

    [HttpGet("{id}/permissions")]
    public async Task<IActionResult> GetPermissions(long id)
    {
        var centerId = GetCenterId();
        var perms = await _roleRepo.GetPermissionsAsync(id, centerId);
        return Ok(ApiResponse<IEnumerable<RolePermission>>.Ok(perms));
    }

    [HttpPost("{id}/permissions")]
    public async Task<IActionResult> AssignPermission(long id, [FromBody] AssignPermissionRequest request)
    {
        var centerId = GetCenterId();
        var userId = GetUserId();
        await _roleRepo.AssignPermissionAsync(id, request.PermissionId, centerId, userId);
        return Ok(ApiResponse<object>.Ok(new { }, "Permission assigned"));
    }

    [HttpDelete("{id}/permissions/{permissionId}")]
    public async Task<IActionResult> RemovePermission(long id, long permissionId)
    {
        var centerId = GetCenterId();
        await _roleRepo.RemovePermissionAsync(id, permissionId, centerId);
        return Ok(ApiResponse<object>.Ok(new { }, "Permission removed"));
    }

    private long GetCenterId()
    {
        var claim = User.FindFirst("CenterId")?.Value;
        return long.TryParse(claim, out var id) ? id : 0;
    }

    private long GetUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(claim, out var id) ? id : 0;
    }
}

public class CreateRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class AssignPermissionRequest
{
    public long PermissionId { get; set; }
}
