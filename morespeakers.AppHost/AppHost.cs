using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server database
var sqldb = builder.AddConnectionString("sqldb");

// Add the main web application
var webApp = builder.AddProject<morespeakers>("web")
    .WaitFor(sqldb)
    .WithExternalHttpEndpoints();

// Add Redis cache for session management (optional)
var redis = builder.AddRedis("cache")
    .WithDataVolume();

webApp.WithReference(redis);

builder.Build().Run();