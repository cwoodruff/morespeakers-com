namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Sectors;

public partial class EditModel
{
    [LoggerMessage(LogLevel.Information, "[Admin:Sectors] Updated sector {Id} {Name}")]
    partial void LogAdminSectorsUpdatedSectorIdName(int id, string name);
}