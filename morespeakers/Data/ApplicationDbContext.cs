using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using morespeakers.Models;

namespace morespeakers.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<SpeakerType> SpeakerTypes { get; set; }
    public DbSet<Expertise> Expertise { get; set; }
    public DbSet<UserExpertise> UserExpertise { get; set; }
    public DbSet<SocialMedia> SocialMedia { get; set; }
    public DbSet<Mentorship> Mentorships { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure User entity
        builder.Entity<User>(entity =>
        {
            entity.ToTable("AspNetUsers");
            
            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.Email)
                .IsUnique();

            entity.HasIndex(e => e.SpeakerTypeId);

            entity.HasOne(e => e.SpeakerType)
                .WithMany(s => s.Users)
                .HasForeignKey(e => e.SpeakerTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.MentorshipsAsMentor)
                .WithOne(m => m.Mentor)
                .HasForeignKey(m => m.MentorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.MentorshipsAsNewSpeaker)
                .WithOne(m => m.NewSpeaker)
                .HasForeignKey(m => m.NewSpeakerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure SpeakerType entity
        builder.Entity<SpeakerType>(entity =>
        {
            entity.HasIndex(e => e.Name)
                .IsUnique();

            entity.Property(e => e.Name)
                .IsRequired();
        });

        // Configure Expertise entity
        builder.Entity<Expertise>(entity =>
        {
            entity.HasIndex(e => e.Name)
                .IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        // Configure UserExpertise many-to-many relationship
        builder.Entity<UserExpertise>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.ExpertiseId });

            entity.HasOne(e => e.User)
                .WithMany(u => u.UserExpertise)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Expertise)
                .WithMany(ex => ex.UserExpertise)
                .HasForeignKey(e => e.ExpertiseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure SocialMedia entity
        builder.Entity<SocialMedia>(entity =>
        {
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.UserId);

            entity.HasOne(e => e.User)
                .WithMany(u => u.SocialMediaLinks)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Mentorship entity
        builder.Entity<Mentorship>(entity =>
        {
            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.RequestDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.Status)
                .HasDefaultValue("Pending");

            entity.HasIndex(e => e.NewSpeakerId);
            entity.HasIndex(e => e.MentorId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.Status, e.RequestDate });

            entity.ToTable(t => t.HasCheckConstraint(
                "CHK_Mentorships_Status",
                "[Status] IN ('Pending', 'Active', 'Completed', 'Cancelled')"));

            entity.ToTable(t => t.HasCheckConstraint(
                "CHK_Mentorships_DifferentUsers",
                "[NewSpeakerId] != [MentorId]"));
        });

        // Seed initial data
        SeedData(builder);
    }

    private static void SeedData(ModelBuilder builder)
    {
        // Seed SpeakerTypes
        builder.Entity<SpeakerType>().HasData(
            new SpeakerType
            {
                Id = 1,
                Name = "NewSpeaker",
                Description = "Aspiring speakers seeking mentorship and guidance"
            },
            new SpeakerType
            {
                Id = 2,
                Name = "ExperiencedSpeaker",
                Description = "Experienced speakers offering mentorship"
            }
        );

        // Seed common Expertise areas
        var expertiseAreas = new[]
        {
            "C#", ".NET", "ASP.NET Core", "Blazor", "Azure", "JavaScript", "TypeScript",
            "React", "Angular", "Vue.js", "Node.js", "Python", "Java", "Kubernetes",
            "Docker", "DevOps", "CI/CD", "Machine Learning", "AI", "Data Science",
            "SQL Server", "PostgreSQL", "MongoDB", "Redis", "Microservices",
            "API Design", "GraphQL", "REST", "Security", "Performance Optimization",
            "Testing", "TDD", "BDD", "Agile", "Scrum", "Leadership", "Architecture",
            "Design Patterns", "Clean Code", "SOLID Principles", "DDD"
        };

        var expertiseEntities = expertiseAreas.Select((area, index) => new Expertise
        {
            Id = index + 1,
            Name = area,
            Description = $"Expertise in {area}",
            CreatedDate = DateTime.UtcNow
        }).ToArray();

        builder.Entity<Expertise>().HasData(expertiseEntities);

        // Seed Identity roles
        builder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = "NewSpeaker",
                NormalizedName = "NEWSPEAKER"
            },
            new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = "ExperiencedSpeaker",
                NormalizedName = "EXPERIENCEDSPEAKER"
            },
            new IdentityRole<Guid>
            {
                Id = Guid.NewGuid(),
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR"
            }
        );
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries<User>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.UpdatedDate = DateTime.UtcNow;
        }
    }
}