using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Web.Areas.Admin.Pages.Catalog;

public class UtilitiesModel(
    IUserManager userManager,
    IOpenGraphSpeakerProfileImageGenerator openGraphSpeakerProfileImageGenerator,
    ILogger<UtilitiesModel> logger)
    : PageModel
{

    [BindProperty]
    public int RegeneratedSpeakerProfileImageCount { get; private set; }

    public void OnGet()
    {

    }

    public async Task<IActionResult> OnPostRegenerateAsync()
    {
        var users = await userManager.GetAllAsync();
        var count = 0;

        foreach (var user in users)
        {
            if (string.IsNullOrEmpty(user.HeadshotUrl))
            {
                continue;
            }

            await openGraphSpeakerProfileImageGenerator.QueueSpeakerOpenGraphProfileImageCreation(user.Id,
                user.HeadshotUrl, user.FullName);
            count++;

        }

        logger.LogInformation("Regenerated {Count} speaker profile images", count);
        RegeneratedSpeakerProfileImageCount = count;

        return Partial("~/Areas/Admin/Pages/Catalog/_Utilities_RegenerateStatus.cshtml", count);
    }
}