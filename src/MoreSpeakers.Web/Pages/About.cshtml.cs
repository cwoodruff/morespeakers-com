using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoreSpeakers.Web.Pages;

public class AboutModel : PageModel
{
    public void OnGet()
    {
        // This page doesn't need any data from the database
        // It's purely informational content
    }
}