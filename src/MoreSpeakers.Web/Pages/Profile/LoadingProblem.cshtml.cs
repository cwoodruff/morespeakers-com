using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoreSpeakers.Web.Pages.Profile;

public class LoadingProblem : PageModel
{
    
    [BindProperty(Name = "UserId", SupportsGet = true)]
    public Guid UserId { get; set; }
    
    public void OnGet()
    {
        
    }
}