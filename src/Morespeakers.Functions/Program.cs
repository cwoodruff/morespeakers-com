using System.Reflection;

using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MoreSpeakers.Functions.Interfaces;
using MoreSpeakers.Functions.Models;
using Serilog;
using Serilog.Exceptions;

var currentDirectory = Directory.GetCurrentDirectory();

var builder = FunctionsApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.ConfigureFunctionsWebApplication();

var settings = new Settings
{
    AzureCommunicationsConnectionString = string.Empty,
    BouncedEmailStatuses = string.Empty
};

builder.Configuration.SetBasePath(currentDirectory);
builder.Configuration.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
builder.Configuration.AddUserSecrets(Assembly.GetExecutingAssembly(), true, true);
builder.Configuration.AddEnvironmentVariables();
builder.Configuration.Bind("Settings", settings);
builder.Services.AddSingleton<ISettings>(settings);

// Configure the logger
string loggerFile = Path.Combine(currentDirectory, $"logs{Path.DirectorySeparatorChar}logs.txt");
ConfigureLogging(builder.Configuration, builder.Services, loggerFile, "Functions");

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.AddAzureBlobServiceClient("AzureStorageBlobs");
builder.AddAzureTableServiceClient("AzureStorageTables");
builder.AddAzureQueueServiceClient("AzureStorageQueues");

builder.Build().Run();


void ConfigureLogging(IConfiguration configurationRoot, IServiceCollection services, string logPath, string applicationName)
{
    var logger = new LoggerConfiguration()
#if DEBUG
        .MinimumLevel.Debug()
#else
        .MinimumLevel.Warning()
#endif
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
            configureApplicationInsightsLoggerOptions: (_) => { });
        loggingBuilder.AddSerilog(logger);
    });
}