using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Add Azure Storage
var storage = builder.AddAzureStorage("AzureStorage");

var logTable = storage.AddTables("AzureStorageTables");
var queues = storage.AddQueues("AzureStorageQueues");
storage.RunAsEmulator(azurite =>
{
    azurite.WithLifetime(ContainerLifetime.Persistent);
});

// Add SQL Server database
var sql = builder.AddSqlServer("sqldb")
    .WithImageTag("2022-latest")
    .WithLifetime(ContainerLifetime.Persistent);

var path = builder.AppHostDirectory;

var sqlText = string.Concat(
    File.ReadAllText(Path.Combine(path, @"../../scripts/database/create-database.sql")),
    " ",
    File.ReadAllText(Path.Combine(path, @"../../scripts/database/create-tables.sql")),
    " ",
    File.ReadAllText(Path.Combine(path, @"../../scripts/database/create-views.sql")),
    " ",
    File.ReadAllText(Path.Combine(path, @"../../scripts/database/create-functions.sql")),
    " ",
    File.ReadAllText(Path.Combine(path, @"../../scripts/database/seed-data.sql")));

var db = sql.AddDatabase("MoreSpeakers")
    .WithCreationScript(sqlText);

// Add the main web application
builder.AddProject<MoreSpeakers_Web>("web")
    .WaitFor(db)
    .WaitFor(logTable)
    .WaitFor(queues)
    .WithEnvironment("StorageConnectionString", logTable)
    .WithEnvironment("ConnectionStrings__sqldb", db)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.Build().Run();