using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using morespeakers.Data;
using morespeakers.Models;

namespace morespeakers.Pages.Profile;

[Authorize]
public class IndexModel(ApplicationDbContext context, UserManager<User> userManager) : PageModel
{
    private readonly ApplicationDbContext _context = context;
    private readonly UserManager<User> _userManager = userManager;

    public User ProfileUser { get; set; } = null!;
    public IEnumerable<UserExpertise> UserExpertise { get; set; } = new List<UserExpertise>();
    public IEnumerable<SocialMedia> SocialMedia { get; set; } = new List<SocialMedia>();
    public bool CanEdit { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Challenge();
        }

        ProfileUser = await _context.Users
            .Include(u => u.SpeakerType)
            .FirstOrDefaultAsync(u => u.Id == currentUser.Id) ?? currentUser;

        UserExpertise = await _context.UserExpertise
            .Include(ue => ue.Expertise)
            .Where(ue => ue.UserId == currentUser.Id)
            .ToListAsync();

        SocialMedia = await _context.SocialMedia
            .Where(sm => sm.UserId == currentUser.Id)
            .ToListAsync();

        CanEdit = true; // User can always edit their own profile

        return Page();
    }
}