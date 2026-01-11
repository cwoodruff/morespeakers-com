using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Rewrite;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Managers;
using MoreSpeakers.Web.Services;
using MoreSpeakers.Data;
using MoreSpeakers.Web.Endpoints;
using MoreSpeakers.Web.Filters;
using Serilog;
using Serilog.Exceptions;
using MoreSpeakers.Web.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add logging and telemetry
builder.Services.AddSingleton<ITelemetryInitializer, AzureWebAppRoleEnvironmentTelemetryInitializer>();
builder.Services.AddApplicationInsightsTelemetry();

// Configure the logger
var fullyQualifiedLogFile = Path.Combine(builder.Environment.ContentRootPath, $"logs{Path.DirectorySeparatorChar}logs.txt");
ConfigureLogging(builder.Configuration, builder.Services, fullyQualifiedLogFile, "Web");

// Add settings
var settings = new Settings
{
    Email = null!,
    GitHub = null!,
    AutoMapper = null!,
    ApplicationInsights = null!,
    Pagination = null!,
    OpenGraph = null!
};
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.Bind("Settings", settings);
settings.ApplicationInsights.ConnectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"] ?? string.Empty;
builder.Services.AddSingleton<ISettings>(settings);
builder.Services.AddSingleton<IAutoMapperSettings>(settings.AutoMapper);

// Add in AutoMapper
builder.Services.AddAutoMapper(config =>
{
    config.LicenseKey = settings.AutoMapper.LicenseKey;
    config.AddProfile<MoreSpeakers.Data.MappingProfiles.MoreSpeakersProfile>();
}, typeof(Program));

// Add database context
builder.AddSqlServerDbContext<MoreSpeakersDbContext>("sqldb");
builder.EnrichSqlServerDbContext<MoreSpeakersDbContext>(
    configureSettings: sqlServerSettings =>
    {
        sqlServerSettings.DisableRetry = false;
        sqlServerSettings.CommandTimeout = 30; // seconds
    });

// Add Identity services
builder.Services.AddDefaultIdentity<MoreSpeakers.Data.Models.User>(options =>
    {
        // Stores settings - required for passkey support
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;

        // Password settings
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 8;
        options.Password.RequiredUniqueChars = 1;

        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        // User settings
        options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
        options.User.RequireUniqueEmail = true;

        // Sign in settings
        options.SignIn.RequireConfirmedEmail = false;
        options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<MoreSpeakersDbContext>();
    
// Configure Passkey Options
builder.Services.Configure<IdentityPasskeyOptions>(options =>
{
    var serverDomain = builder.Configuration["Identity:Passkeys:ServerDomain"];
    if (!string.IsNullOrEmpty(serverDomain))
    {
        options.ServerDomain = serverDomain;
    }
});

// Configure cookie paths explicitly to ensure expected redirects
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Pages/NotFound";
});

// Add Authorization with AdminOnly policy
builder.Services.AddAuthorization(options =>
{
    // Baseline Admin area policy: only Administrators can enter the Admin area
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole(AppRoles.Administrator));

    // Keep Administrator with full access to all policies
    options.AddPolicy(PolicyNames.ManageUsers, policy =>
        policy.RequireRole(AppRoles.Administrator, AppRoles.UserManager, AppRoles.Moderator));

    options.AddPolicy(PolicyNames.ManageCatalog, policy =>
        policy.RequireRole(AppRoles.Administrator, AppRoles.CatalogManager, AppRoles.Moderator));

    options.AddPolicy(PolicyNames.ViewReports, policy =>
        policy.RequireRole(AppRoles.Administrator, AppRoles.Reporter, AppRoles.Moderator));
});

