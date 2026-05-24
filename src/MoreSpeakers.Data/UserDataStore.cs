using System.Security.Claims;

using AutoMapper;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using MoreSpeakers.Data.Models;
using MoreSpeakers.Domain;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models.AdminUsers;

using SpeakerType = MoreSpeakers.Domain.Models.SpeakerType;
using User = MoreSpeakers.Domain.Models.User;
using UserExpertise = MoreSpeakers.Domain.Models.UserExpertise;

namespace MoreSpeakers.Data;

public partial class UserDataStore : IUserDataStore
{
    private readonly MoreSpeakersDbContext _context;
    private readonly UserManager<Data.Models.User> _userManager;
    private readonly IMapper _mapper;
    private readonly IAutoMapperSettings _autoMapperSettings;
    private readonly ILogger<UserDataStore> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ISettings _settings;

    public UserDataStore(MoreSpeakersDbContext context, UserManager<Data.Models.User> userManager, IMapper mapper, IAutoMapperSettings autoMapperSettings, ILogger<UserDataStore> logger, ILoggerFactory loggerFactory, ISettings settings)
    {
        _context = context;
        _mapper = mapper;
        _autoMapperSettings = autoMapperSettings;
        _userManager = userManager;
        _logger = logger;
        _loggerFactory = loggerFactory;
        _settings = settings;
    }

    // ------------------------------------------
    // Wrapper methods for AspNetCore Identity
    // ------------------------------------------

    public async Task<User?> GetUserAsync(ClaimsPrincipal user)
    {
        var identityUserByClaim = await _userManager.GetUserAsync(user);
        return _mapper.Map<User>(identityUserByClaim);
    }

    public async Task<IdentityResult> ChangePasswordAsync(User user, string currentPassword, string newPassword)
    {
        try
        {
            // Need to load the user from the "Identity" manager to change the password
            var identityUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (identityUser == null)
            {
                LogCouldNotFindUserWithIdUseridInTheIdentityDatabase(user.Id);
                throw new InvalidOperationException(
                    $"Could not find user with id: {user.Id} in the Identity database.");
            }

            var result = await _userManager.ChangePasswordAsync(identityUser, currentPassword, newPassword);
            if (!result.Succeeded)
            {
                LogFailedToChangePasswordForUserWithIdUserid(user.Id);
            }
            return result;
        }
        catch (Exception ex)
        {
            LogFailedToChangePasswordForUserWithIdUserid(ex, user.Id);
            return IdentityResult.Failed();
        }
    }

    public async Task<IdentityResult> CreateAsync(User user, string password)
    {
        var identityUser = _mapper.Map<Data.Models.User>(user);

        try
        {
            var result = await _userManager.CreateAsync(identityUser, password);
            if (!result.Succeeded)
            {
                LogFailedToCreateUserWithIdUserid(user.Id);
            }
            return result;
        }
        catch (Exception ex)
        {
            LogFailedToCreateUserWithIdUserid(ex, user.Id);
            return IdentityResult.Failed();
        }
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        var user = await _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Include(u => u.UserSocialMediaSites)
            .ThenInclude(sms => sms.SocialMediaSite)
            .FirstOrDefaultAsync(e => e.Email == email);
        return _mapper.Map<User?>(user);
    }

    public async Task<User?> GetUserIdAsync(ClaimsPrincipal user)
    {
        var identityUser = await _userManager.GetUserAsync(user);
        return _mapper.Map<User>(identityUser);
    }

    // ------------------------------------------
    // Admin Users (List/Search)
    // ------------------------------------------

