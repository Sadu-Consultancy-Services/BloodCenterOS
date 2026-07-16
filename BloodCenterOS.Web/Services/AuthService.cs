using BloodCenterOS.Core.Models;
using BloodCenterOS.Web.Models;

namespace BloodCenterOS.Web.Services;

public interface IWebAuthService
{
    Task<bool> LoginAsync(string username, string password);
    void Logout();
    bool IsAuthenticated { get; }
    string? DisplayName { get; }
    string? Role { get; }
}

public class WebAuthService : IWebAuthService
{
    private readonly ApiClient _api;
    private readonly ITokenStore _tokenStore;

    public WebAuthService(ApiClient api, ITokenStore tokenStore)
    {
        _api = api;
        _tokenStore = tokenStore;
    }

    public bool IsAuthenticated => _tokenStore.IsAuthenticated;
    public string? DisplayName => _tokenStore.DisplayName;
    public string? Role => _tokenStore.Role;

    public async Task<bool> LoginAsync(string username, string password)
    {
        var result = await _api.LoginAsync(new LoginRequest { UserName = username, Password = password });
        if (result is { Success: true, Data: not null })
        {
            _tokenStore.Set(result.Data.Token, result.Data.DisplayName, result.Data.UserId, result.Data.Role);
            return true;
        }
        return false;
    }

    public void Logout()
    {
        _tokenStore.Clear();
    }
}
