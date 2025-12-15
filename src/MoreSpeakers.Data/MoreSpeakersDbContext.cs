using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using MoreSpeakers.Data.Models;

namespace MoreSpeakers.Data;

public class MoreSpeakersDbContext
    : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public MoreSpeakersDbContext(DbContextOptions<MoreSpeakersDbContext> options)
        : base(options)
    {
        
    }
    
    public DbSet<SpeakerType> SpeakerType { get; set; }
    public DbSet<Expertise> Expertise { get; set; }
    public DbSet<ExpertiseCategory> ExpertiseCategory { get; set; }
    public DbSet<Sector> Sectors { get; set; }
    public DbSet<UserExpertise> UserExpertise { get; set; }
    public DbSet<Mentorship> Mentorship { get; set; }
    public DbSet<MentorshipExpertise> MentorshipExpertise { get; set; }
    public DbSet<SocialMediaSite> SocialMediaSite { get; set; }
    public DbSet<UserSocialMediaSites> UserSocialMediaSite { get; set; }
    
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

            entity.HasMany(e => e.MentorshipsAsMentee)
                .WithOne(m => m.Mentee)
                .HasForeignKey(m => m.MenteeId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure SpeakerType entity
        builder.Entity<SpeakerType>(entity =>
        {
            entity.ToTable("SpeakerTypes");
            entity.HasIndex(e => e.Name)
                .IsUnique();

            entity.Property(e => e.Name)
                .IsRequired();
        });

        // Configure Expertise entity
        builder.Entity<Expertise>(entity =>
        {
            entity.ToTable("Expertises");
            entity.HasIndex(e => e.Name)
                .IsUnique();

            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(e => e.ExpertiseCategoryId);

            entity.HasOne(e => e.ExpertiseCategory)
                .WithMany(c => c.Expertises)
                .HasForeignKey(e => e.ExpertiseCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        builder.Entity<Sector>(entity =>
        {
            entity.ToTable("Sectors");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Slug)
                .HasMaxLength(500);

            entity.Property(e => e.Description)
                .HasMaxLength(255);

            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);

            entity.HasIndex(e => e.Name).IsUnique();
            entity.HasIndex(e => e.Slug).IsUnique();
        });

        // Configure ExpertiseCategory entity
        builder.Entity<ExpertiseCategory>(entity =>
        {
            entity.ToTable("ExpertiseCategories");
            entity.HasIndex(e => e.Name).IsUnique();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasIndex(e => e.SectorId);

            entity.HasOne(e => e.Sector)
                .WithMany(s => s.ExpertiseCategories)
                .HasForeignKey(e => e.SectorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure UserExpertise many-to-many relationship
        builder.Entity<UserExpertise>(entity =>
        {
            entity.ToTable("UserExpertises");
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
        
        // Configure SocialMediaSite entity
        builder.Entity<SocialMediaSite>(entity =>
        {
            entity.ToTable("SocialMediaSites");
            
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Icon).IsRequired();
            entity.Property(e => e.UrlFormat).IsRequired();
            
            entity.HasIndex(e => e.Name)
                .IsUnique();
            
        });
        
        // Configure UserSocialMediaSite many-to-many relationship
        builder.Entity<UserSocialMediaSites>(entity =>
        {
            entity.ToTable("UserSocialMediaSites");

            entity.Property(usm => usm.SocialId).IsRequired();
            entity.HasIndex(usm => new { usm.UserId });
            
            entity.HasOne(usm => usm.User)
                .WithMany(u => u.UserSocialMediaSites)
                .HasForeignKey(usm => usm.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(usm => usm.SocialMediaSite)
                .WithMany()
                .HasForeignKey(usm => usm.SocialMediaSiteId)
                .OnDelete(DeleteBehavior.Cascade);
                
        });

        // Configure Mentorship entity
        builder.Entity<Mentorship>(entity =>
        {
            entity.ToTable("Mentorships");
            entity.Property(e => e.Id)
                .HasDefaultValueSql("NEWID()");

            entity.Property(e => e.RequestedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.Status)
                .HasConversion<string>()
                .HasDefaultValue(MentorshipStatus.Pending);

            entity.Property(e => e.Type)
                .HasConversion<string>()
                .HasDefaultValue(MentorshipType.NewToExperienced);

            entity.HasIndex(e => e.MenteeId);
            entity.HasIndex(e => e.MentorId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.Status, e.RequestedAt });

            entity.ToTable(t => t.HasCheckConstraint(
                "CHK_Mentorships_DifferentUsers",
                "[MenteeId] != [MentorId]"));
        });

        // Configure MentorshipExpertise many-to-many relationship
        builder.Entity<MentorshipExpertise>(entity =>
        {
            entity.ToTable("MentorshipExpertises");
            entity.HasKey(e => new { e.MentorshipId, e.ExpertiseId });

            entity.HasOne(e => e.Mentorship)
                .WithMany(m => m.FocusAreas)
                .HasForeignKey(e => e.MentorshipId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Expertise)
                .WithMany()
                .HasForeignKey(e => e.ExpertiseId)
                .OnDelete(DeleteBehavior.Cascade);
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

        foreach (var entry in entries) entry.Entity.UpdatedDate = DateTime.UtcNow;
    }
}