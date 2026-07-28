using System.Security.Claims;

namespace BloodCenterOS.Web.Services;

public class JwtCookieMiddleware
{
    private readonly RequestDelegate _next;

    public JwtCookieMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, ITokenStore tokenStore)
    {
        if (context.Request.Cookies.TryGetValue("bc_token", out var token) && !string.IsNullOrEmpty(token))
        {
            if (!tokenStore.IsAuthenticated)
            {
                var name = context.Request.Cookies["bc_name"] ?? "";
                var userId = long.TryParse(context.Request.Cookies["bc_userid"], out var uid) ? uid : 0;
                var role = context.Request.Cookies["bc_role"] ?? "";
                tokenStore.Set(token, name, userId, role);
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, tokenStore.DisplayName ?? ""),
                new(ClaimTypes.NameIdentifier, tokenStore.UserId.ToString()),
                new(ClaimTypes.Role, tokenStore.Role ?? "")
            };
            var identity = new ClaimsIdentity(claims, "jwt-cookie");
            context.User = new ClaimsPrincipal(identity);
        }

        await _next(context);
    }
}
