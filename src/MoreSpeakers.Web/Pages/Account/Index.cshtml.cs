using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MoreSpeakers.Web.Data;
using MoreSpeakers.Web.Models;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Pages.Account;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IExpertiseService _expertiseService;
    private readonly UserManager<User> _userManager;

    public IndexModel(
        UserManager<User> userManager,
        ApplicationDbContext context,
        IExpertiseService expertiseService)
    {
        _userManager = userManager;
        _context = context;
        _expertiseService = expertiseService;
    }

    [BindProperty] public EditAccountModel Input { get; set; } = new();

    public User CurrentUser { get; set; } = null!;
    public IEnumerable<UserExpertise> UserExpertise { get; set; } = new List<UserExpertise>();
    public IEnumerable<SocialMedia> SocialMedia { get; set; } = new List<SocialMedia>();
    public IEnumerable<Expertise> AvailableExpertise { get; set; } = new List<Expertise>();
    public SpeakerType? SpeakerType { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadUserDataAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostEditFieldAsync(string field)
    {
        await LoadUserDataAsync();

        if (!ModelState.IsValid)
        {
            ViewData["Field"] = field;
            ViewData["PageModel"] = this;
            return Partial("_EditField", this);
        }

        try
        {
            // Update the specific field
            switch (field.ToLower())
            {
                case "firstname":
                    CurrentUser.FirstName = Input.FirstName;
                    break;
                case "lastname":
                    CurrentUser.LastName = Input.LastName;
                    break;
                case "email":
                    CurrentUser.Email = Input.Email;
                    CurrentUser.UserName = Input.Email;
                    CurrentUser.NormalizedEmail = Input.Email.ToUpper();
                    CurrentUser.NormalizedUserName = Input.Email.ToUpper();
                    break;
                case "phonenumber":
                    CurrentUser.PhoneNumber = Input.PhoneNumber;
                    break;
                case "bio":
                    CurrentUser.Bio = Input.Bio;
                    break;
                case "goals":
                    CurrentUser.Goals = Input.Goals;
                    break;
                case "sessionizeurl":
                    CurrentUser.SessionizeUrl = Input.SessionizeUrl;
                    break;
                case "headshoturl":
                    CurrentUser.HeadshotUrl = Input.HeadshotUrl;
                    break;
                case "speakertypeid":
                    CurrentUser.SpeakerTypeId = Input.SpeakerTypeId;
                    break;
            }

            await _userManager.UpdateAsync(CurrentUser);
            await LoadUserDataAsync(); // Reload to get updated data

            ViewData["Field"] = field;
            ViewData["PageModel"] = this;
            return Partial("_DisplayField");
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "An error occurred while updating your information.");
            ViewData["Field"] = field;
            ViewData["PageModel"] = this;
            return Partial("_EditField", this);
        }
    }

    public async Task<IActionResult> OnGetEditFieldAsync(string field)
    {
        await LoadUserDataAsync();
        ViewData["Field"] = field;
        ViewData["PageModel"] = this;
        return Partial("_EditField", this);
    }

    public async Task<IActionResult> OnGetCancelEditAsync(string field)
    {
        await LoadUserDataAsync();
        ViewData["Field"] = field;
        ViewData["PageModel"] = this;
        return Partial("_DisplayField");
    }

    private async Task LoadUserDataAsync()
    {
        var userIdString = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            throw new InvalidOperationException("Invalid user ID");

        CurrentUser = await _context.Users
            .Include(u => u.SpeakerType)
            .FirstOrDefaultAsync(u => u.Id == userId) ?? throw new InvalidOperationException("User not found");

        UserExpertise = await _context.UserExpertise
            .Include(ue => ue.Expertise)
            .Where(ue => ue.UserId == userId)
            .ToListAsync();

        SocialMedia = await _context.SocialMedia
            .Where(sm => sm.UserId == userId)
            .ToListAsync();

        AvailableExpertise = await _expertiseService.GetAllExpertiseAsync();
        SpeakerType = CurrentUser.SpeakerType;

        // Populate input model with current values
        Input.FirstName = CurrentUser.FirstName;
        Input.LastName = CurrentUser.LastName;
        Input.Email = CurrentUser.Email ?? string.Empty;
        Input.PhoneNumber = CurrentUser.PhoneNumber;
        Input.Bio = CurrentUser.Bio;
        Input.Goals = CurrentUser.Goals;
        Input.SessionizeUrl = CurrentUser.SessionizeUrl;
        Input.HeadshotUrl = CurrentUser.HeadshotUrl;
        Input.SpeakerTypeId = CurrentUser.SpeakerTypeId;
    }

    public class EditAccountModel
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Phone]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(6000)]
        [Display(Name = "Bio")]
        [DataType(DataType.MultilineText)]
        public string Bio { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        [Display(Name = "Goals")]
        [DataType(DataType.MultilineText)]
        public string Goals { get; set; } = string.Empty;

        [Url]
        [StringLength(500)]
        [Display(Name = "Sessionize Profile URL")]
        public string? SessionizeUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "Headshot URL")]
        public string? HeadshotUrl { get; set; }

        [Required]
        [Display(Name = "Speaker Type")]
        public int SpeakerTypeId { get; set; }
    }
}