using Bogus;
using morespeakers.Models;

namespace MoreSpeakers.Tests.Utilities;

public static class TestDataBuilder
{
    private static readonly Faker<User> UserFaker = new Faker<User>()
        .RuleFor(u => u.Id, f => Guid.NewGuid())
        .RuleFor(u => u.FirstName, f => f.Name.FirstName())
        .RuleFor(u => u.LastName, f => f.Name.LastName())
        .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
        .RuleFor(u => u.UserName, (f, u) => u.Email)
        .RuleFor(u => u.PhoneNumber, f => f.Phone.PhoneNumber())
        .RuleFor(u => u.Bio, f => f.Lorem.Paragraph(3))
        .RuleFor(u => u.Goals, f => f.Lorem.Paragraph(2))
        .RuleFor(u => u.SessionizeUrl, f => f.Internet.Url())
        .RuleFor(u => u.HeadshotUrl, f => f.Internet.Avatar())
        .RuleFor(u => u.SpeakerTypeId, f => f.Random.Int(1, 2))
        .RuleFor(u => u.CreatedDate, f => f.Date.Recent(365))
        .RuleFor(u => u.UpdatedDate, (f, u) => u.CreatedDate.AddDays(f.Random.Int(0, 30)));

    private static readonly Faker<Expertise> ExpertiseFaker = new Faker<Expertise>()
        .RuleFor(e => e.Id, f => f.Random.Int(1, 1000))
        .RuleFor(e => e.Name, f => f.Hacker.Noun())
        .RuleFor(e => e.Description, f => f.Lorem.Sentence())
        .RuleFor(e => e.CreatedDate, f => f.Date.Recent(365));

    private static readonly Faker<SpeakerType> SpeakerTypeFaker = new Faker<SpeakerType>()
        .RuleFor(st => st.Id, f => f.Random.Int(1, 10))
        .RuleFor(st => st.Name, f => f.PickRandom("NewSpeaker", "ExperiencedSpeaker"))
        .RuleFor(st => st.Description, f => f.Lorem.Sentence());

    private static readonly Faker<SocialMedia> SocialMediaFaker = new Faker<SocialMedia>()
        .RuleFor(sm => sm.Id, f => f.Random.Int(1, 1000))
        .RuleFor(sm => sm.Platform, f => f.PickRandom("LinkedIn", "Twitter", "GitHub", "YouTube", "Website"))
        .RuleFor(sm => sm.Url, f => f.Internet.Url())
        .RuleFor(sm => sm.UserId, f => Guid.NewGuid())
        .RuleFor(sm => sm.CreatedDate, f => f.Date.Recent(365));

    private static readonly Faker<Mentorship> MentorshipFaker = new Faker<Mentorship>()
        .RuleFor(m => m.Id, f => Guid.NewGuid())
        .RuleFor(m => m.NewSpeakerId, f => Guid.NewGuid())
        .RuleFor(m => m.MentorId, f => Guid.NewGuid())
        .RuleFor(m => m.Status, f => f.PickRandom("Pending", "Active", "Completed", "Cancelled"))
        .RuleFor(m => m.RequestDate, f => f.Date.Recent(60))
        .RuleFor(m => m.AcceptedDate, (f, m) => m.Status != "Pending" ? f.Date.Between(m.RequestDate, DateTime.UtcNow) : null)
        .RuleFor(m => m.CompletedDate, (f, m) => m.Status == "Completed" ? f.Date.Between(m.AcceptedDate ?? m.RequestDate, DateTime.UtcNow) : null)
        .RuleFor(m => m.Notes, f => f.Lorem.Paragraph());

    public static User CreateUser(Action<User>? configure = null)
    {
        var user = UserFaker.Generate();
        configure?.Invoke(user);
        return user;
    }

    public static List<User> CreateUsers(int count, Action<User>? configure = null)
    {
        var users = UserFaker.Generate(count);
        if (configure != null)
        {
            users.ForEach(configure);
        }
        return users;
    }

    public static User CreateNewSpeaker(Action<User>? configure = null)
    {
        return CreateUser(user =>
        {
            user.SpeakerTypeId = 1;
            user.SpeakerType = new SpeakerType { Id = 1, Name = "NewSpeaker", Description = "New speakers" };
            configure?.Invoke(user);
        });
    }

