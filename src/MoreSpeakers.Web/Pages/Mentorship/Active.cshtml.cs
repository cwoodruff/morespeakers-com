using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoreSpeakers.Web.Data;
using MoreSpeakers.Web.Models;

namespace MoreSpeakers.Web.Pages.Mentorship;

[Authorize]
public class ActiveModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public ActiveModel(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public List<Models.Mentorship> ActiveMentorships { get; set; } = new();
    public Guid CurrentUserId { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        CurrentUserId = currentUser.Id;

        ActiveMentorships = await _context.Mentorship
            .Include(m => m.Mentor)
            .ThenInclude(m => m.SpeakerType)
            .Include(m => m.Mentee)
            .ThenInclude(m => m.SpeakerType)
            .Include(m => m.FocusAreas)
            .ThenInclude(fa => fa.Expertise)
            .Where(m => (m.MentorId == currentUser.Id || m.MenteeId == currentUser.Id) && 
                       m.Status == MentorshipStatus.Active)
            .OrderBy(m => m.StartedAt)
            .ToListAsync();

        return Page();
    }

    public async Task<IActionResult> OnPostCompleteMentorshipAsync(Guid mentorshipId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentorship = await _context.Mentorship
            .FirstOrDefaultAsync(m => m.Id == mentorshipId &&
                                      (m.MentorId == currentUser.Id || m.MenteeId == currentUser.Id));

        if (mentorship == null) return NotFound();

        mentorship.Status = MentorshipStatus.Completed;
        mentorship.CompletedAt = DateTime.UtcNow;
        mentorship.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Let the client know lists/pages can refresh if listening
        Response.Headers["HX-Trigger"] = "{\"mentorship:completed\":{\"id\":\"" + mentorshipId + "\"},\"mentorship:updated\":true}";

        // Remove the card from the DOM; page summary is static until next full load
        return Content(string.Empty);
    }

    public async Task<IActionResult> OnPostCancelMentorshipAsync(Guid mentorshipId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        var mentorship = await _context.Mentorship
            .FirstOrDefaultAsync(m => m.Id == mentorshipId &&
                                      (m.MentorId == currentUser.Id || m.MenteeId == currentUser.Id));

        if (mentorship == null) return NotFound();

        mentorship.Status = MentorshipStatus.Cancelled;
        mentorship.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        Response.Headers["HX-Trigger"] = "{\"mentorship:cancelled\":{\"id\":\"" + mentorshipId + "\"},\"mentorship:updated\":true}";

        return Content(string.Empty);
    }
}