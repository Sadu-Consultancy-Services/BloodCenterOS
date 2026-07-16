using BloodCenterOS.Web.Models.ViewModels;

namespace BloodCenterOS.Web.Models;

public class UserSearchResult
{
    public List<UserListItem> Items { get; set; } = new();
    public long TotalCount { get; set; }
}

public class UserSearchItem
{
    public long UserId { get; set; }
    public string UserName { get; set; } = "";
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class UserDetailResult
{
    public long UserId { get; set; }
    public long? CenterId { get; set; }
    public string UserName { get; set; } = "";
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<RoleInfo>? Roles { get; set; }
}

public class RoleInfo
{
    public long RoleId { get; set; }
    public string RoleName { get; set; } = "";
}

public class RoleItemResult
{
    public long RoleId { get; set; }
    public string RoleName { get; set; } = "";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PermissionItemResult
{
    public long PermissionId { get; set; }
    public string PermissionCode { get; set; } = "";
    public string? Description { get; set; }
}

public class AssignedPermissionResult
{
    public long PermissionId { get; set; }
    public string PermissionCode { get; set; } = "";
}
public class SetConfigRequest
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}
