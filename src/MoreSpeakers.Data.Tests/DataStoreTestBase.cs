using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Moq;

using MoreSpeakers.Data.MappingProfiles;

using DataCategory = MoreSpeakers.Data.Models.ExpertiseCategory;
using DataExpertise = MoreSpeakers.Data.Models.Expertise;
using DataSector = MoreSpeakers.Data.Models.Sector;
using DataSpeakerType = MoreSpeakers.Data.Models.SpeakerType;
using DataUser = MoreSpeakers.Data.Models.User;

namespace MoreSpeakers.Data.Tests;

/// <summary>
/// Shared test harness for DataStore tests.
/// Uses a fresh EF Core InMemory database per test because the production model contains SQL Server-specific defaults
/// and constraints that make SQLite in-memory brittle for fast unit-style coverage.
/// </summary>
public abstract class DataStoreTestBase : IAsyncLifetime
{
    protected MoreSpeakersDbContext Context { get; private set; } = null!;

    protected IMapper Mapper { get; private set; } = null!;

    protected CancellationToken CancellationToken => TestContext.Current.CancellationToken;

    public ValueTask InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<MoreSpeakersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        Context = new MoreSpeakersDbContext(options);

        var mapperConfiguration = new MapperConfiguration(
            cfg => cfg.AddProfile<MoreSpeakersProfile>(),
            new LoggerFactory());

        Mapper = mapperConfiguration.CreateMapper();

        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() => Context.DisposeAsync();

    protected Mock<ILogger<T>> CreateLogger<T>() where T : class => new();

    protected ExpertiseDataStore CreateExpertiseStore() => new(Context, Mapper, CreateLogger<ExpertiseDataStore>().Object);

    protected async Task<DataSpeakerType> AddSpeakerTypeAsync(Action<DataSpeakerType>? configure = null)
    {
        var speakerType = DataStoreFakers.SpeakerType().Generate();
        configure?.Invoke(speakerType);

        Context.SpeakerType.Add(speakerType);
        await Context.SaveChangesAsync(CancellationToken);

        return speakerType;
    }

    protected async Task<DataSector> AddSectorAsync(Action<DataSector>? configure = null)
    {
        var sector = DataStoreFakers.Sector().Generate();
        configure?.Invoke(sector);

        Context.Sectors.Add(sector);
        await Context.SaveChangesAsync(CancellationToken);

        return sector;
    }

    protected async Task<DataCategory> AddCategoryAsync(DataSector sector, Action<DataCategory>? configure = null)
    {
        var category = DataStoreFakers.ExpertiseCategory(sector.Id).Generate();
        category.SectorId = sector.Id;
        configure?.Invoke(category);

        Context.ExpertiseCategory.Add(category);
        await Context.SaveChangesAsync(CancellationToken);

        return category;
    }

    protected async Task<DataExpertise> AddExpertiseAsync(
        DataCategory category,
        string? name = null,
        string? description = null,
        bool isActive = true)
    {
        var generated = DataStoreFakers.Expertise(category.Id).Generate();
        var expertise = new DataExpertise
        {
            Name = name ?? generated.Name,
            Description = description ?? generated.Description,
            CreatedDate = generated.CreatedDate,
            IsActive = isActive,
            ExpertiseCategoryId = category.Id
        };

        Context.Expertise.Add(expertise);
        await Context.SaveChangesAsync(CancellationToken);

        return expertise;
    }

    protected async Task<DataUser> AddUserAsync(Action<DataUser>? configure = null)
    {
        var user = DataStoreFakers.User().Generate();
        configure?.Invoke(user);

        Context.Users.Add(user);
        await Context.SaveChangesAsync(CancellationToken);

        return user;
    }
}
