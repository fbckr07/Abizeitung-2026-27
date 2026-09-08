using System.Security.Claims;
using Frontend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Frontend.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] string code, [FromForm] string? returnUrl = null)
    {
        if (AuthRateLimiting.HasLockoutCookie(HttpContext))
        {
            return Redirect(AuthRateLimiting.BuildLoginRedirect(HttpContext, "/login", returnUrl, "locked"));
        }

        var student = await authService.ValidateCodeAsync(code, HttpContext.RequestAborted);

        if (student is null)
        {
            var locked = AuthRateLimiting.RegisterFailedAttempt(HttpContext);
            return Redirect(AuthRateLimiting.BuildLoginRedirect(
                HttpContext, "/login", returnUrl, locked ? "locked" : "invalid"));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, student.Id.ToString()),
            new(ClaimTypes.Name, student.Name),
            new("Geschlecht", student.Gender.ToString()),
        };

        // TODO: Possible Nullable muss weg
        foreach (var recht in student.Permissions)
        {
            claims.Add(new Claim("Recht", recht));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
                AllowRefresh = true
            });

        AuthRateLimiting.ClearFailedAttempts(HttpContext);
        AuthRateLimiting.ClearRedirectCookie(HttpContext);
        return Redirect(AuthRateLimiting.SanitizeReturnUrl(returnUrl) ?? "/home");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/login");
    }
}
