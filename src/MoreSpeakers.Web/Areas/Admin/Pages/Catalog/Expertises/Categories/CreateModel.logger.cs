namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Categories;

public partial class CreateModel
{
    [LoggerMessage(LogLevel.Information, "[Admin:Categories] Created category {Id} {Name}")]
    partial void LogAdminCategoriesCreated(int id, string name);
}