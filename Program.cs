using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NyayaSetu.Data;
using NyayaSetu.Interfaces;
using NyayaSetu.Models;
using NyayaSetu.Services;

var builder = WebApplication.CreateBuilder(args);

// Database & Entity Framework Core (SQL Server)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// ASP.NET Core Identity with secure password and lockout policies
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.User.RequireUniqueEmail = true;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Role-based Authorization Policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CitizenOnly", p => p.RequireRole(AppRoles.Citizen));
    options.AddPolicy("OfficerOnly", p => p.RequireRole(AppRoles.DepartmentOfficer, AppRoles.SuperAdmin));
    options.AddPolicy("LegalAidOnly", p => p.RequireRole(AppRoles.LegalAidOfficer, AppRoles.SuperAdmin));
    options.AddPolicy("AdminOnly", p => p.RequireRole(AppRoles.SuperAdmin));
});

// Application Services
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IOcrService, OcrService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();
builder.Services.AddScoped<ILegalAidService, LegalAidService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddSingleton<IAiAssistantService, AiAssistantService>();
builder.Services.AddSingleton<ILocalizationService, LocalizationService>();

// Automatic Overdue Complaint Escalation Background Worker
builder.Services.AddHostedService<OverdueEscalationBackgroundService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Seed roles, departments, sample accounts, and initial data on startup
await DbInitializer.SeedAsync(app.Services);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
