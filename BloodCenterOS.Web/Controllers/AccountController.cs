using BloodCenterOS.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace BloodCenterOS.Web.Controllers;

public class AccountController : Controller
{
    private readonly IWebAuthService _authService;
    private readonly ITokenStore _tokenStore;

    public AccountController(IWebAuthService authService, ITokenStore tokenStore)
    {
        _authService = authService;
        _tokenStore = tokenStore;
    }

    public IActionResult Login()
    {
        if (_authService.IsAuthenticated)
            return RedirectToAction("Index", "Home");
        ViewBag.Title = "Login";
        ViewData["HideLayout"] = true;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        var ok = await _authService.LoginAsync(username, password);
        if (ok)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddHours(8)
            };
            Response.Cookies.Append("bc_token", _tokenStore.Token!, cookieOptions);
            Response.Cookies.Append("bc_name", _tokenStore.DisplayName ?? "", cookieOptions);
            Response.Cookies.Append("bc_userid", _tokenStore.UserId.ToString(), cookieOptions);
            Response.Cookies.Append("bc_role", _tokenStore.Role ?? "", cookieOptions);
            return RedirectToAction("Index", "Home");
        }
        ViewBag.Error = "Invalid username or password";
        ViewBag.Title = "Login";
        ViewData["HideLayout"] = true;
        return View();
    }

    public IActionResult Logout()
    {
        _authService.Logout();
        foreach (var key in new[] { "bc_token", "bc_name", "bc_userid", "bc_role" })
            Response.Cookies.Delete(key);
        return RedirectToAction("Login");
    }

    public IActionResult ForgotPassword()
    {
        ViewBag.Title = "Forgot Password";
        ViewData["HideLayout"] = true;
        return View();
    }
}
