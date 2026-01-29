namespace MoreSpeakers.Web.Pages;

public partial class IndexModel
{
    [LoggerMessage(LogLevel.Error, "Error loading index page")]
    partial void LogErrorLoadingIndexPage(Exception exception);
}