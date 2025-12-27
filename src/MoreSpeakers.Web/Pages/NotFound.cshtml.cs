using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MoreSpeakers.Web.Pages;

public class NotFoundModel : PageModel
{
    public void OnGet()
    {
        Response.StatusCode = 404;
    }
}
