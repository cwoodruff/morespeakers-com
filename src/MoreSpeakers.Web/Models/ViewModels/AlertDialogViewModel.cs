namespace MoreSpeakers.Web.Models.ViewModels;

public class AlertDialogViewModel
{
    public AlertTypeEnum AlertType { get; set; } = AlertTypeEnum.Danger;
    public string Message { get; set; } = string.Empty;

    public string AlertContext
    {
        get
        {
            return AlertType switch
            {
                AlertTypeEnum.Primary => "alert-primary",
                AlertTypeEnum.Secondary => "alert-secondary",
                AlertTypeEnum.Success => "alert-success",
                AlertTypeEnum.Danger => "alert-danger",
                AlertTypeEnum.Warning => "alert-warning",
                AlertTypeEnum.Info => "alert-info",
                AlertTypeEnum.Light => "alert-light",
                AlertTypeEnum.Dark => "alert-dark",
                _ => "alert-primary"
            };
        }
    }
}

public enum AlertTypeEnum
{
    Primary,
    Secondary,
    Success,
    Danger,
    Warning,
    Info,
    Light,
    Dark
}