namespace MoreSpeakers.Web.Areas.Admin.Pages.Shared;

/// <summary>
/// Simple view model for Admin dashboard tiles.
/// </summary>
public record DashboardTile(string Title, string Description, string IconClass, string Area, string Page);
