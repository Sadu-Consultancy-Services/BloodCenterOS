namespace BloodCenterOS.Core.Models;

public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string Role { get; set; } = string.Empty;
    public long LoginHistoryId { get; set; }
}
