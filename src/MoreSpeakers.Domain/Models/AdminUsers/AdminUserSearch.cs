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

public sealed class UserAdminSort
{
    public UserAdminSortBy By { get; init; } = UserAdminSortBy.Email;
    public SortDirection Direction { get; init; } = SortDirection.Asc;
}

public sealed class UserAdminFilter
{
    public string? Query { get; init; }
    public TriState EmailConfirmed { get; init; } = TriState.Any;
    public TriState LockedOut { get; init; } = TriState.Any;
    public string? RoleName { get; init; }
}

public sealed class UserListRow
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public bool EmailConfirmed { get; init; }
    public bool IsLockedOut { get; init; }
    public string? Role { get; init; }
    public DateTimeOffset? CreatedUtc { get; init; }
    public DateTimeOffset? LastSignInUtc { get; init; }
}