    public async Task<Result<PagedResult<UserListRow>>> AdminSearchUsersAsync(UserAdminFilter filter, UserAdminSort sort, int page, int pageSize)
    {
        if (page < 1) page = _settings.Pagination.StartPage;
        if (pageSize < 1) pageSize = _settings.Pagination.MinimalPageSize;
        if (pageSize > 100) pageSize = _settings.Pagination.MaximizePageSize;

        var now = DateTimeOffset.UtcNow;

        var users = _context.Users.AsNoTracking().AsQueryable();

        // Search (email or username)
        if (!string.IsNullOrWhiteSpace(filter.Query))
        {
            var q = filter.Query.Trim();
            var qLower = q.ToLower();
#pragma warning disable CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
            users = users.Where(u => (u.Email != null && u.Email.ToLower().Contains(qLower)) ||
                                     (u.UserName != null && u.UserName.ToLower().Contains(qLower)));
#pragma warning restore CA1862 // Use the 'StringComparison' method overloads to perform case-insensitive string comparisons
        }

        // Email confirmed tri-state
        users = filter.EmailConfirmed switch
        {
            TriState.True => users.Where(u => u.EmailConfirmed),
            TriState.False => users.Where(u => !u.EmailConfirmed),
            _ => users
        };

        // Locked out tri-state
        users = filter.LockedOut switch
        {
            TriState.True => users.Where(u => u.LockoutEnabled && u.LockoutEnd != null && u.LockoutEnd > now),
            TriState.False => users.Where(u => !u.LockoutEnabled || u.LockoutEnd == null || u.LockoutEnd <= now),
            _ => users
        };

        // Deleted tri-state
        users = filter.IsDeleted switch
        {
            TriState.True => users.Where(u => u.IsDeleted),
            TriState.False => users.Where(u => !u.IsDeleted),
            _ => users
        };

        // Role filter via join
        if (!string.IsNullOrWhiteSpace(filter.RoleName))
        {
            var roleName = filter.RoleName.Trim();
            var roleIds = _context.Roles
                .AsNoTracking()
                .Where(r => r.Name != null && r.Name == roleName)
                .Select(r => r.Id);

            var userIdsInRole = _context.UserRoles
                .AsNoTracking()
                .Where(ur => roleIds.Contains(ur.RoleId))
                .Select(ur => ur.UserId);

            users = users.Where(u => userIdsInRole.Contains(u.Id));
        }

        // Total count after filters
        var totalCount = await users.CountAsync();

        // Build projection with computed fields used for sorting and display
        var projected = from u in users
                        select new AdminUser
                        {
                            Id = u.Id,
                            Email = u.Email,
                            UserName = u.UserName,
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            EmailConfirmed = u.EmailConfirmed,
                            IsLockedOut = u.LockoutEnabled && u.LockoutEnd != null && u.LockoutEnd > now,
                            IsDeleted = u.IsDeleted,
                            Role = (from ur in _context.UserRoles
                                    join r in _context.Roles on ur.RoleId equals r.Id
                                    where ur.UserId == u.Id
                                    orderby r.Name
                                    select r.Name).FirstOrDefault(),
                            // CreatedDate in DB is stored in UTC (default GETUTCDATE()).
                            // Avoid non-translatable DateTime.SpecifyKind; simple cast translates in EF.
                            CreatedUtc = u.CreatedDate,
                            LastSignInUtc = null
                        };

        // Sorting
        IOrderedQueryable<AdminUser> ordered = sort.By switch
        {
            UserAdminSortBy.UserName => sort.Direction == SortDirection.Asc
                                ? projected.OrderBy(x => x.UserName).ThenBy(x => x.Email).ThenBy(x => x.Id)
                                : projected.OrderByDescending(x => x.UserName).ThenBy(x => x.Email).ThenBy(x => x.Id),
            UserAdminSortBy.EmailConfirmed => sort.Direction == SortDirection.Asc
                                ? projected.OrderBy(x => x.EmailConfirmed).ThenBy(x => x.Email).ThenBy(x => x.Id)
                                : projected.OrderByDescending(x => x.EmailConfirmed).ThenBy(x => x.Email).ThenBy(x => x.Id),
            UserAdminSortBy.LockedOut => sort.Direction == SortDirection.Asc
                                ? projected.OrderBy(x => x.IsLockedOut).ThenBy(x => x.Email).ThenBy(x => x.Id)
                                : projected.OrderByDescending(x => x.IsLockedOut).ThenBy(x => x.Email).ThenBy(x => x.Id),
            UserAdminSortBy.Role => sort.Direction == SortDirection.Asc
                                ? projected.OrderBy(x => x.Role).ThenBy(x => x.Email).ThenBy(x => x.Id)
                                : projected.OrderByDescending(x => x.Role).ThenBy(x => x.Email).ThenBy(x => x.Id),
            UserAdminSortBy.CreatedUtc => sort.Direction == SortDirection.Asc
                                ? projected.OrderBy(x => x.CreatedUtc).ThenBy(x => x.Email).ThenBy(x => x.Id)
                                : projected.OrderByDescending(x => x.CreatedUtc).ThenBy(x => x.Email).ThenBy(x => x.Id),
            UserAdminSortBy.LastSignInUtc => sort.Direction == SortDirection.Asc
                                ? projected.OrderBy(x => x.LastSignInUtc).ThenBy(x => x.Email).ThenBy(x => x.Id)
                                : projected.OrderByDescending(x => x.LastSignInUtc).ThenBy(x => x.Email).ThenBy(x => x.Id),
            UserAdminSortBy.Deleted => sort.Direction == SortDirection.Asc
                                ? projected.OrderBy(x => x.IsDeleted).ThenBy(x => x.Email).ThenBy(x => x.Id)
                                : projected.OrderByDescending(x => x.IsDeleted).ThenBy(x => x.Email).ThenBy(x => x.Id),
            _ => sort.Direction == SortDirection.Asc
                                ? projected.OrderBy(x => x.Email).ThenBy(x => x.Id)
                                : projected.OrderByDescending(x => x.Email).ThenBy(x => x.Id),
        };
        var pageItems = await ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserListRow
            {
                Id = x.Id,
                Email = x.Email!,
                FirstName = x.FirstName ?? string.Empty,
                LastName = x.LastName ?? string.Empty,
                UserName = x.UserName!,
                EmailConfirmed = x.EmailConfirmed,
                IsLockedOut = x.IsLockedOut,
                IsDeleted = x.IsDeleted,
                Role = x.Role,
                CreatedUtc = x.CreatedUtc,
                LastSignInUtc = x.LastSignInUtc
            })
            .ToListAsync();

