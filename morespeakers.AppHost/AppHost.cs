var builder = DistributedApplication.CreateBuilder(args);

// Add SQL Server database
var sqlServer = builder.AddSqlServer("sqlserver")
    .WithDataVolume();

var database = sqlServer.AddDatabase("morespeakers");

// Add the main web application
var webApp = builder.AddProject<Projects.morespeakers>("web")
    .WithReference(database)
    .WithExternalHttpEndpoints();

// Add Redis cache for session management (optional)
var redis = builder.AddRedis("cache")
    .WithDataVolume();

webApp.WithReference(redis);

builder.Build().Run();