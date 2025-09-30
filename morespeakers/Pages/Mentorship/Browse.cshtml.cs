using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using morespeakers.Data;
using morespeakers.Models;
using morespeakers.Models.ViewModels;

namespace morespeakers.Pages.Mentorship;

[Authorize]
public class BrowseModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public BrowseModel(ApplicationDbContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public BrowseMentorsViewModel ViewModel { get; set; } = null!;
    public MentorshipType MentorshipType { get; set; }
    public List<string> SelectedExpertise { get; set; } = new();
    public bool? AvailableNow { get; set; }
    public List<Expertise> AvailableExpertise { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(
        MentorshipType type = MentorshipType.NewToExperienced,
        string? expertise = null,
        bool? availableNow = null)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null) return Unauthorized();

        MentorshipType = type;
        SelectedExpertise = expertise?.Split(',').ToList() ?? new List<string>();
        AvailableNow = availableNow;
        AvailableExpertise = await _context.Expertise.OrderBy(e => e.Name).ToListAsync();

        ViewModel = new BrowseMentorsViewModel
        {
            CurrentUser = currentUser,
            MentorshipType = type,
            SelectedExpertise = SelectedExpertise,
            AvailableNow = availableNow,
            AvailableExpertise = AvailableExpertise
        };

        // Load mentors based on filters
        await LoadMentors(ViewModel);

        return Page();
    }

    private async Task LoadMentors(BrowseMentorsViewModel model)
    {
        var query = _context.Users
            .Include(u => u.SpeakerType)
            .Include(u => u.UserExpertise)
            .ThenInclude(ue => ue.Expertise)
            .Where(u => u.Id != model.CurrentUser.Id); // Exclude current user

        // Filter by speaker type based on mentorship type
        if (model.MentorshipType == MentorshipType.NewToExperienced)
        {
            query = query.Where(u => u.SpeakerTypeId == 2); // Experienced speakers only
        }
        else
        {
            query = query.Where(u => u.SpeakerTypeId == 2); // Both can mentor each other, but for now experienced only
        }

        // Filter by expertise
        if (model.SelectedExpertise.Any())
        {
            var expertiseIds = await _context.Expertise
                .Where(e => model.SelectedExpertise.Contains(e.Name))
                .Select(e => e.Id)
                .ToListAsync();

            query = query.Where(u => u.UserExpertise.Any(ue => expertiseIds.Contains(ue.ExpertiseId)));
        }

        // Filter by availability
        if (model.AvailableNow == true)
        {
            query = query.Where(u => u.IsAvailableForMentoring);
        }

        model.Mentors = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }
}