        return Result.Success(new PagedResult<UserListRow>
        {
            Items = pageItems,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<Result<IReadOnlyList<string>>> GetAllRoleNamesAsync()
    {
        var roles = await _context.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => r.Name!)
            .ToListAsync();
        return Result.Success<IReadOnlyList<string>>(roles);
    }

    public async Task<Result<IReadOnlyList<string>>> GetRolesForUserAsync(Guid userId)
    {
        // Return distinct non-null role names assigned to the user, ordered by name
        var roleNames = await (from ur in _context.UserRoles.AsNoTracking()
                               join r in _context.Roles.AsNoTracking() on ur.RoleId equals r.Id
                               where ur.UserId == userId && r.Name != null
                               orderby r.Name
                               select r.Name!)
            .ToListAsync();

        return Result.Success<IReadOnlyList<string>>(roleNames);
    }

    public async Task<IdentityResult> AddToRolesAsync(Guid userId, IEnumerable<string> roles)
    {
        try
        {
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            return identityUser == null
                ? IdentityResult.Failed(new IdentityError { Description = "User not found" })
                : await _userManager.AddToRolesAsync(identityUser, roles);
        }
        catch (Exception ex)
        {
            LogFailedToAddRolesToUserUserid(userId);
            return IdentityResult.Failed(new IdentityError { Description = ex.Message });
        }
    }

    public async Task<IdentityResult> RemoveFromRolesAsync(Guid userId, IEnumerable<string> roles)
    {
        try
        {
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            return identityUser == null
                ? IdentityResult.Failed(new IdentityError { Description = "User not found" })
                : await _userManager.RemoveFromRolesAsync(identityUser, roles);
        }
        catch (Exception ex)
        {
            LogFailedToRemoveRolesFromUserUserid(userId);
            return IdentityResult.Failed(new IdentityError { Description = ex.Message });
        }
    }

    private sealed class AdminUser
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool IsLockedOut { get; set; }
        public bool IsDeleted { get; set; }
        public string? Role { get; set; }
        public DateTimeOffset? CreatedUtc { get; set; }
        public DateTimeOffset? LastSignInUtc { get; set; }
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
    {
        var identityUser = _mapper.Map<Data.Models.User>(user);
        return await _userManager.GenerateEmailConfirmationTokenAsync(identityUser);
    }

    public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
    {
        var identityUser = await _userManager.FindByIdAsync(user.Id.ToString());

        return identityUser == null
            ? IdentityResult.Failed(new IdentityError { Description = "User not found" })
            : await _userManager.ConfirmEmailAsync(identityUser, token);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(User user)
    {
        var identityUser = await _userManager.FindByIdAsync(user.Id.ToString());
        return identityUser == null
            ? string.Empty
            : await _userManager.GeneratePasswordResetTokenAsync(identityUser);
    }

    public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword)
    {
        var identityUser = await _userManager.FindByIdAsync(user.Id.ToString());
        return identityUser == null
            ? IdentityResult.Failed(new IdentityError { Description = "User not found" })
            : await _userManager.ResetPasswordAsync(identityUser, token, newPassword);
    }

    // Passkey Support
    public async Task<IdentityResult> AddOrUpdatePasskeyAsync(User user, UserPasskeyInfo passkey)
    {
        var identityUser = await _userManager.FindByIdAsync(user.Id.ToString());
        return identityUser == null
            ? IdentityResult.Failed(new IdentityError { Description = "User not found" })
            : await _userManager.AddOrUpdatePasskeyAsync(identityUser, passkey);
    }

    public async Task<Result<IEnumerable<UserPasskey>>> GetUserPasskeysAsync(Guid userId)
    {
        // Query standard IdentityUserPasskey table directly to list keys for management UI
        // This avoids needing a custom table while still leveraging standard Identity storage
        var passkeys = await _context.Set<IdentityUserPasskey<Guid>>()
            .Where(p => p.UserId == userId)
            .ToListAsync();

        var results = new List<UserPasskey>();

        foreach (var pk in passkeys)
        {
            // pk.Data contains metadata in .NET 10 Identity
            // We access the Name property directly
            var friendlyName = pk.Data.Name ?? "Passkey";

            // Convert CredentialId bytes to Base64Url safe string for ID
            var credentialIdBase64 = Convert.ToBase64String(pk.CredentialId)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');

            results.Add(new UserPasskey
            {
                Id = credentialIdBase64,
                UserId = pk.UserId,
                FriendlyName = friendlyName
            });
        }

        return Result.Success<IEnumerable<UserPasskey>>(results);
    }

    // ------------------------------------------
    // Admin Users (Lock/Unlock)
    // ------------------------------------------

    public async Task<Result> EnableLockoutAsync(Guid userId, bool enabled)
    {
        try
        {
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            if (identityUser == null)
            {
                LogAdminlockoutEnablelockoutFailedUserUseridNotFound(userId);
                return Failure("user.not-found", $"User {userId} not found.");
            }

            var result = await _userManager.SetLockoutEnabledAsync(identityUser, enabled);
            if (!result.Succeeded)
            {
                LogAdminlockoutSetlockoutenabledasyncFailedForUseridErrors(userId, string.Join(",", result.Errors.Select(e => e.Code)));
                return Failure("user.lockout.enable-failed", string.Join(",", result.Errors.Select(e => e.Description)));
            }

            LogAdminlockoutLockoutenabledSetToEnabledForUserUserid(enabled, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            LogAdminlockoutEnablelockoutasyncExceptionForUserUserid(ex, userId);
            return Failure("user.lockout.enable-failed", "An error occurred enabling lockout.", ex);
        }
    }

    public async Task<Result> SetLockoutEndAsync(Guid userId, DateTimeOffset? lockoutEndUtc)
    {
        try
        {
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            if (identityUser == null)
            {
                LogAdminlockoutSetlockoutendFailedUserUseridNotFound(userId);
                return Failure("user.not-found", $"User {userId} not found.");
            }

            DateTimeOffset? normalized = lockoutEndUtc?.ToUniversalTime();
            if (normalized.HasValue && normalized.Value <= DateTimeOffset.UtcNow)
            {
                LogAdminlockoutRejectedSetlockoutendWithPastNowValueForUseridLockoutend(userId, lockoutEndUtc);
                return Failure("user.lockout.end-in-past", "Lockout end date must be in the future.");
            }

            var result = await _userManager.SetLockoutEndDateAsync(identityUser, normalized);
            if (!result.Succeeded)
            {
                LogAdminlockoutSetlockoutenddateasyncFailedForUseridErrors(userId, string.Join(",", result.Errors.Select(e => e.Code)));
                return Failure("user.lockout.set-end-failed", string.Join(",", result.Errors.Select(e => e.Description)));
            }

            LogAdminlockoutLockoutendSetToLockoutendForUserUserid(normalized, userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            LogAdminlockoutSetlockoutendasyncExceptionForUserUserid(ex, userId);
            return Failure("user.lockout.set-end-failed", "An error occurred setting lockout end.", ex);
        }
    }

    public async Task<Result> UnlockAsync(Guid userId)
    {
        try
        {
            var identityUser = await _userManager.FindByIdAsync(userId.ToString());
            if (identityUser == null)
            {
                LogAdminlockoutUnlockFailedUserUseridNotFound(userId);
                return Failure("user.not-found", $"User {userId} not found.");
            }

            var endResult = await _userManager.SetLockoutEndDateAsync(identityUser, null);
            if (!endResult.Succeeded)
            {
                LogAdminlockoutClearingLockoutendFailedForUseridErrors(userId, string.Join(",", endResult.Errors.Select(e => e.Code)));
                return Failure("user.lockout.clear-failed", string.Join(",", endResult.Errors.Select(e => e.Description)));
            }

            var resetResult = await _userManager.ResetAccessFailedCountAsync(identityUser);
            if (!resetResult.Succeeded)
            {
                LogAdminlockoutResetaccessfailedcountFailedForUseridErrors(userId, string.Join(",", resetResult.Errors.Select(e => e.Code)));
                return Failure("user.lockout.reset-failed", string.Join(",", resetResult.Errors.Select(e => e.Description)));
            }

            LogAdminlockoutUserUseridUnlocked(userId);
            return Result.Success();
        }
        catch (Exception ex)
        {
            LogAdminlockoutUnlockasyncExceptionForUserUserid(ex, userId);
            return Failure("user.lockout.unlock-failed", "An error occurred unlocking user.", ex);
        }
    }

    public async Task<Result<int>> GetUserCountInRoleAsync(string roleName)
    {
        try
        {
            var count = await (from ur in _context.UserRoles.AsNoTracking()
                               join r in _context.Roles.AsNoTracking() on ur.RoleId equals r.Id
                               join u in _context.Users.AsNoTracking() on ur.UserId equals u.Id
                               where r.Name == roleName && !u.IsDeleted
                               select ur.UserId)
                .Distinct()
                .CountAsync();

            return Result.Success(count);
        }
        catch (Exception ex)
        {
            LogAdminlockoutGetusercountinroleasyncFailedForRoleRole(ex, roleName);
            return Failure<int>("user.role.count-failed", "Failed to get user count for role.", ex);
        }
    }

    // ------------------------------------------
    // Admin Users (Soft/Hard Delete)
    // ------------------------------------------

    public async Task<Result> SoftDeleteAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Failure("user.not-found", $"User {userId} not found.");
            }

            user.IsDeleted = true;
            user.DeletedAt = DateTimeOffset.UtcNow;
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to soft delete user {UserId}", userId);
            return Failure("user.soft-delete.failed", "Failed to soft delete user.", ex);
        }
    }

