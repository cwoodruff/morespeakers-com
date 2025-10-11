using Azure.Provisioning.Storage;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Add Azure Storage
var storage = builder.AddAzureStorage("AzureStorage");
var blobs = storage.AddBlobs("AzureStorageBlobs");
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

// Add the Azure Functions
builder.AddAzureFunctionsProject<Morespeakers_Functions>("functions")
    .WithRoleAssignments(storage,
        // Storage Account Contributor and Storage Blob Data Owner roles are required by the Azure Functions host
        StorageBuiltInRole.StorageAccountContributor, StorageBuiltInRole.StorageBlobDataOwner,
        // Queue Data Contributor role is required to send messages to the queue
        StorageBuiltInRole.StorageQueueDataContributor)
    .WithHostStorage(storage)
    .WithReference(blobs)
    .WithReference(logTable)
    .WithReference(queues)
    .WithExternalHttpEndpoints()
    .WaitFor(db)
    .WaitFor(blobs)
    .WaitFor(logTable)
    .WaitFor(queues)
    .WithEnvironment("ConnectionStrings__sqldb", db);

// Add the main web application
builder.AddProject<MoreSpeakers_Web>("web")
    .WithReference(blobs)
    .WithReference(logTable)
    .WithReference(queues)
    .WaitFor(db)
    .WaitFor(blobs)
    .WaitFor(logTable)
    .WaitFor(queues)
    .WithEnvironment("ConnectionStrings__sqldb", db)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health");

builder.Build().Run();