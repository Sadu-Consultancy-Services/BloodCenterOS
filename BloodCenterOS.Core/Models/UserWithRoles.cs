namespace BloodCenterOS.Core.Models;

public class UserWithRoles : User
{
    public List<string> Roles { get; set; } = new();
}
