namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Expertises;

public partial class IndexModel
{
    [LoggerMessage(LogLevel.Information, "[Admin:Expertises] Deactivated expertise {Id} {Name}")]
    partial void LogAdminExpertisesDeactivatedExpertiseIdName(int id, string name);

    [LoggerMessage(LogLevel.Information, "[Admin:Expertises] Activated category {Id} {Name}")]
    partial void LogAdminExpertisesActivatedCategoryIdName(int id, string name);
}