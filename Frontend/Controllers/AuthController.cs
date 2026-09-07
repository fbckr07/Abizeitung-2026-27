using System.Security.Claims;
using Frontend.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Frontend.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting(AuthRateLimiting.LoginPolicy)]
    public async Task<IActionResult> Login([FromForm] string code, [FromForm] string? returnUrl = null)
    {
        var student = await authService.ValidateCodeAsync(code, HttpContext.RequestAborted);

        if (student is null)
        {
            return Redirect(AuthRateLimiting.BuildLoginRedirect("/login", returnUrl, "invalid"));
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

        return Redirect(AuthRateLimiting.SanitizeReturnUrl(returnUrl) ?? "/");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/login");
    }
}
