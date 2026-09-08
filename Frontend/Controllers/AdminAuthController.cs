using System.Security.Claims;
using Frontend.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Frontend.Data.Entities;
using Frontend.Services;

namespace Frontend.Controllers;

[ApiController]
[Route("admin/auth")]
public sealed class AdminAuthController(AppDbContext db) : ControllerBase
{
    public const string AdminScheme = "AdminAuth";

    private static readonly PasswordHasher<AdminUser> Hasher = new();

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromForm] string username,
        [FromForm] string password,
        [FromForm] string? returnUrl = null)
    {
        if (AuthRateLimiting.HasAdminLockoutCookie(HttpContext))
        {
            return Redirect(AuthRateLimiting.BuildLoginRedirect(HttpContext, "/admin/login", returnUrl, "locked", true));
        }

        var admin = await db.AdminUsers.FirstOrDefaultAsync(a => a.Username == username, HttpContext.RequestAborted);

        var gueltig = admin is not null
            && Hasher.VerifyHashedPassword(admin, admin.PasswordHash, password) != PasswordVerificationResult.Failed;

        if (!gueltig || admin is null)
        {
            var locked = AuthRateLimiting.RegisterFailedAttempt(HttpContext, true);
            return Redirect(AuthRateLimiting.BuildLoginRedirect(
                HttpContext, "/admin/login", returnUrl, locked ? "locked" : "invalid", true));
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, admin.Id.ToString()),
            new(ClaimTypes.Name, admin.Username),
            new(ClaimTypes.Role, "Admin")
        };

        foreach (var permission in admin.Permissions)
        {
            claims.Add(new Claim("Recht", permission));
        }

        var identity = new ClaimsIdentity(claims, AdminScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            AdminScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                AllowRefresh = true
            });

        AuthRateLimiting.ClearFailedAttempts(HttpContext, true);
        AuthRateLimiting.ClearRedirectCookie(HttpContext, true);
        return Redirect(AuthRateLimiting.SanitizeReturnUrl(returnUrl) ?? "/admin");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AdminScheme);
        return Redirect("/");
    }
}