// Add Razor Pages
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");

    // Admin area baseline: require AdminOnly for all pages under /Admin
    // Keep the dashboard (Index) under AdminOnly only; granular policies apply to the subfolders below.
    options.Conventions.AuthorizeAreaFolder("Admin", "/", policy: PolicyNames.AdminOnly);

    // Granular least-privilege policies by subfolders within the Admin area
    // Users management pages → ManageUsers policy
    options.Conventions.AuthorizeAreaFolder("Admin", "/Users", policy: PolicyNames.ManageUsers);
    // Catalog/content management pages → ManageCatalog policy
    options.Conventions.AuthorizeAreaFolder("Admin", "/Catalog", policy: PolicyNames.ManageCatalog);
    // Reports/analytics pages → ViewReports policy
    options.Conventions.AuthorizeAreaFolder("Admin", "/Reports", policy: PolicyNames.ViewReports);
    
    // services.AddRazorPages()
    //     .AddMvcOptions(options =>
    //     {
    //         options.Filters.Add(new MustChangePasswordFilter(new UserManager()));
    //     });
});

// Add Azure Storage services
builder.AddAzureBlobServiceClient("AzureStorageBlobs");
builder.AddAzureTableServiceClient("AzureStorageTables");
builder.AddAzureQueueServiceClient("AzureStorageQueues");

// Add application services
builder.Services.AddScoped<IExpertiseDataStore, ExpertiseDataStore>();
builder.Services.AddScoped<IMentoringDataStore, MentoringDataStore>();
builder.Services.AddScoped<ISocialMediaSiteDataStore, SocialMediaSiteDataStore>();
builder.Services.AddScoped<ISectorDataStore, SectorDataStore>(); 
builder.Services.AddScoped<IUserDataStore, UserDataStore>();
builder.Services.AddScoped<IExpertiseManager, ExpertiseManager>();
builder.Services.AddScoped<IMentoringManager, MentoringManager>();
builder.Services.AddScoped<ISectorManager, SectorManager>();
builder.Services.AddScoped<ISocialMediaSiteManager, SocialMediaSiteManager>();
builder.Services.AddScoped<IUserManager, UserManager>();

builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, EmailSender>();
builder.Services.AddScoped<ITemplatedEmailSender, TemplatedEmailSender>();
builder.Services.AddScoped<IRazorPartialToStringRenderer, RazorPartialToStringRenderer>();
builder.Services.AddScoped<IOpenGraphService, OpenGraphService>();
builder.Services.AddScoped<IOpenGraphSpeakerProfileImageGenerator, OpenGraphSpeakerProfileImageGenerator>();

// Register GitHub Service and Add in-memory caching (required by GitHubService constructor)
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IGitHubService, GitHubService>();

// Add HTTP context accessor for services
builder.Services.AddHttpContextAccessor();

// Configure cookie policy
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = _ => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseRewriter(new RewriteOptions().Add(context =>
{
    if (context.HttpContext.Request.Path == "/Identity/Account/Logout")
    {
        context.HttpContext.Response.Redirect("/");
    }
}));

// Handle 404 errors
app.UseStatusCodePagesWithReExecute("/NotFound");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapDefaultEndpoints();
app.MapRazorPages().WithStaticAssets();
app.MapPasskeyEndpoints(); // Enable passkey endpoints
app.MapExpertiseEndpoints(); // Enable expertise API endpoints

app.Run();

void ConfigureLogging(IConfigurationRoot configurationRoot, IServiceCollection services, string logPath, string applicationName)
{
    var logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithEnvironmentName()
        .Enrich.WithAssemblyName()
        .Enrich.WithAssemblyVersion(true)
        .Enrich.WithExceptionDetails()
        .Enrich.WithProperty("Application", applicationName)
        .Destructure.ToMaximumDepth(4)
        .Destructure.ToMaximumStringLength(100)
        .Destructure.ToMaximumCollectionCount(10)
        .WriteTo.Console()
        .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
        .CreateLogger();
    services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddApplicationInsights(configureTelemetryConfiguration: (config) =>
                config.ConnectionString =
                    configurationRoot["APPLICATIONINSIGHTS_CONNECTION_STRING"],
            configureApplicationInsightsLoggerOptions: (_) => { });loggingBuilder.AddApplicationInsights();
        loggingBuilder.AddSerilog(logger);
    });
}

// Expose Program for WebApplicationFactory in integration tests
public partial class Program { }