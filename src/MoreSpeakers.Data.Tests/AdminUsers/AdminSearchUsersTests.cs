using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;

using MoreSpeakers.Data;
using MoreSpeakers.Data.Models;
using MoreSpeakers.Domain.Models.AdminUsers;
using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Data.Tests.AdminUsers;

public class AdminSearchUsersTests
{
    private static UserDataStore CreateStore(out MoreSpeakersDbContext ctx)
    {
        var options = new DbContextOptionsBuilder<MoreSpeakersDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ctx = new MoreSpeakersDbContext(options);

        // Minimal dependencies (not used by methods under test)
        var userStore = new Mock<IUserStore<User>>();
        var userManager = new UserManager<User>(userStore.Object, null, null, null, null, null, null, null, null);

        var mapper = new Mock<AutoMapper.IMapper>().Object;
        var amSettings = new Mock<IAutoMapperSettings>().Object;
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<UserDataStore>>().Object;
        var loggerFactory = new Mock<Microsoft.Extensions.Logging.ILoggerFactory>().Object;

        return new UserDataStore(ctx, userManager, mapper, amSettings, logger, loggerFactory);
    }

    private static (User u, string roleName) AddUser(MoreSpeakersDbContext ctx, string email, string userName,
        bool emailConfirmed = true, bool lockedOut = false, string? role = null, DateTime? created = null)
    {
        var u = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            EmailConfirmed = emailConfirmed,
            LockoutEnabled = true,
            LockoutEnd = lockedOut ? DateTimeOffset.UtcNow.AddHours(1) : (DateTimeOffset?)null,
            CreatedDate = (created ?? DateTime.UtcNow).ToUniversalTime(),
            UpdatedDate = DateTime.UtcNow
        };
        ctx.Users.Add(u);

        string assignedRole = string.Empty;
        if (!string.IsNullOrWhiteSpace(role))
        {
            var existingRole = ctx.Roles.FirstOrDefault(r => r.Name == role);
            if (existingRole == null)
            {
                existingRole = new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = role, NormalizedName = role.ToUpperInvariant() };
                ctx.Roles.Add(existingRole);
            }

