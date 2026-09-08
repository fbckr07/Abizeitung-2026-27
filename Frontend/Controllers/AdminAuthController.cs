using System.Security.Claims;
using Frontend.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
    [EnableRateLimiting(AuthRateLimiting.AdminLoginPolicy)]
    public async Task<IActionResult> Login(
        [FromForm] string username,
        [FromForm] string password,
        [FromForm] string? returnUrl = null)
    {
        var admin = await db.AdminUsers.FirstOrDefaultAsync(a => a.Username == username, HttpContext.RequestAborted);

        var gueltig = admin is not null
            && Hasher.VerifyHashedPassword(admin, admin.PasswordHash, password) != PasswordVerificationResult.Failed;

        if (!gueltig || admin is null)
        {
            return Redirect(AuthRateLimiting.BuildLoginRedirect("/admin/login", returnUrl, "invalid"));
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

        return Redirect(AuthRateLimiting.SanitizeReturnUrl(returnUrl) ?? "/admin");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(AdminScheme);
        return Redirect("/");
    }
}
