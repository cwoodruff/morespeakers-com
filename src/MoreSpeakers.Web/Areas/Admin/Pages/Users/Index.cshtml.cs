using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Users;

[Authorize(Roles = Domain.Constants.UserRoles.Administrator)]
public partial class IndexModel(IUserManager userManager, ILogger<IndexModel> logger) : PageModel
{
    private readonly IUserManager _userManager = userManager;
    private readonly ILogger<IndexModel> _logger = logger;

    public sealed class QueryModel
    {
        [FromQuery(Name = "q")] public string? Q { get; set; }
        [FromQuery(Name = "lockout")] public string? Lockout { get; set; } = "any";
        [FromQuery(Name = "emailConfirmed")] public string? EmailConfirmed { get; set; } = "any";
        [FromQuery(Name = "deleted")] public string? Deleted { get; set; } = "false";
        [FromQuery(Name = "role")] public string? Role { get; set; }
        [FromQuery(Name = "sort")] public string? Sort { get; set; } = "email";
        [FromQuery(Name = "dir")] public string? Dir { get; set; } = "asc";
        [FromQuery(Name = "page")] public int Page { get; set; } = 1;
        [FromQuery(Name = "pageSize")] public int PageSize { get; set; } = 20;
    }

    [BindProperty(SupportsGet = true)]
    public QueryModel Query { get; set; } = new();

    public required PagedResult<UserListRow> Result { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        // Normalize inputs
        var page = Math.Max(1, Query.Page);
        var pageSize = Math.Clamp(Query.PageSize, 1, 100);

        var filter = new UserAdminFilter
        {
            Query = string.IsNullOrWhiteSpace(Query.Q) ? null : Query.Q!.Trim(),
            EmailConfirmed = ParseTriState(Query.EmailConfirmed),
            LockedOut = ParseTriState(Query.Lockout),
            IsDeleted = ParseTriState(Query.Deleted),
            RoleName = string.IsNullOrWhiteSpace(Query.Role) ? null : Query.Role!.Trim()
        };

        var sort = new UserAdminSort
        {
            By = ParseSortBy(Query.Sort),
            Direction = string.Equals(Query.Dir, "desc", StringComparison.OrdinalIgnoreCase)
                ? SortDirection.Desc
                : SortDirection.Asc
        };

        Roles = await _userManager.GetAllRoleNamesAsync();
        Result = await _userManager.AdminSearchUsersAsync(filter, sort, page, pageSize);

        LogAdminQuery(filter.Query, Query.Lockout, Query.EmailConfirmed, filter.RoleName, Query?.Sort, Query?.Dir, Result.Page, Result.PageSize, Result.TotalCount);

        return Request.Headers.TryGetValue("HX-Request", out var hx) && string.Equals(hx, "true", StringComparison.OrdinalIgnoreCase)
            ? Partial("_UserList", this)
            : Page();
    }

    private static readonly Dictionary<string, TriState> TriStateMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["true"] = TriState.True,
        ["false"] = TriState.False,
        ["locked"] = TriState.True,      // alias
        ["notlocked"] = TriState.False,  // alias
        ["deleted"] = TriState.True,     // alias
        ["notdeleted"] = TriState.False, // alias
    };

    private static TriState ParseTriState(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? TriState.Any
            : TriStateMap.TryGetValue(value.Trim(), out var state) ? state : TriState.Any;
    }

    private static readonly Dictionary<string, UserAdminSortBy> SortByMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["email"] = UserAdminSortBy.Email,
        ["username"] = UserAdminSortBy.UserName,
        ["emailconfirmed"] = UserAdminSortBy.EmailConfirmed,
        ["lockout"] = UserAdminSortBy.LockedOut,
        ["role"] = UserAdminSortBy.Role,
        ["created"] = UserAdminSortBy.CreatedUtc,
        ["lastsignin"] = UserAdminSortBy.LastSignInUtc,
        ["deleted"] = UserAdminSortBy.Deleted,
    };

    private static UserAdminSortBy ParseSortBy(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? UserAdminSortBy.Email
            : SortByMap.TryGetValue(value.Trim(), out var by) ? by : UserAdminSortBy.Email;
    }

    public async Task<IActionResult> OnPostSoftDeleteAsync(Guid id)
    {
        var ok = await _userManager.SoftDeleteAsync(id);
        if (!ok)
        {
            Response.StatusCode = 400;
        }

        // Re-execute the list query to refresh the table
        var page = Math.Max(1, Query.Page);
        var pageSize = Math.Clamp(Query.PageSize, 1, 100);

        var filter = new UserAdminFilter
        {
            Query = string.IsNullOrWhiteSpace(Query.Q) ? null : Query.Q!.Trim(),
            EmailConfirmed = ParseTriState(Query.EmailConfirmed),
            LockedOut = ParseTriState(Query.Lockout),
            IsDeleted = ParseTriState(Query.Deleted),
            RoleName = string.IsNullOrWhiteSpace(Query.Role) ? null : Query.Role!.Trim()
        };

        var sort = new UserAdminSort
        {
            By = ParseSortBy(Query.Sort),
            Direction = string.Equals(Query.Dir, "desc", StringComparison.OrdinalIgnoreCase)
                ? SortDirection.Desc
                : SortDirection.Asc
        };

        Roles = await _userManager.GetAllRoleNamesAsync();
        Result = await _userManager.AdminSearchUsersAsync(filter, sort, page, pageSize);

        return Partial("_UserList", this);
    }
}
