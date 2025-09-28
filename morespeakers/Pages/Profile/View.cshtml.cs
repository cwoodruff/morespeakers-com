using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using morespeakers.Data;
using morespeakers.Models;

namespace morespeakers.Pages.Profile;

public class ViewModel(ApplicationDbContext context) : PageModel
{
    private readonly ApplicationDbContext _context = context;

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public User? ProfileUser { get; set; }
    public IEnumerable<UserExpertise> UserExpertise { get; set; } = new List<UserExpertise>();
    public IEnumerable<SocialMedia> SocialMedia { get; set; } = new List<SocialMedia>();

    public async Task<IActionResult> OnGetAsync()
    {
        if (Id == Guid.Empty)
        {
            return NotFound();
        }

        ProfileUser = await _context.Users
            .Include(u => u.SpeakerType)
            .FirstOrDefaultAsync(u => u.Id == Id);

        if (ProfileUser == null)
        {
            return NotFound();
        }

        UserExpertise = await _context.UserExpertise
            .Include(ue => ue.Expertise)
            .Where(ue => ue.UserId == Id)
            .ToListAsync();

        SocialMedia = await _context.SocialMedia
            .Where(sm => sm.UserId == Id)
            .ToListAsync();

        return Page();
    }
}
