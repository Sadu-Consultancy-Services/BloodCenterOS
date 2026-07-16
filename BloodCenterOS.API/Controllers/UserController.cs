using System.Security.Claims;
using BloodCenterOS.Core.Models;
using BloodCenterOS.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.API.Controllers;

[ApiController]
[Authorize]
[Route("api/users")]
public class UserController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly IRoleRepository _roleRepo;

    public UserController(IUserRepository userRepo, IRoleRepository roleRepo)
    {
        _userRepo = userRepo;
        _roleRepo = roleRepo;
    }

    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] string? keyword, [FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        var centerId = GetCenterId();
        var results = await _userRepo.SearchAsync(centerId, keyword, page, size);
        var list = results.Select(r => new
        {
            UserId = (long)r.userid,
            UserName = (string)r.username,
            DisplayName = (string?)r.displayname,
            Email = (string?)r.email,
            Phone = (string?)r.phone,
            IsLocked = (bool)r.islocked,
            LastLoginAt = (DateTime?)r.lastloginat,
            CreatedAt = (DateTime)r.createdat
        }).ToList();
        var totalCount = results.Any() ? (long)results.First().totalcount : 0;
        return Ok(ApiResponse<object>.Ok(new { items = list, totalCount }));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user is null)
            return NotFound(ApiResponse<object>.Fail("User not found"));
        var roles = await _userRepo.GetRolesAsync(id);
        return Ok(ApiResponse<object>.Ok(new
        {
            user.UserId,
            user.CenterId,
            user.UserName,
            user.DisplayName,
            user.Email,
            user.Phone,
            user.IsLocked,
            user.LastLoginAt,
            user.CreatedAt,
            Roles = roles.Select(r => new { r.RoleId, r.RoleName })
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
    {
        var centerId = GetCenterId();
        var userId = GetUserId();
        var hash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new User
        {
            CenterId = centerId,
            UserName = request.UserName,
            DisplayName = request.DisplayName,
            Email = request.Email,
            Phone = request.Phone,
            PasswordHash = hash,
            PasswordSalt = string.Empty,
            CreatedBy = userId
        };

        var id = await _userRepo.CreateAsync(user);
        return Ok(ApiResponse<long>.Ok(id, "User created"));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateUserRequest request)
    {
        var userId = GetUserId();
        await _userRepo.UpdateAsync(id, request.DisplayName, request.Email, request.Phone, userId);
        return Ok(ApiResponse<object>.Ok(new { }, "User updated"));
    }

    [HttpPut("{id}/lock")]
    public async Task<IActionResult> ToggleLock(long id, [FromBody] ToggleLockRequest request)
    {
        await _userRepo.ToggleLockAsync(id, request.Locked);
        var msg = request.Locked ? "User locked" : "User unlocked";
        return Ok(ApiResponse<object>.Ok(new { }, msg));
    }

    [HttpGet("{id}/roles")]
    public async Task<IActionResult> GetRoles(long id)
    {
        var roles = await _userRepo.GetRolesAsync(id);
        return Ok(ApiResponse<IEnumerable<Role>>.Ok(roles));
    }

    [HttpPost("{id}/roles")]
    public async Task<IActionResult> AssignRole(long id, [FromBody] AssignUserRoleRequest request)
    {
        var centerId = GetCenterId();
        var userId = GetUserId();
        await _userRepo.AssignRoleAsync(id, request.RoleId, centerId, userId);
        return Ok(ApiResponse<object>.Ok(new { }, "Role assigned"));
    }

    [HttpDelete("{id}/roles/{roleId}")]
    public async Task<IActionResult> RemoveRole(long id, long roleId)
    {
        await _userRepo.RemoveRoleAsync(id, roleId);
        return Ok(ApiResponse<object>.Ok(new { }, "Role removed"));
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

public class CreateUserRequest
{
    public string UserName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UpdateUserRequest
{
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}

public class ToggleLockRequest
{
    public bool Locked { get; set; }
}

public class AssignUserRoleRequest
{
    public long RoleId { get; set; }
}
