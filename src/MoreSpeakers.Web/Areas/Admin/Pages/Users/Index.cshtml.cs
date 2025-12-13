using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models.AdminUsers;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Users;

[Authorize(Roles = "Administrator")]
public class IndexModel(IUserManager userManager, ILogger<IndexModel> logger) : PageModel
{
    private readonly IUserManager _userManager = userManager;
    private readonly ILogger<IndexModel> _logger = logger;

    public sealed class QueryModel
    {
        [FromQuery(Name = "q")] public string? Q { get; set; }
        [FromQuery(Name = "lockout")] public string? Lockout { get; set; } = "any";
        [FromQuery(Name = "emailConfirmed")] public string? EmailConfirmed { get; set; } = "any";
        [FromQuery(Name = "role")] public string? Role { get; set; }
        [FromQuery(Name = "sort")] public string? Sort { get; set; } = "email";
        [FromQuery(Name = "dir")] public string? Dir { get; set; } = "asc";
        [FromQuery(Name = "page")] public int Page { get; set; } = 1;
        [FromQuery(Name = "pageSize")] public int PageSize { get; set; } = 20;
    }

    [BindProperty(SupportsGet = true)]
    public QueryModel Query { get; set; } = new();

    public required PagedResult<UserListRow> Result { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();

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

        _logger.LogInformation("[AdminUsers] q={Q}, lockout={Lockout}, emailConfirmed={EmailConfirmed}, role={Role}, sort={Sort}, dir={Dir}, page={Page}, pageSize={PageSize}, total={Total}",
            filter.Query, Query.Lockout, Query.EmailConfirmed, filter.RoleName, Query.Sort, Query.Dir, Result.Page, Result.PageSize, Result.TotalCount);

        return Page();
    }

    private static TriState ParseTriState(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            "true" => TriState.True,
            "false" => TriState.False,
            "locked" => TriState.True, // support alias
            "notlocked" => TriState.False, // support alias
            _ => TriState.Any
        };
    }

    private static UserAdminSortBy ParseSortBy(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            "username" => UserAdminSortBy.UserName,
            "emailconfirmed" => UserAdminSortBy.EmailConfirmed,
            "lockout" => UserAdminSortBy.LockedOut,
            "role" => UserAdminSortBy.Role,
            "created" => UserAdminSortBy.CreatedUtc,
            "lastsignin" => UserAdminSortBy.LastSignInUtc,
            _ => UserAdminSortBy.Email
        };
    }
}
