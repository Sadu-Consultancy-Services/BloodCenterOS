namespace BloodCenterOS.Core.Models;

public class Permission
{
    public long PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string? Description { get; set; }
}
