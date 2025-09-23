using Microsoft.EntityFrameworkCore;
using morespeakers.Data;
using morespeakers.Models;

namespace MoreSpeakers.Tests;

public abstract class TestBase : IDisposable
{
    private static readonly object _lock = new();
    private static int _testCounter;
    protected readonly ApplicationDbContext Context;

    protected TestBase()
    {
        var testId = Interlocked.Increment(ref _testCounter);
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"TestDb_{testId}_{Guid.NewGuid()}")
            .EnableSensitiveDataLogging()
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
        SeedTestData();
    }

    public virtual void Dispose()
    {
        Context.Dispose();
    }

    protected virtual void SeedTestData()
    {
        // Clear any existing data first
        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();

        // Don't add SpeakerTypes and Expertise manually - they're seeded by the ApplicationDbContext
        // Just save to trigger the built-in seeding
        Context.SaveChanges();

        // Now add test users
        var newSpeakerId = Guid.NewGuid();
        var experiencedSpeakerId = Guid.NewGuid();

        var newSpeaker = new User
        {
            Id = newSpeakerId,
            UserName = "newspeaker@test.com",
            Email = "newspeaker@test.com",
            FirstName = "John",
            LastName = "Doe",
            Bio = "Aspiring speaker looking for guidance",
            Goals = "Learn public speaking",
            PhoneNumber = "555-0123",
            SpeakerTypeId = 1,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        var experiencedSpeaker = new User
        {
            Id = experiencedSpeakerId,
            UserName = "mentor@test.com",
            Email = "mentor@test.com",
            FirstName = "Jane",
            LastName = "Smith",
            Bio = "Experienced speaker and mentor",
            Goals = "Help others become great speakers",
            PhoneNumber = "555-0124",
            SpeakerTypeId = 2,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };

        Context.Users.AddRange(newSpeaker, experiencedSpeaker);
        Context.SaveChanges();

        // Add UserExpertise relationships
        Context.UserExpertise.AddRange(
            new UserExpertise { UserId = newSpeakerId, ExpertiseId = 1 },
            new UserExpertise { UserId = newSpeakerId, ExpertiseId = 3 },
            new UserExpertise { UserId = experiencedSpeakerId, ExpertiseId = 1 },
            new UserExpertise { UserId = experiencedSpeakerId, ExpertiseId = 2 },
            new UserExpertise { UserId = experiencedSpeakerId, ExpertiseId = 4 }
        );

        // Add Social Media
        Context.SocialMedia.AddRange(
            new SocialMedia
            {
                UserId = newSpeakerId, Platform = "LinkedIn", Url = "https://linkedin.com/in/johndoe",
                CreatedDate = DateTime.UtcNow
            },
            new SocialMedia
            {
                UserId = experiencedSpeakerId, Platform = "Twitter", Url = "https://twitter.com/janesmith",
                CreatedDate = DateTime.UtcNow
            }
        );

        Context.SaveChanges();
    }

    protected User GetNewSpeaker()
    {
        return Context.Users.First(u => u.SpeakerTypeId == 1);
    }

    protected User GetExperiencedSpeaker()
    {
        return Context.Users.First(u => u.SpeakerTypeId == 2);
    }
}