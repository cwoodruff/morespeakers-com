using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using MoreSpeakers.Domain.Interfaces;

namespace MoreSpeakers.Web.Filters;

public class MustChangePasswordFilter(IUserManager userManager) : IAsyncPageFilter
{
    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated == true)
        {
            var user = await userManager.GetUserAsync(context.HttpContext.User);
            if (user != null && user.MustChangePassword)
            {
                var path = context.HttpContext.Request.Path.Value;
                // Allow access to the change password page and sign out
                if (path != null && 
                    !path.StartsWith("/Profile/Edit", StringComparison.OrdinalIgnoreCase) && 
                    !path.StartsWith("/Identity/Account/Logout", StringComparison.OrdinalIgnoreCase))
                {
                    context.Result = new RedirectToPageResult("/Profile/Edit", new { tab = "password" });
                    return;
                }
            }
        }

        await next();
    }
}