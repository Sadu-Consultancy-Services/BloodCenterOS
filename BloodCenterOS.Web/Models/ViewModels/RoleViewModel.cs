namespace BloodCenterOS.Web.Models.ViewModels;

public class RoleListViewModel
{
    public List<RoleItem> Roles { get; set; } = new();
    public List<PermissionItem> AllPermissions { get; set; } = new();
}

public class RoleItem
{
    public long RoleId { get; set; }
    public string RoleName { get; set; } = "";
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PermissionItem
{
    public long PermissionId { get; set; }
    public string PermissionCode { get; set; } = "";
    public string? Description { get; set; }
}

public class RolePermissionViewModel
{
    public long RoleId { get; set; }
    public string RoleName { get; set; } = "";
    public List<PermissionItem> AllPermissions { get; set; } = new();
    public List<string> AssignedCodes { get; set; } = new();
}
