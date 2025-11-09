using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Web.Areas.Identity.Pages.Account;

public class ConfirmEmail : PageModel
{

    private readonly IUserManager _userManager;
    private readonly ILogger<ConfirmEmail> _logger;
    private readonly TelemetryClient _telemetryClient;
    public ConfirmEmail(IUserManager userManager, ILogger<ConfirmEmail> logger, TelemetryClient telemetryClient)
    {
        _userManager = userManager;
        _logger = logger;
        _telemetryClient = telemetryClient;
    }
    
    [BindProperty(SupportsGet = true)]
    public string Email { get; set; } = string.Empty;
    
    [BindProperty(SupportsGet = true)]
    public string Token { get; set; } = string.Empty;
    
    public bool WasSuccessful { get; set; }
    
    public async Task OnGet()
    {
        var user = await _userManager.FindByEmailAsync(Email);
        if (user == null)
        {
            WasSuccessful = false;
            return;
        }

        var result = await _userManager.ConfirmEmailAsync(user, Token);
        WasSuccessful = result;
    }
}