    public static User CreateExperiencedSpeaker(Action<User>? configure = null)
    {
        return CreateUser(user =>
        {
            user.SpeakerTypeId = 2;
            user.SpeakerType = new SpeakerType { Id = 2, Name = "ExperiencedSpeaker", Description = "Experienced speakers" };
            configure?.Invoke(user);
        });
    }

    public static Expertise CreateExpertise(Action<Expertise>? configure = null)
    {
        var expertise = ExpertiseFaker.Generate();
        configure?.Invoke(expertise);
        return expertise;
    }

    public static List<Expertise> CreateExpertise(int count, Action<Expertise>? configure = null)
    {
        var expertise = ExpertiseFaker.Generate(count);
        if (configure != null)
        {
            expertise.ForEach(configure);
        }
        return expertise;
    }

    public static SpeakerType CreateSpeakerType(Action<SpeakerType>? configure = null)
    {
        var speakerType = SpeakerTypeFaker.Generate();
        configure?.Invoke(speakerType);
        return speakerType;
    }

    public static SocialMedia CreateSocialMedia(Action<SocialMedia>? configure = null)
    {
        var socialMedia = SocialMediaFaker.Generate();
        configure?.Invoke(socialMedia);
        return socialMedia;
    }

    public static List<SocialMedia> CreateSocialMedia(int count, Action<SocialMedia>? configure = null)
    {
        var socialMedia = SocialMediaFaker.Generate(count);
        if (configure != null)
        {
            socialMedia.ForEach(configure);
        }
        return socialMedia;
    }

    public static Mentorship CreateMentorship(Action<Mentorship>? configure = null)
    {
        var mentorship = MentorshipFaker.Generate();
        configure?.Invoke(mentorship);
        return mentorship;
    }

    public static List<Mentorship> CreateMentorships(int count, Action<Mentorship>? configure = null)
    {
        var mentorships = MentorshipFaker.Generate(count);
        if (configure != null)
        {
            mentorships.ForEach(configure);
        }
        return mentorships;
    }

    public static UserExpertise CreateUserExpertise(Guid? userId = null, int? expertiseId = null)
    {
        return new UserExpertise
        {
            UserId = userId ?? Guid.NewGuid(),
            ExpertiseId = expertiseId ?? new Faker().Random.Int(1, 100)
        };
    }

    public static User CreateUserWithExpertise(int expertiseCount = 3, Action<User>? configure = null)
    {
        var user = CreateUser(configure);
        var expertise = CreateExpertise(expertiseCount);

        user.UserExpertise = expertise.Select(e => new UserExpertise
        {
            UserId = user.Id,
            ExpertiseId = e.Id,
            Expertise = e
        }).ToList();

        return user;
    }

    public static User CreateUserWithSocialMedia(int socialMediaCount = 2, Action<User>? configure = null)
    {
        var user = CreateUser(configure);
        var socialMedia = CreateSocialMedia(socialMediaCount, sm => sm.UserId = user.Id);
        user.SocialMediaLinks = socialMedia;
        return user;
    }

    public static Mentorship CreatePendingMentorship(Guid? newSpeakerId = null, Guid? mentorId = null)
    {
        return CreateMentorship(m =>
        {
            m.NewSpeakerId = newSpeakerId ?? Guid.NewGuid();
            m.MentorId = mentorId ?? Guid.NewGuid();
            m.Status = "Pending";
            m.AcceptedDate = null;
            m.CompletedDate = null;
        });
    }

    public static Mentorship CreateActiveMentorship(Guid? newSpeakerId = null, Guid? mentorId = null)
    {
        return CreateMentorship(m =>
        {
            m.NewSpeakerId = newSpeakerId ?? Guid.NewGuid();
            m.MentorId = mentorId ?? Guid.NewGuid();
            m.Status = "Active";
            m.AcceptedDate = DateTime.UtcNow.AddDays(-10);
            m.CompletedDate = null;
        });
    }

    public static Mentorship CreateCompletedMentorship(Guid? newSpeakerId = null, Guid? mentorId = null)
    {
        return CreateMentorship(m =>
        {
            m.NewSpeakerId = newSpeakerId ?? Guid.NewGuid();
            m.MentorId = mentorId ?? Guid.NewGuid();
            m.Status = "Completed";
            m.AcceptedDate = DateTime.UtcNow.AddDays(-30);
            m.CompletedDate = DateTime.UtcNow.AddDays(-5);
        });
    }
}