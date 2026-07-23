using Microsoft.JSInterop;

namespace BloodCenterOS.Web.Services;

public interface ITokenStore
{
    string? Token { get; }
    string? DisplayName { get; }
    long UserId { get; }
    string? Role { get; }
    long LoginHistoryId { get; }
    bool IsAuthenticated { get; }
    void Set(string token, string displayName, long userId, string role, long loginHistoryId = 0);
    void Clear();
}

public class TokenStore : ITokenStore
{
    private string? _token;
    private string? _displayName;
    private long _userId;
    private string? _role;
    private long _loginHistoryId;

    public string? Token => _token;
    public string? DisplayName => _displayName;
    public long UserId => _userId;
    public string? Role => _role;
    public long LoginHistoryId => _loginHistoryId;
    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);

    public void Set(string token, string displayName, long userId, string role, long loginHistoryId = 0)
    {
        _token = token;
        _displayName = displayName;
        _userId = userId;
        _role = role;
        _loginHistoryId = loginHistoryId;
    }

    public void Clear()
    {
        _token = null;
        _displayName = null;
        _userId = 0;
        _role = null;
        _loginHistoryId = 0;
    }
}
