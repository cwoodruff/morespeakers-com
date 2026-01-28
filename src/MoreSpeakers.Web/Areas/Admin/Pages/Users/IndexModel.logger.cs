namespace MoreSpeakers.Web.Areas.Admin.Pages.Users;

public partial class IndexModel
{
    [LoggerMessage(LogLevel.Information, "[AdminUsers] q={Q}, lockout={Lockout}, emailConfirmed={EmailConfirmed}, role={Role}, sort={Sort}, dir={Dir}, page={Page}, pageSize={PageSize}, total={Total}")]
    partial void LogAdminQuery(string? q, string? lockout, string? emailConfirmed, string? role, string? sort, string? dir, int page, int pageSize, int total);
}