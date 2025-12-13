namespace MoreSpeakers.Domain.Models.AdminUsers;

public enum TriState
{
    Any = 0,
    True = 1,
    False = 2
}

public enum UserAdminSortBy
{
    Email = 0,
    UserName = 1,
    EmailConfirmed = 2,
    LockedOut = 3,
    Role = 4,
    CreatedUtc = 5,
    LastSignInUtc = 6
}