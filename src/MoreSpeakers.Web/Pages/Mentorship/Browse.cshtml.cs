using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoreSpeakers.Web.Pages.Mentorship;

[Authorize]
public class Browse(ILogger<Browse> logger) : PageModel
{
    private readonly ILogger<Browse> _logger = logger;

    public LocalRedirectResult OnGetAsync()
    {
        _logger.LogInformation("Browse mentorship was called");
        return LocalRedirectPermanentPreserveMethod("~/Speakers/Index");
    }
}