using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http.Extensions;

namespace Frontend.Services;

public static class AuthRateLimiting
{
    public const string LoginPolicy = "login";
    public const string StatusCookie = "login_lockout";

    public const string AdminLoginPolicy = "admin-login";
    public const string AdminStatusCookie = "admin_login_lockout";

    public static readonly TimeSpan Window = TimeSpan.FromMinutes(10);
    public const int PermitLimit = 10;
    public static readonly TimeSpan Lockout = TimeSpan.FromMinutes(15);

    public static RateLimitPartition<string> PartitionByIp(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetSlidingWindowLimiter(ip, _ => new SlidingWindowRateLimiterOptions
        {
            PermitLimit = PermitLimit,
            Window = Window,
            SegmentsPerWindow = 10,
            QueueLimit = 0,
            AutoReplenishment = true
        });
    }

    public static string? SanitizeReturnUrl(string? returnUrl)
    {
        return !string.IsNullOrEmpty(returnUrl)
               && returnUrl.StartsWith('/')
               && !returnUrl.StartsWith("//")
            ? returnUrl
            : null;
    }

    public static string BuildLoginRedirect(string path, string? returnUrl, string? error)
    {
        var qb = new QueryBuilder();
        var sanitized = SanitizeReturnUrl(returnUrl);
        if (sanitized is not null)
        {
            qb.Add("returnUrl", sanitized);
        }

        if (error is not null)
        {
            qb.Add("error", error);
        }

        return path + qb.ToQueryString();
    }

    public static void SetLockoutCookie(HttpContext context)
    {
        context.Response.Cookies.Append(StatusCookie, "1", new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.Add(Lockout)
        });
    }

    public static bool HasLockoutCookie(HttpContext context)
    {
        return context.Request.Cookies.ContainsKey(StatusCookie);
    }

    public static void SetAdminLockoutCookie(HttpContext context)
    {
        context.Response.Cookies.Append(AdminStatusCookie, "1", new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Expires = DateTimeOffset.UtcNow.Add(Lockout)
        });
    }

    public static bool HasAdminLockoutCookie(HttpContext context)
    {
        return context.Request.Cookies.ContainsKey(AdminStatusCookie);
    }

    public static string FormatDuration(TimeSpan duration)
    {
        return duration.TotalMinutes.ToString("0", CultureInfo.InvariantCulture);
    }
}
