namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Categories;

public partial class EditModel
{
    [LoggerMessage(LogLevel.Information, "[Admin:Categories] Updated category {Id} {Name}")]
    partial void LogAdminCategoriesUpdated(int id, string name);
}