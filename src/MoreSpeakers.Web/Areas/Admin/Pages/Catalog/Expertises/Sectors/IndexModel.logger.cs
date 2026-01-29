namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Sectors;

public partial class IndexModel
{
    [LoggerMessage(LogLevel.Information, "[Admin:Sectors] Deactivated sector {Id} {Name}")]
    partial void LogAdminSectorsDeactivated(int id, string name);

    [LoggerMessage(LogLevel.Information, "[Admin:Sectors] Activated sector {Id} {Name}")]
    partial void LogAdminSectorsActivated(int id, string name);
}