            ctx.UserRoles.Add(new IdentityUserRole<Guid>
            {
                RoleId = existingRole.Id,
                UserId = u.Id
            });
            assignedRole = existingRole.Name!;
        }

        return (u, assignedRole);
    }

    [Fact]
    public async Task Search_ByEmail_And_UserName_Works()
    {
        var store = CreateStore(out var ctx);

        AddUser(ctx, "alice@example.com", "alice", emailConfirmed: true, lockedOut: false, role: "Administrator");
        AddUser(ctx, "bob@example.com", "bobby", emailConfirmed: false, lockedOut: false, role: "Moderator");
        AddUser(ctx, "carol@test.com", "carol", emailConfirmed: true, lockedOut: true, role: null);
        await ctx.SaveChangesAsync();

        // Search by email substring
        var result1 = await store.AdminSearchUsersAsync(new UserAdminFilter { Query = "example" },
            new UserAdminSort { By = UserAdminSortBy.Email, Direction = SortDirection.Asc }, 1, 20);
        Assert.Equal(2, result1.TotalCount);

        // Search by username substring
        var result2 = await store.AdminSearchUsersAsync(new UserAdminFilter { Query = "rol" },
            new UserAdminSort { By = UserAdminSortBy.Email, Direction = SortDirection.Asc }, 1, 20);
        Assert.Equal(1, result2.TotalCount);
        Assert.Single(result2.Items);
        Assert.Equal("carol@test.com", result2.Items[0].Email);
    }

    [Fact]
    public async Task Filters_EmailConfirmed_And_Lockout_TriState()
    {
        var store = CreateStore(out var ctx);

        AddUser(ctx, "a@x.com", "a", emailConfirmed: true, lockedOut: false);
        AddUser(ctx, "b@x.com", "b", emailConfirmed: false, lockedOut: false);
        AddUser(ctx, "c@x.com", "c", emailConfirmed: true, lockedOut: true);
        await ctx.SaveChangesAsync();

        var onlyConfirmed = await store.AdminSearchUsersAsync(new UserAdminFilter { EmailConfirmed = TriState.True },
            new UserAdminSort(), 1, 50);
        Assert.Equal(2, onlyConfirmed.TotalCount);
        Assert.All(onlyConfirmed.Items, i => Assert.True(i.EmailConfirmed));

        var onlyUnconfirmed = await store.AdminSearchUsersAsync(new UserAdminFilter { EmailConfirmed = TriState.False },
            new UserAdminSort(), 1, 50);
        Assert.Equal(1, onlyUnconfirmed.TotalCount);
        Assert.All(onlyUnconfirmed.Items, i => Assert.False(i.EmailConfirmed));

        var onlyLocked = await store.AdminSearchUsersAsync(new UserAdminFilter { LockedOut = TriState.True },
            new UserAdminSort(), 1, 50);
        Assert.Equal(1, onlyLocked.TotalCount);
        Assert.All(onlyLocked.Items, i => Assert.True(i.IsLockedOut));

        var onlyNotLocked = await store.AdminSearchUsersAsync(new UserAdminFilter { LockedOut = TriState.False },
            new UserAdminSort(), 1, 50);
        Assert.Equal(2, onlyNotLocked.TotalCount);
        Assert.All(onlyNotLocked.Items, i => Assert.False(i.IsLockedOut));
    }

    [Fact]
    public async Task Role_Filter_Returns_Only_Users_In_Role()
    {
        var store = CreateStore(out var ctx);

        AddUser(ctx, "admin1@x.com", "admin1", role: "Administrator");
        AddUser(ctx, "admin2@x.com", "admin2", role: "Administrator");
        AddUser(ctx, "mod1@x.com", "mod1", role: "Moderator");
        AddUser(ctx, "user1@x.com", "user1", role: null);
        await ctx.SaveChangesAsync();

        var res = await store.AdminSearchUsersAsync(new UserAdminFilter { RoleName = "Administrator" }, new UserAdminSort(), 1, 20);
        Assert.Equal(2, res.TotalCount);
        Assert.All(res.Items, i => Assert.Equal("Administrator", i.Role));
    }

    [Fact]
    public async Task Sorting_And_Pagination_Work()
    {
        var store = CreateStore(out var ctx);

        // Create 30 users with varying emails and created dates
        for (int i = 0; i < 30; i++)
        {
            var email = $"user{i:00}@example.com";
            var username = $"user{i:00}";
            var created = DateTime.UtcNow.AddDays(-i);
            AddUser(ctx, email, username, emailConfirmed: i % 2 == 0, lockedOut: i % 3 == 0, created: created);
        }
        await ctx.SaveChangesAsync();

        // Sort by Email descending, take page 2 (page size 10)
        var sort = new UserAdminSort { By = UserAdminSortBy.Email, Direction = SortDirection.Desc };
        var page2 = await store.AdminSearchUsersAsync(new UserAdminFilter(), sort, 2, 10);
        Assert.Equal(30, page2.TotalCount);
        Assert.Equal(10, page2.Items.Count);
        // Verify first item on page 2 is the 11th email in descending order
        var allDesc = (await store.AdminSearchUsersAsync(new UserAdminFilter(), sort, 1, 100)).Items.Select(i => i.Email).ToList();
        Assert.Equal(allDesc.Skip(10).First(), page2.Items.First().Email);

        // Sort by CreatedUtc ascending
        var byCreatedAsc = await store.AdminSearchUsersAsync(new UserAdminFilter(),
            new UserAdminSort { By = UserAdminSortBy.CreatedUtc, Direction = SortDirection.Asc }, 1, 5);
        Assert.Equal(5, byCreatedAsc.Items.Count);
        var createdDates = byCreatedAsc.Items.Select(i => i.CreatedUtc).ToList();
        Assert.True(createdDates.SequenceEqual(createdDates.OrderBy(d => d)));
    }

    [Fact]
    public async Task GetAllRoleNamesAsync_Returns_Sorted_Roles()
    {
        var store = CreateStore(out var ctx);

        // Seed roles only
        ctx.Roles.AddRange(new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "Moderator", NormalizedName = "MODERATOR" },
            new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "Administrator", NormalizedName = "ADMINISTRATOR" },
            new IdentityRole<Guid> { Id = Guid.NewGuid(), Name = "User", NormalizedName = "USER" });
        await ctx.SaveChangesAsync();

        var roles = await store.GetAllRoleNamesAsync();
        Assert.Equal(new[] { "Administrator", "Moderator", "User" }, roles);
    }
}
