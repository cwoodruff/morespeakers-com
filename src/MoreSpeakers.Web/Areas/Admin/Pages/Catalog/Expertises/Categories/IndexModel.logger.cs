namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Categories;

public partial class IndexModel
{
    [LoggerMessage(LogLevel.Information, "[Admin:Categories] Activated category {Id} {Name}")]
    partial void LogAdminCategoriesActivatedCategoryIdName(int id, string name);

    [LoggerMessage(LogLevel.Information, "[Admin:Categories] Deactivated category {Id} {Name}")]
    partial void LogAdminCategoriesDeactivatedCategoryIdName(int id, string name);
}