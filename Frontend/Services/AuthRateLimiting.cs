using System.Globalization;
using System.Text.Json;

namespace Frontend.Services;

public static class AuthRateLimiting
{
    public const string StatusCookie = "login_lockout";
    public const string FailedAttemptsCookie = "login_failed_attempts";
    public const string RedirectCookie = "login_redirect";

    public const string AdminStatusCookie = "admin_login_lockout";
    public const string AdminFailedAttemptsCookie = "admin_login_failed_attempts";
    public const string AdminRedirectCookie = "admin_login_redirect";

    public static readonly TimeSpan Window = TimeSpan.FromMinutes(10);
    public const int PermitLimit = 10;
    public static readonly TimeSpan Lockout = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan RedirectLifetime = TimeSpan.FromMinutes(5);

    public sealed record LoginRedirectState(string? ReturnUrl, string? Error);

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
        SetLockoutCookie(context, StatusCookie);
    }

    public static bool HasLockoutCookie(HttpContext context)
    {
        return context.Request.Cookies.ContainsKey(StatusCookie);
    }

    public static void SetAdminLockoutCookie(HttpContext context)
    {
        SetLockoutCookie(context, AdminStatusCookie);
    }

    public static bool HasAdminLockoutCookie(HttpContext context)
    {
        return context.Request.Cookies.ContainsKey(AdminStatusCookie);
    }

    public static bool RegisterFailedAttempt(HttpContext context, bool isAdmin = false)
    {
        var attemptsCookie = isAdmin ? AdminFailedAttemptsCookie : FailedAttemptsCookie;
        var attempts = int.TryParse(context.Request.Cookies[attemptsCookie], out var value) ? value : 0;
        attempts++;

        if (attempts >= PermitLimit)
        {
            if (isAdmin)
            {
                SetAdminLockoutCookie(context);
            }
            else
            {
                SetLockoutCookie(context);
            }

            context.Response.Cookies.Delete(attemptsCookie);
            return true;
        }

        context.Response.Cookies.Append(attemptsCookie, attempts.ToString(CultureInfo.InvariantCulture), new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            MaxAge = Window,
            Expires = DateTimeOffset.UtcNow.Add(Window)
        });

        return false;
    }

    public static void ClearFailedAttempts(HttpContext context, bool isAdmin = false)
    {
        context.Response.Cookies.Delete(isAdmin ? AdminFailedAttemptsCookie : FailedAttemptsCookie);
        context.Response.Cookies.Delete(isAdmin ? AdminStatusCookie : StatusCookie);
    }

    private static void SetLockoutCookie(HttpContext context, string cookieName)
    {
        context.Response.Cookies.Append(cookieName, "1", new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            MaxAge = Lockout,
            Expires = DateTimeOffset.UtcNow.Add(Lockout)
        });
    }

    public static string FormatDuration(TimeSpan duration)
    {
        return duration.TotalMinutes.ToString("0", CultureInfo.InvariantCulture);
    }
}
