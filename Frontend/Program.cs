using System.Security.Claims;
using MudBlazor.Services;
using Frontend.Components;
using Frontend.Data;
using Frontend.Data.Entities;
using Frontend.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IYearbookService, YearbookService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IVotingStudentService, VotingStudentService>();
builder.Services.AddScoped<IVotingTeacherService, VotingTeacherService>();
builder.Services.AddScoped<ITeacherQuoteService, TeacherQuoteService>();
builder.Services.AddScoped<ITeacherManagementService, TeacherManagementService>();
builder.Services.AddScoped<IModerationService, ModerationService>();
builder.Services.AddScoped<IRankingService, RankingService>();
builder.Services.AddScoped<IResultService, ResultService>();
builder.Services.AddScoped<IStudentManagementService, StudentManagementService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<IAdminUserService, AdminUserService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
// TODO: Muss ich in separate Module aufteilen

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

const string AdminCookieName = "abizeitung_admin_auth";
builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Default";
        options.DefaultChallengeScheme = "Default";
    })
    .AddPolicyScheme("Default", "Default", options =>
    {
        options.ForwardDefaultSelector = context =>
            context.Request.Cookies.ContainsKey(AdminCookieName)
                ? "AdminAuth"
                : CookieAuthenticationDefaults.AuthenticationScheme;
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = "abizeitung_auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    })
    .AddCookie("AdminAuth", options =>
    {
        options.Cookie.Name = AdminCookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/admin/login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Permissions.Steckbrief, options => options.RequireClaim("Recht", Permissions.Steckbrief));
    options.AddPolicy(Permissions.RankingSchueler, options => options.RequireClaim("Recht", Permissions.RankingSchueler));
    options.AddPolicy(Permissions.RankingLehrer, options => options.RequireClaim("Recht", Permissions.RankingLehrer));
    options.AddPolicy(Permissions.Lehrerzitate, options => options.RequireClaim("Recht", Permissions.Lehrerzitate));
    options.AddPolicy(Permissions.Bestellungen, options => options.RequireClaim("Recht", Permissions.Bestellungen));
    options.AddPolicy(Permissions.SchülerKommentare, options => options.RequireClaim("Recht", Permissions.SchülerKommentare));
    options.AddPolicy("Database", options => options.RequireClaim(ClaimTypes.Role, "Admin"));
    // TODO: Eigene Klasse für Policies schreiben ._.
});
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(AuthRateLimiting.LoginPolicy, AuthRateLimiting.PartitionByIp);
    options.AddPolicy(AuthRateLimiting.AdminLoginPolicy, AuthRateLimiting.PartitionByIp);
    options.OnRejected = (context, cancellationToken) =>
    {
        var httpContext = context.HttpContext;
        var formReturnUrl = httpContext.Request.HasFormContentType
            ? httpContext.Request.Form["returnUrl"].ToString()
            : null;

        if (httpContext.Request.Path.StartsWithSegments("/admin"))
        {
            AuthRateLimiting.SetAdminLockoutCookie(httpContext);
            httpContext.Response.Redirect(AuthRateLimiting.BuildLoginRedirect("/admin/login", formReturnUrl, "locked"));
        }
        else
        {
            AuthRateLimiting.SetLockoutCookie(httpContext);
            httpContext.Response.Redirect(AuthRateLimiting.BuildLoginRedirect("/login", formReturnUrl, "locked"));
        }

        return ValueTask.CompletedTask;
    };
});

// TODO: Funktioniert nicht, muss ich verbessern
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapReverseProxy();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    
    var existing = await dbContext.AdminUsers.FirstOrDefaultAsync(a => a.Username == "admin");
    if (existing == null)
    {
        var hasher = new PasswordHasher<AdminUser>();
        var user = new AdminUser()
        {
            Username = app.Configuration["Admin:Username"] ?? "admin",
            PasswordHash = string.Empty
        };
        user.PasswordHash = hasher.HashPassword(user, app.Configuration["Admin:Password"] ?? "admin");
        await dbContext.AdminUsers.AddAsync(user);
        await dbContext.SaveChangesAsync();
    }

    
}

app.Run();