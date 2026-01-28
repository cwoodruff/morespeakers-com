namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog.Expertises.Sectors;

public partial class CreateModel
{
    [LoggerMessage(LogLevel.Information, "[Admin:Sectors] Created sector {Id} {Name}")]
    partial void LogAdminSectorsCreated(int id, string name);
}