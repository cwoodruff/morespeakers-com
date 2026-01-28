namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Expertises;

public partial class EditModel
{
    [LoggerMessage(LogLevel.Information, "[Admin:Expertises] Updated expertise {Id} {Name}")]
    partial void LogAdminExpertisesUpdated(int id, string name);
}