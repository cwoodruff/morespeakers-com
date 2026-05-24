using Bogus;

using DataCategory = MoreSpeakers.Data.Models.ExpertiseCategory;
using DataExpertise = MoreSpeakers.Data.Models.Expertise;
using DataSector = MoreSpeakers.Data.Models.Sector;
using DataSpeakerType = MoreSpeakers.Data.Models.SpeakerType;
using DataUser = MoreSpeakers.Data.Models.User;

namespace MoreSpeakers.Data.Tests;

internal static class DataStoreFakers
{
    public static Faker<DataSpeakerType> SpeakerType() => new Faker<DataSpeakerType>()
        .RuleFor(s => s.Id, _ => 0)
        .RuleFor(s => s.Name, f => $"SpeakerType-{f.UniqueIndex}")
        .RuleFor(s => s.Description, f => f.Lorem.Sentence());

    public static Faker<DataSector> Sector() => new Faker<DataSector>()
        .RuleFor(s => s.Id, _ => 0)
        .RuleFor(s => s.Name, f => $"{f.Commerce.Department()}-{f.UniqueIndex}")
        .RuleFor(s => s.Slug, (_, sector) => sector.Name.ToLowerInvariant().Replace(' ', '-'))
        .RuleFor(s => s.Description, f => f.Lorem.Sentence())
        .RuleFor(s => s.DisplayOrder, f => f.Random.Int(1, 100))
        .RuleFor(s => s.IsActive, _ => true);

    public static Faker<DataCategory> ExpertiseCategory(int sectorId) => new Faker<DataCategory>()
        .RuleFor(c => c.Id, _ => 0)
        .RuleFor(c => c.Name, f => $"Category-{f.UniqueIndex}")
        .RuleFor(c => c.Description, f => f.Lorem.Sentence())
        .RuleFor(c => c.CreatedDate, _ => DateTime.UtcNow)
        .RuleFor(c => c.IsActive, _ => true)
        .RuleFor(c => c.SectorId, _ => sectorId);

    public static Faker<DataExpertise> Expertise(int categoryId) => new Faker<DataExpertise>()
        .RuleFor(e => e.Id, _ => 0)
        .RuleFor(e => e.Name, f => $"Expertise-{f.UniqueIndex}")
        .RuleFor(e => e.Description, f => f.Lorem.Sentence())
        .RuleFor(e => e.CreatedDate, _ => DateTime.UtcNow)
        .RuleFor(e => e.IsActive, _ => true)
        .RuleFor(e => e.ExpertiseCategoryId, _ => categoryId);

    public static Faker<DataUser> User(int speakerTypeId = 1) => new Faker<DataUser>()
        .RuleFor(u => u.Id, _ => Guid.NewGuid())
        .RuleFor(u => u.FirstName, f => f.Name.FirstName())
        .RuleFor(u => u.LastName, f => f.Name.LastName())
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.NormalizedEmail, (_, user) => user.Email!.ToUpperInvariant())
        .RuleFor(u => u.UserName, (_, user) => user.Email)
        .RuleFor(u => u.NormalizedUserName, (_, user) => user.UserName!.ToUpperInvariant())
        .RuleFor(u => u.EmailConfirmed, _ => true)
        .RuleFor(u => u.Bio, f => f.Lorem.Paragraph())
        .RuleFor(u => u.Goals, f => f.Lorem.Sentence())
        .RuleFor(u => u.SpeakerTypeId, _ => speakerTypeId)
        .RuleFor(u => u.IsAvailableForMentoring, _ => true)
        .RuleFor(u => u.MaxMentees, _ => 2)
        .RuleFor(u => u.CreatedDate, _ => DateTime.UtcNow)
        .RuleFor(u => u.UpdatedDate, _ => DateTime.UtcNow)
        .RuleFor(u => u.LockoutEnabled, _ => true)
        .RuleFor(u => u.SecurityStamp, _ => Guid.NewGuid().ToString("N"));
}
