namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Expertises;

public partial class CreateModel
{
    [LoggerMessage(LogLevel.Information, "[Admin:Expertises] Created expertise {Id} {Name}")]
    partial void LogAdminExpertisesCreatedExpertiseIdName(int id, string name);
}