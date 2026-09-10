using System.Security.Claims;
using Frontend;
using MudBlazor.Services;
using Frontend.Components;
using Frontend.Data;
using Frontend.Data.Entities;
using Frontend.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
builder.Services.AddScoped<ICategoryAdminService, CategoryAdminService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IProfileValidator, ProfileValidator>();
builder.Services.AddScoped<ITeacherQuoteLikeService, TeacherQuoteLikeService>();
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
        options.Events.OnRedirectToLogin = context =>
        {
            var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
            context.Response.Redirect(AuthRateLimiting.BuildLoginRedirect(context.HttpContext, "/login", returnUrl, null));
            return Task.CompletedTask;
        };
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    })
    .AddCookie("AdminAuth", options =>
    {
        options.Cookie.Name = AdminCookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/admin/login";
        options.Events.OnRedirectToLogin = context =>
        {
            var returnUrl = context.Request.PathBase + context.Request.Path + context.Request.QueryString;
            context.Response.Redirect(AuthRateLimiting.BuildLoginRedirect(context.HttpContext, "/admin/login", returnUrl, null, true));
            return Task.CompletedTask;
        };
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
    
    //Admin Policies
    options.AddPolicy(Permissions.AdminList, options => options.RequireClaim("Recht", Permissions.AdminList));
    options.AddPolicy(Permissions.AdminErgebnisse, options => options.RequireClaim("Recht", Permissions.AdminErgebnisse));
    options.AddPolicy(Permissions.AdminFragen, options => options.RequireClaim("Recht", Permissions.AdminFragen));
    options.AddPolicy(Permissions.AdminLehrer, options => options.RequireClaim("Recht", Permissions.AdminLehrer));
    options.AddPolicy(Permissions.AdminLehrerzitate, options => options.RequireClaim("Recht", Permissions.AdminLehrerzitate));
    options.AddPolicy(Permissions.AdminSchueler, options => options.RequireClaim("Recht", Permissions.AdminSchueler));
    options.AddPolicy(Permissions.AdminSteckbriefe, options => options.RequireClaim("Recht", Permissions.AdminSteckbriefe));
});
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapControllers();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGet("/", async ([FromQuery] string? code) =>
{
    if (!string.IsNullOrEmpty(code))
    {
        return Results.Redirect($"/login/?code={code}");
    }

    return Results.Redirect("/login");
});

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
            PasswordHash = string.Empty,
            Permissions = new List<string>() {Permissions.AdminList}
        };
        user.PasswordHash = hasher.HashPassword(user, app.Configuration["Admin:Password"] ?? "admin");
        await dbContext.AdminUsers.AddAsync(user);
        await dbContext.SaveChangesAsync();
    }
}

app.Run();