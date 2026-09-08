using System.Globalization;
using System.Text.Json;
using System.Threading.RateLimiting;

namespace Frontend.Services;

public static class AuthRateLimiting
{
    public const string LoginPolicy = "login";
    public const string StatusCookie = "login_lockout";
    public const string RedirectCookie = "login_redirect";

    public const string AdminLoginPolicy = "admin-login";
    public const string AdminStatusCookie = "admin_login_lockout";
    public const string AdminRedirectCookie = "admin_login_redirect";

    public static readonly TimeSpan Window = TimeSpan.FromMinutes(10);
    public const int PermitLimit = 10;
    public static readonly TimeSpan Lockout = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan RedirectLifetime = TimeSpan.FromMinutes(5);

    public sealed record LoginRedirectState(string? ReturnUrl, string? Error);

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

    public static string BuildLoginRedirect(HttpContext context, string path, string? returnUrl, string? error,
        bool isAdmin = false)
    {
        SetRedirectCookie(context, returnUrl, error, isAdmin);
        return path;
    }

    public static LoginRedirectState? GetRedirectState(HttpContext context, bool isAdmin = false)
    {
        var cookieName = isAdmin ? AdminRedirectCookie : RedirectCookie;
        var value = context.Request.Cookies[cookieName];
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        try
        {
            var state = JsonSerializer.Deserialize<LoginRedirectState>(value);
            return state is null
                ? null
                : state with { ReturnUrl = SanitizeReturnUrl(state.ReturnUrl) };
        }
        catch (JsonException)
        {
            return null;
        }
    }

    public static void ClearRedirectCookie(HttpContext context, bool isAdmin = false)
    {
        context.Response.Cookies.Delete(isAdmin ? AdminRedirectCookie : RedirectCookie);
    }

    private static void SetRedirectCookie(HttpContext context, string? returnUrl, string? error, bool isAdmin)
    {
        var sanitized = SanitizeReturnUrl(returnUrl);
        if (sanitized is null && error is null)
        {
            ClearRedirectCookie(context, isAdmin);
            return;
        }

        context.Response.Cookies.Append(
            isAdmin ? AdminRedirectCookie : RedirectCookie,
            JsonSerializer.Serialize(new LoginRedirectState(sanitized, error)),
            new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                MaxAge = RedirectLifetime,
                Expires = DateTimeOffset.UtcNow.Add(RedirectLifetime)
            });
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
