using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using morespeakers.Data;
using morespeakers.Models;

namespace morespeakers.Pages.Mentorship;

[Authorize]
public class RequestsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public RequestsModel(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public List<Models.Mentorship> IncomingRequests { get; set; } = new();
    public List<Models.Mentorship> OutgoingRequests { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        IncomingRequests = await GetIncomingRequests(currentUser.Id);
        OutgoingRequests = await GetOutgoingRequests(currentUser.Id);

        return Page();
    }

    private async Task<List<Models.Mentorship>> GetIncomingRequests(Guid userId)
    {
        return await _context.Mentorship
            .Include(m => m.Mentee)
            .Include(m => m.FocusAreas)
            .ThenInclude(fa => fa.Expertise)
            .Where(m => m.MentorId == userId && m.Status == MentorshipStatus.Pending)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
    }

    private async Task<List<Models.Mentorship>> GetOutgoingRequests(Guid userId)
    {
        return await _context.Mentorship
            .Include(m => m.Mentor)
            .ThenInclude(m => m.SpeakerType)
            .Include(m => m.FocusAreas)
            .ThenInclude(fa => fa.Expertise)
            .Where(m => m.MenteeId == userId)
            .OrderByDescending(m => m.RequestedAt)
            .ToListAsync();
    }
}