namespace MoreSpeakers.Web.Models.ViewModels;

public class ConfirmEmailViewModel
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}