    public async Task<Result> RestoreAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Failure("user.not-found", $"User {userId} not found.");
            }

            user.IsDeleted = false;
            user.DeletedAt = null;
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore user {UserId}", userId);
            return Failure("user.restore.failed", "Failed to restore user.", ex);
        }
    }

    public async Task<Result> RemovePasskeyAsync(Guid userId, byte[] credentialId)
    {
        try
        {
            var passkey = await _context.Set<IdentityUserPasskey<Guid>>()
                .FirstOrDefaultAsync(p => p.UserId == userId && p.CredentialId == credentialId);

            if (passkey == null)
            {
                return Result.Success();
            }

            _context.Set<IdentityUserPasskey<Guid>>().Remove(passkey);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            LogFailedToRemovePasskeyForUserUserid(ex, userId);
            return Failure("user.passkey.remove-failed", "Failed to remove passkey.", ex);
        }
    }

    // ------------------------------------------
    // Application Methods
    // ------------------------------------------

    public async Task<Result<User>> GetAsync(Guid primaryKey)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.SpeakerType)
                .Include(u => u.UserExpertise)
                .ThenInclude(ue => ue.Expertise)
                .Include(u => u.UserSocialMediaSites)
                .ThenInclude(sms => sms.SocialMediaSite)
                .FirstOrDefaultAsync(e => e.Id == primaryKey);

            if (user == null)
            {
                return Failure<User>("user.not-found", $"User {primaryKey} not found.");
            }

            return Result.Success(_mapper.Map<User>(user));
        }
        catch (Exception ex)
        {
            return Failure<User>("user.get.failed", "Failed to retrieve user.", ex);
        }
    }

    public async Task<Result<User>> SaveAsync(User user)
    {
        try
        {
            var saved = (user.Id == Guid.Empty) ? await AddUser(user) : await UpdateUser(user);
            return Result.Success(saved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save user {UserId}", user.Id);
            return Failure<User>("user.save.failed", "Failed to save user.", ex);
        }
    }

    private async Task<User> AddUser(User user)
    {
        try
        {
            var dbUser = _mapper.Map<Models.User>(user);
            _context.Entry(dbUser).State = EntityState.Added;

            // Save the user expertise
            foreach (var expertise in user.UserExpertise)
            {
                var dbUserExpertise = _mapper.Map<Models.UserExpertise>(expertise);
                _context.UserExpertise.Add(dbUserExpertise);
            }

            // Save the user social media sites
            foreach (var socialMediaSite in user.UserSocialMediaSites)
            {
                var dbUserSocialMediaSite = _mapper.Map<UserSocialMediaSites>(socialMediaSite);
                _context.UserSocialMediaSite.Add(dbUserSocialMediaSite);
            }

            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return _mapper.Map<User>(dbUser);
            }
        }
        catch (Exception ex)
        {
            LogFailedToAddTheNewUser(ex);
        }
        throw new ApplicationException("Failed to add the new user");
    }

    private async Task<User> UpdateUser(User user)
    {
        try
        {
            var dbUser = await _context.Users
                .Include(u => u.SpeakerType)
                .Include(u => u.UserExpertise)
                .ThenInclude(ue => ue.Expertise)
                .Include(u => u.UserSocialMediaSites)
                .ThenInclude(sms => sms.SocialMediaSite)
                .FirstOrDefaultAsync(e => e.Id == user.Id);

            if (dbUser == null)
            {
                user.Id = Guid.NewGuid();
                return await AddUser(user);
            }

            // Custom mapping is needed to handle the UserExpertise and UserSocialMediaSites collections.
            // Entity Framework Core does not support updating collections.
            var map = new MapperConfiguration(cfg =>
            {
                cfg.LicenseKey = _autoMapperSettings.LicenseKey;
                cfg.CreateMap<User, Data.Models.User>()
                    .ForMember(d => d.UserExpertise, opt => opt.Ignore())
                    .ForMember(d => d.UserSocialMediaSites, opt => opt.Ignore());
                cfg.CreateMap<Models.MentorshipStatus, Domain.Models.MentorshipStatus>().ReverseMap();
                cfg.CreateMap<Models.MentorshipType, Domain.Models.MentorshipType>().ReverseMap();
                cfg.CreateMap<Models.SpeakerType, SpeakerType>().ReverseMap();
            }, _loggerFactory);
            var mapper = map.CreateMapper();

            mapper.Map(user, dbUser);

            // Expertises
            var dbUserExpertisesIds = dbUser.UserExpertise.Select(ue => ue.ExpertiseId).ToList();
            var userExpertisesIds = user.UserExpertise.Select(ue => ue.ExpertiseId).ToList();

            // Expertises in user.UserExpertises but not in dbUser.UserExpertises (Add)
            var expertisesToAdd = userExpertisesIds.Except(dbUserExpertisesIds);
            foreach (var expertiseId in expertisesToAdd)
            {
                dbUser.UserExpertise.Add(new Data.Models.UserExpertise { UserId = dbUser.Id, ExpertiseId = expertiseId });
            }

            // Expertises in dbUser.UserExpertises but not in user.UserExpertises (Remove)
            var expertisesToRemove = dbUserExpertisesIds.Except(userExpertisesIds);
            foreach (var expertiseId in expertisesToRemove)
            {
                var dbUserExpertise = dbUser.UserExpertise.FirstOrDefault(ue => ue.ExpertiseId == expertiseId);
                if (dbUserExpertise != null)
                {
                    _context.UserExpertise.Remove(dbUserExpertise);
                }
            }

            // Social Media Sites
            var dbUserSocialMediaSitesIds = dbUser.UserSocialMediaSites.Select(ue => ue.SocialMediaSiteId).ToList();
            var userSocialMediaSitesIds = user.UserSocialMediaSites.Select(ue => ue.SocialMediaSiteId).ToList();

            // Social Media Sites in user.UserSocialMediaSites but not in dbUser.UserSocialMediaSites (Add)
            var socialMediaSitesToAdd = userSocialMediaSitesIds.Except(dbUserSocialMediaSitesIds);
            foreach (var socialMediaSiteId in socialMediaSitesToAdd)
            {
                dbUser.UserSocialMediaSites.Add(new UserSocialMediaSites()
                {
                    UserId = dbUser.Id,
                    SocialMediaSiteId = socialMediaSiteId,
                    SocialId = user.UserSocialMediaSites.FirstOrDefault(sms => sms.SocialMediaSiteId == socialMediaSiteId)?.SocialId ?? string.Empty
                });
            }

            // Social Media Sites in dbUser.UserSocialMediaSites but not in user.UserSocialMediaSites (Remove)
            var socialMediaSitesToRemove = dbUserSocialMediaSitesIds.Except(userSocialMediaSitesIds);
            foreach (var socialMediaSiteId in socialMediaSitesToRemove)
            {
                var dbUserSocialMediaSite = dbUser.UserSocialMediaSites.FirstOrDefault(sms => sms.SocialMediaSiteId == socialMediaSiteId);
                if (dbUserSocialMediaSite != null)
                {
                    _context.UserSocialMediaSite.Remove(dbUserSocialMediaSite);
                }
            }

            // Social Media sites that exist in both dbUser.UserSocialMediaSites and user.UserSocialMediaSites
            // Used to see what may need to be updated
            var socialMediaSitesToUpdate = dbUserSocialMediaSitesIds.Intersect(userSocialMediaSitesIds);
            foreach (var socialMediaSiteId in socialMediaSitesToUpdate)
            {
                var dbUserSocialMediaSite = dbUser.UserSocialMediaSites.FirstOrDefault(sms => sms.SocialMediaSiteId == socialMediaSiteId);
                dbUserSocialMediaSite?.SocialId = user.UserSocialMediaSites.FirstOrDefault(sms => sms.SocialMediaSiteId == socialMediaSiteId)?.SocialId ?? string.Empty;
            }

            var result = await _context.SaveChangesAsync() != 0;
            if (result)
            {
                return _mapper.Map<User>(dbUser);
            }

        }
        catch (Exception ex)
        {
            LogFailedToUpdateTheUserId(ex, user.Id);
        }
        throw new ApplicationException($"Failed to update the user '{user.Id}'");
    }

    public async Task<Result<List<User>>> GetAllAsync()
    {
        try
        {
            var speakers = await _context.Users
                .Include(u => u.SpeakerType)
                .Include(u => u.UserExpertise)
                .ThenInclude(ue => ue.Expertise)
                .Include(u => u.UserSocialMediaSites)
                .ThenInclude(sms => sms.SocialMediaSite)
                .ToListAsync();

            return Result.Success(_mapper.Map<List<User>>(speakers));
        }
        catch (Exception ex)
        {
            return Failure<List<User>>("user.get-all.failed", "Failed to retrieve users.", ex);
        }
    }

    public async Task<Result> DeleteAsync(User entity)
    {
        return await DeleteAsync(entity.Id);
    }

    public async Task<Result> DeleteAsync(Guid primaryKey)
    {
        try
        {
            var speaker = await _context.Users
                .Include(u => u.UserExpertise)
                .FirstOrDefaultAsync(e => e.Id == primaryKey);

            if (speaker is null)
            {
                return Result.Success();
            }

            foreach (var userExpertise in speaker.UserExpertise)
            {
                _context.UserExpertise.Remove(userExpertise);
            }

            _context.Users.Remove(speaker);
            await _context.SaveChangesAsync();
            return Result.Success();
        }
        catch (Exception ex)
        {
            LogFailedToDeleteTheUserIdId(ex, primaryKey);
            return Failure("user.delete.failed", "Failed to delete user.", ex);
        }
    }

    public async Task<Result<IEnumerable<User>>> GetNewSpeakersAsync()
    {
        try
        {
            var users = await _context.Users
                .Include(u => u.SpeakerType)
                .Include(u => u.UserExpertise)
                .ThenInclude(ue => ue.Expertise)
                .Include(u => u.UserSocialMediaSites)
                .ThenInclude(sms => sms.SocialMediaSite)
                .Where(u => u.SpeakerType.Id == (int)SpeakerTypeEnum.NewSpeaker)
                .OrderBy(u => u.FirstName)
                .ToListAsync();

            return Result.Success<IEnumerable<User>>(_mapper.Map<IEnumerable<User>>(users));
        }
        catch (Exception ex)
        {
            return Failure<IEnumerable<User>>("user.get-new-speakers.failed", "Failed to retrieve new speakers.", ex);
        }
    }

    public async Task<Result<IEnumerable<User>>> GetExperiencedSpeakersAsync()
    {
        try
        {
            var users = await _context.Users
                .Include(u => u.SpeakerType)
                .Include(u => u.UserExpertise)
                .ThenInclude(ue => ue.Expertise)
                .Include(u => u.UserSocialMediaSites)
                .ThenInclude(sms => sms.SocialMediaSite)
                .Where(u => u.SpeakerType.Id == (int)SpeakerTypeEnum.ExperiencedSpeaker)
                .OrderBy(u => u.FirstName)
                .ToListAsync();

            return Result.Success<IEnumerable<User>>(_mapper.Map<IEnumerable<User>>(users));
        }
        catch (Exception ex)
        {
            return Failure<IEnumerable<User>>("user.get-experienced-speakers.failed", "Failed to retrieve experienced speakers.", ex);
        }
    }

    public async Task<Result<SpeakerSearchResult>> SearchSpeakersAsync(string? searchTerm, int? speakerTypeId = null, List<int>? expertiseIds = null, SpeakerSearchOrderBy sortOrder = SpeakerSearchOrderBy.Name, int? page = null, int? pageSize = null)
    {
        try
        {
            var query = _context.Users
                .Include(u => u.SpeakerType)
                .Include(u => u.UserExpertise)
                .ThenInclude(ue => ue.Expertise)
                .Include(u => u.UserSocialMediaSites)
                .ThenInclude(sms => sms.SocialMediaSite)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u =>
                    u.FirstName.Contains(searchTerm) ||
                    u.LastName.Contains(searchTerm) ||
                    u.Bio.Contains(searchTerm) ||
                    u.UserExpertise.Any(ue => ue.Expertise.Name.Contains(searchTerm)));
            }

            if (speakerTypeId.HasValue)
            {
                query = query.Where(u => u.SpeakerTypeId == speakerTypeId.Value);
            }

            if (expertiseIds != null && expertiseIds.Count != 0)
            {
                foreach (var expertiseId in expertiseIds)
                {
                    query = query.Where(u => u.UserExpertise.Any(ue => ue.ExpertiseId == expertiseId));
                }
            }

            query = query.Where(u => !u.IsDeleted);

            query = sortOrder switch
            {
                SpeakerSearchOrderBy.Newest => query.OrderByDescending(u => u.CreatedDate),
                SpeakerSearchOrderBy.Expertise => query
                    .OrderBy(u => u.UserExpertise.Count)
                    .ThenBy(u => u.LastName)
                    .ThenBy(u => u.FirstName),
                _ => query
                    .OrderBy(u => u.LastName)
                    .ThenBy(u => u.FirstName),
            };

            var users = await query.ToListAsync();
            var totalCount = users.Count;

            if (page.HasValue && pageSize.HasValue)
            {
                users = [.. users.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value)];
            }

            var results = new SpeakerSearchResult
            {
                RowCount = totalCount,
                Speakers = _mapper.Map<IEnumerable<User>>(users),
                PageSize = pageSize ?? totalCount,
                CurrentPage = page ?? 1,
                TotalPages = page.HasValue ? RoundDivide(totalCount, pageSize ?? totalCount) : 1
            };

            return Result.Success(results);
        }
        catch (Exception ex)
        {
            return Failure<SpeakerSearchResult>("user.search.failed", "Failed to search speakers.", ex);
        }
    }

    private static int RoundDivide(int numerator, int denominator)
    {
        return (numerator + (denominator / 2)) / denominator;
    }


    public async Task<Result<IEnumerable<User>>> GetSpeakersByExpertiseAsync(int expertiseId)
    {
        try
        {
            var users = await _context.Users
                .Include(u => u.SpeakerType)
                .Include(u => u.UserExpertise)
                .ThenInclude(ue => ue.Expertise)
                .Where(u => u.UserExpertise.Any(ue => ue.ExpertiseId == expertiseId))
                .OrderBy(u => u.FirstName)
                .ToListAsync();

            return Result.Success<IEnumerable<User>>(_mapper.Map<IEnumerable<User>>(users));
        }
        catch (Exception ex)
        {
            return Failure<IEnumerable<User>>("user.get-by-expertise.failed", "Failed to retrieve speakers by expertise.", ex);
        }
    }

    public async Task<Result> AddUserSocialMediaSiteAsync(Guid userId, UserSocialMediaSite userSocialMediaSite)
    {
        try
        {
            var dbUserSocialMediaSite = _mapper.Map<UserSocialMediaSites>(userSocialMediaSite);
            _context.UserSocialMediaSite.Add(dbUserSocialMediaSite);

            var saved = await _context.SaveChangesAsync() != 0;
            if (!saved)
            {
                LogFailedToAddSocialMediaLinkForUserForIdUseridSocialmediasiteidSocialmediasiteid(userId, userSocialMediaSite.SocialMediaSiteId, userSocialMediaSite.SocialId);
                return Failure("user.social-media.add-failed", "Failed to add social media site.");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            LogFailedToAddSocialMediaLinkForUserForIdUseridSocialmediasiteidSocialmediasiteid(ex, userId, userSocialMediaSite.SocialMediaSiteId, userSocialMediaSite.SocialId);
            return Failure("user.social-media.add-failed", "Failed to add social media site.", ex);
        }
    }

    public async Task<Result> RemoveUserSocialMediaSiteAsync(int userSocialMediaSiteId)
    {
        try
        {
            var userSocialMediaSite = await _context.UserSocialMediaSite.FindAsync(userSocialMediaSiteId);
            if (userSocialMediaSite == null)
            {
                return Result.Success();
            }

            _context.UserSocialMediaSite.Remove(userSocialMediaSite);
            var saved = await _context.SaveChangesAsync() != 0;
            if (!saved)
            {
                LogFailedToRemoveSocialMediaLinkWithIdUsersocialmediasiteid(userSocialMediaSiteId);
                return Failure("user.social-media.remove-failed", "Failed to remove social media site.");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            LogFailedToRemoveSocialMediaLinkWithIdUsersocialmediasiteid(ex, userSocialMediaSiteId);
            return Failure("user.social-media.remove-failed", "Failed to remove social media site.", ex);
        }
    }

    public async Task<Result<IEnumerable<UserSocialMediaSite>>> GetUserSocialMediaSitesAsync(Guid userId)
    {
        try
        {
            var userSocialMediaSitesList = await _context.UserSocialMediaSite
                .Where(sm => sm.UserId == userId)
                .ToListAsync();

            return Result.Success<IEnumerable<UserSocialMediaSite>>(_mapper.Map<List<UserSocialMediaSite>>(userSocialMediaSitesList));
        }
        catch (Exception ex)
        {
            return Failure<IEnumerable<UserSocialMediaSite>>("user.social-media.get-failed", "Failed to retrieve user social media sites.", ex);
        }
    }

    public async Task<Result> AddExpertiseToUserAsync(Guid userId, int expertiseId)
    {
        try
        {
            var userExpertise = new UserExpertise
            {
                UserId = userId,
                ExpertiseId = expertiseId
            };

            var dbUserExperience = _mapper.Map<Models.UserExpertise>(userExpertise);
            _context.UserExpertise.Add(dbUserExperience);
            var saved = await _context.SaveChangesAsync() != 0;
            if (!saved)
            {
                LogFailedToAddExpertiseToUserWithIdUseridExpertiseidExpertiseid(userId, expertiseId);
                return Failure("user.expertise.add-failed", "Failed to add expertise to user.");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            LogFailedToAddExpertiseToUserWithIdUseridExpertiseidExpertiseid(ex, userId, expertiseId);
            return Failure("user.expertise.add-failed", "Failed to add expertise to user.", ex);
        }
    }

    public async Task<Result> RemoveExpertiseFromUserAsync(Guid userId, int expertiseId)
    {
        try
        {
            var userExpertise = await _context.UserExpertise
                .FirstOrDefaultAsync(ue => ue.UserId == userId && ue.ExpertiseId == expertiseId);

            if (userExpertise == null)
            {
                return Result.Success();
            }

            _context.UserExpertise.Remove(userExpertise);
            var saved = await _context.SaveChangesAsync() != 0;
            if (!saved)
            {
                LogFailedToRemoveExpertiseFromUserWithIdUseridExpertiseidExpertiseid(userId, expertiseId);
                return Failure("user.expertise.remove-failed", "Failed to remove expertise from user.");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            LogFailedToRemoveExpertiseFromUserWithIdUseridExpertiseidExpertiseid(ex, userId, expertiseId);
            return Failure("user.expertise.remove-failed", "Failed to remove expertise from user.", ex);
        }
    }

    public async Task<Result<IEnumerable<UserExpertise>>> GetUserExpertisesForUserAsync(Guid userId)
    {
        try
        {
            var userExpertises = await _context.UserExpertise
                .Include(ue => ue.Expertise)
                .Where(ue => ue.UserId == userId)
                .ToListAsync();

            return Result.Success<IEnumerable<UserExpertise>>(_mapper.Map<List<UserExpertise>>(userExpertises));
        }
        catch (Exception ex)
        {
            return Failure<IEnumerable<UserExpertise>>("user.expertise.get-failed", "Failed to retrieve user expertises.", ex);
        }
    }


    public async Task<Result<(int newSpeakers, int experiencedSpeakers, int activeMentorships)>> GetStatisticsForApplicationAsync()
    {
        try
        {
            var newSpeakers = await _context.Users.CountAsync(u => u.SpeakerType.Id == (int)SpeakerTypeEnum.NewSpeaker);
            var experiencedSpeakers = await _context.Users.CountAsync(u => u.SpeakerType.Id == (int)SpeakerTypeEnum.ExperiencedSpeaker);
            var activeMentorships = await _context.Mentorship.CountAsync(m => m.Status == Models.MentorshipStatus.Active);

            return Result.Success((newSpeakers, experiencedSpeakers, activeMentorships));
        }
        catch (Exception ex)
        {
            return Failure<(int newSpeakers, int experiencedSpeakers, int activeMentorships)>("user.statistics.failed", "Failed to retrieve application statistics.", ex);
        }
    }

    public async Task<Result<IEnumerable<User>>> GetFeaturedSpeakersAsync(int count)
    {
        try
        {
            var speakers = await _context.Users
                .Where(s => !string.IsNullOrEmpty(s.Bio) && s.UserExpertise.Any() && s.SpeakerType.Id == (int)SpeakerTypeEnum.ExperiencedSpeaker)
                .OrderBy(u => Guid.NewGuid())
                .Take(count)
                .ToListAsync();

            return Result.Success<IEnumerable<User>>(_mapper.Map<List<User>>(speakers));
        }
        catch (Exception ex)
        {
            return Failure<IEnumerable<User>>("user.featured-speakers.failed", "Failed to retrieve featured speakers.", ex);
        }
    }

    public async Task<Result<IEnumerable<SpeakerType>>> GetSpeakerTypesAsync()
    {
        try
        {
            var speakerTypes = await _context.SpeakerType.ToListAsync();
            return Result.Success<IEnumerable<SpeakerType>>(_mapper.Map<List<SpeakerType>>(speakerTypes));
        }
        catch (Exception ex)
        {
            return Failure<IEnumerable<SpeakerType>>("user.speaker-types.failed", "Failed to retrieve speaker types.", ex);
        }
    }

    private static Result Failure(string code, string message, Exception? ex = null)
        => Result.Failure(new Error(code, message, ex));

    private static Result<T> Failure<T>(string code, string message, Exception? ex = null)
        => Result.Failure<T>(new Error(code, message, ex));
}
