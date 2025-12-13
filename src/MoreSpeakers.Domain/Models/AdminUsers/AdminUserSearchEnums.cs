using System.ComponentModel;

namespace MoreSpeakers.Domain.Models.AdminUsers;

public enum TriState
{
    Any = 0,
    True = 1,
    False = 2
}

public enum UserAdminSortBy
{
    [Description("Email")]
    Email = 0,
    [Description("User Namer")]
    UserName = 1,
    [Description("Email Confirmed")]
    EmailConfirmed = 2,
    [Description("Account Locked")]
    LockedOut = 3,
    [Description("Role")]
    Role = 4,
    [Description("Create  Date")]
    CreatedUtc = 5,
    [Description("Last signed In")]
    LastSignInUtc = 6
}