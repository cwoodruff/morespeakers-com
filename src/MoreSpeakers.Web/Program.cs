using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WindowsServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Managers;
using MoreSpeakers.Web.Services;
using MoreSpeakers.Data;

using Serilog;
using Serilog.Exceptions;

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
    Email = null!
};
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.Bind("Settings", settings);
builder.Services.AddSingleton<ISettings>(settings);
builder.Services.TryAddSingleton<IDatabaseSettings>(new DatabaseSettings
{
    DatabaseConnectionString = builder.Configuration.GetConnectionString("sqldb") ?? string.Empty
});

// Add database context
builder.Services.AddDbContext<MoreSpeakersDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("sqldb") ?? string.Empty);
});
//builder.AddSqlServerDbContext<MoreSpeakersDbContext>("sqldb");
builder.EnrichSqlServerDbContext<MoreSpeakersDbContext>(
    configureSettings: sqlServerSettings =>
    {
        sqlServerSettings.DisableRetry = false;
        sqlServerSettings.CommandTimeout = 30; // seconds
    });

// Add Identity services
builder.Services.AddDefaultIdentity<MoreSpeakers.Data.Models.User>(options =>
    {
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

// Add Razor Pages
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
});

// Add Azure Storage services
builder.AddAzureBlobServiceClient("AzureStorageBlobs");
builder.AddAzureTableServiceClient("AzureStorageTables");
builder.AddAzureQueueServiceClient("AzureStorageQueues");

// Add application services
builder.Services.AddScoped<IExpertiseDataStore, ExpertiseDataStore>();
builder.Services.AddScoped<IMentoringDataStore, MentoringDataStore>();
builder.Services.AddScoped<ISpeakerDataStore, SpeakerDataStore>();
builder.Services.AddScoped<IExpertiseManager, ExpertiseManager>();
builder.Services.AddScoped<IMentoringManager, MentoringManager>();
builder.Services.AddScoped<ISpeakerManager, SpeakerManager>();

builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

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

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCookiePolicy();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapRazorPages();

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