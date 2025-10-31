using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Web.Models.ViewModels;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Pages.Profile;

[Authorize]
public class EditModel(
    IExpertiseManager expertiseManager,
    IUserManager userManager,
    IFileUploadService fileUploadService) : PageModel
{
    private readonly IExpertiseManager _expertiseManager = expertiseManager;
    private readonly IUserManager _userManager = userManager;
    private readonly IFileUploadService _fileUploadService = fileUploadService;

    [BindProperty]
    public ProfileEditInputModel Input { get; set; } = new();
    
    [BindProperty]
    public PasswordChangeInputModel PasswordInput { get; set; } = new();

    public Domain.Models.User ProfileUser { get; set; } = null!;
    public IEnumerable<Domain.Models.Expertise> AvailableExpertise { get; set; } = new List<Domain.Models.Expertise>();
    public IEnumerable<Domain.Models.UserExpertise> UserExpertise { get; set; } = new List<Domain.Models.UserExpertise>();
    public IEnumerable<Domain.Models.SocialMedia> SocialMedia { get; set; } = new List<Domain.Models.SocialMedia>();

    // Properties for HTMX state management
    public string ActiveTab { get; set; } = "profile";
    public bool HasValidationErrors { get; set; }
    public string ValidationMessage { get; set; } = string.Empty;
    public string SuccessMessage { get; set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        var result = await LoadUserDataAsync();
        if (result != null) return result;

        ActiveTab = "profile";
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        var result = await LoadUserDataAsync();
        if (result != null) return result;

        ActiveTab = "profile";

        var validationErrors = ValidateProfileEditInputModel(Input);
        if (validationErrors.Count != 0)
        {
            HasValidationErrors = true;
            ValidationMessage = "Please correct the password errors below.</br><ul>";
            foreach (var error in validationErrors)
            {
                ValidationMessage += $"<li>{error.ErrorMessage}</li>";
            }
            ValidationMessage += "</ul>";
            return Partial("_ProfileEditForm", this);
        }

        try
        {
            // Handle headshot upload if provided
            if (Input.HeadshotFile != null && Input.HeadshotFile.Length > 0)
            {
                if (_fileUploadService.IsValidImageFile(Input.HeadshotFile))
                {
                    // Delete old headshot if exists
                    if (!string.IsNullOrEmpty(ProfileUser.HeadshotUrl) && 
                        ProfileUser.HeadshotUrl.StartsWith("/uploads/headshots/"))
                    {
                        var oldFileName = Path.GetFileName(ProfileUser.HeadshotUrl);
                        await _fileUploadService.DeleteHeadshotAsync(oldFileName);
                    }

                    // Upload new headshot
                    var fileName = await _fileUploadService.UploadHeadshotAsync(Input.HeadshotFile, ProfileUser.Id);
                    if (fileName != null)
                    {
                        ProfileUser.HeadshotUrl = _fileUploadService.GetHeadshotPath(fileName);
                    }
                }
                else
                {
                    HasValidationErrors = true;
                    ValidationMessage = "Invalid image file. Please upload a JPG, PNG, or GIF file under 5MB.";
                    return Partial("_ProfileEditForm", this);
                }
            }
            else if (!string.IsNullOrEmpty(Input.HeadshotUrl))
            {
                // Use provided URL if no file uploaded
                ProfileUser.HeadshotUrl = Input.HeadshotUrl;
            }

            // Update user profile
            ProfileUser.FirstName = Input.FirstName;
            ProfileUser.LastName = Input.LastName;
            ProfileUser.PhoneNumber = Input.PhoneNumber;
            ProfileUser.Bio = Input.Bio;
            ProfileUser.Goals = Input.Goals;
            ProfileUser.SessionizeUrl = Input.SessionizeUrl;
            ProfileUser.SpeakerTypeId = Input.SpeakerTypeId;
            ProfileUser.UpdatedDate = DateTime.UtcNow;
            
            // Change this to use UserManager UpdateAsync() method
            await _userManager.SaveAsync(ProfileUser);

            // Update expertise
            await UpdateUserExpertiseAsync();

            // Update social media
            await UpdateSocialMediaAsync();

            HasValidationErrors = false;
            SuccessMessage = "Profile updated successfully!";
            
            return Partial("_ProfileEditForm", this);
        }
        catch (Exception ex)
        {
            HasValidationErrors = true;
            ValidationMessage = "An error occurred while updating your profile. Please try again.";

            return Partial("_ProfileEditForm", this);
        }
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        var result = await LoadUserDataAsync();
        if (result != null) return result;

        ActiveTab = "password";

        var validationErrors = ValidatePasswordInputModel(PasswordInput);
        if (validationErrors.Count != 0)
        {
            HasValidationErrors = true;
            ValidationMessage = "Please correct the password errors below.</br><ul>";
            foreach (var error in validationErrors)
            {
                ValidationMessage += $"<li>{error.ErrorMessage}</li>";
            }
            ValidationMessage += "</ul>";
            return Partial("_PasswordChangeForm", this);
        }

        try
        {
            var changeResult = await _userManager.ChangePasswordAsync(
                ProfileUser, 
                PasswordInput.CurrentPassword, 
                PasswordInput.NewPassword);

            if (changeResult.Succeeded)
            {
                HasValidationErrors = false;
                SuccessMessage = "Password changed successfully!";
                PasswordInput = new PasswordChangeInputModel(); // Clear form
            }
            else
            {
                HasValidationErrors = true;
                ValidationMessage = string.Join("; ", changeResult.Errors.Select(e => e.Description));
            }

            return Partial("_PasswordChangeForm", this);
        }
        catch (Exception ex)
        {
            HasValidationErrors = true;
            ValidationMessage = "An error occurred while changing your password. Please try again.";
            return Partial("_PasswordChangeForm", this);
        }
    }

    private List<ValidationResult> ValidateProfileEditInputModel(ProfileEditInputModel model)
    {
        return ValidateModel(model);
    }
    
    private List<ValidationResult> ValidatePasswordInputModel(PasswordChangeInputModel model)
    {
        return ValidateModel(model);
    }
    
    private List<ValidationResult> ValidateModel(object model)
    {
        var context = new ValidationContext(model, null, null);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(model, context, results, true);

        return !isValid ? results : [];
    }

    public async Task<IActionResult> OnGetTabAsync(string tab)
    {
        var result = await LoadUserDataAsync();
        if (result != null) return result;

        ActiveTab = tab;
        
        return tab switch
        {
            "password" => Partial("_PasswordChangeForm", this),
            "profile" => Partial("_ProfileEditForm", this),
            _ => Partial("_ProfileEditForm", this)
        };
    }

    // Note implemented yet, but will be used in the future for uploading headshots
    public async Task<IActionResult> OnPostUploadHeadshotAsync()
    {
        //var result = await LoadUserDataAsync();
        //if (result != null) return result;

        ActiveTab = "profile";

        if (Input.HeadshotFile == null || Input.HeadshotFile.Length == 0)
        {
            HasValidationErrors = true;
            ValidationMessage = "Please select an image file to upload.";
            return Partial("_HeadshotUpload", this);
        }

        try
        {
            if (_fileUploadService.IsValidImageFile(Input.HeadshotFile))
            {
                // Delete old headshot if exists
                if (!string.IsNullOrEmpty(ProfileUser.HeadshotUrl) && 
                    ProfileUser.HeadshotUrl.StartsWith("/uploads/headshots/"))
                {
                    var oldFileName = Path.GetFileName(ProfileUser.HeadshotUrl);
                    await _fileUploadService.DeleteHeadshotAsync(oldFileName);
                }

                // Upload new headshot
                var fileName = await _fileUploadService.UploadHeadshotAsync(Input.HeadshotFile, ProfileUser.Id);
                if (fileName != null)
                {
                    ProfileUser.HeadshotUrl = _fileUploadService.GetHeadshotPath(fileName);
                    ProfileUser.UpdatedDate = DateTime.UtcNow;
                    // NOTE: Place holder for saving to DB
                    //await _context.SaveChangesAsync();

                    HasValidationErrors = false;
                    SuccessMessage = "Headshot uploaded successfully!";
                    Input.HeadshotUrl = ProfileUser.HeadshotUrl; // Update input model
                }
                else
                {
                    HasValidationErrors = true;
                    ValidationMessage = "Failed to upload image. Please try again.";
                }
            }
            else
            {
                HasValidationErrors = true;
                ValidationMessage = "Invalid image file. Please upload a JPG, PNG, or GIF file under 5MB.";
            }

            return Partial("_HeadshotUpload", this);
        }
        catch (Exception ex)
        {
            HasValidationErrors = true;
            ValidationMessage = "An error occurred while uploading your image. Please try again.";
            return Partial("_HeadshotUpload", this);
        }
    }

    private async Task<IActionResult?> LoadUserDataAsync()
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Challenge();
        }

        ProfileUser = await _userManager.GetAsync(currentUser.Id);
        
        AvailableExpertise = await _expertiseManager.GetAllAsync();
        
        UserExpertise = await _userManager.GetUserExpertisesForUserAsync(currentUser.Id);
        
        SocialMedia = await _userManager.GetUserSocialMediaForUserAsync(currentUser.Id);

        // Populate Input model if not already populated
        if (string.IsNullOrEmpty(Input.FirstName))
        {
            Input.FirstName = ProfileUser.FirstName;
            Input.LastName = ProfileUser.LastName;
            Input.PhoneNumber = ProfileUser.PhoneNumber ?? string.Empty;
            Input.Bio = ProfileUser.Bio ?? string.Empty;
            Input.Goals = ProfileUser.Goals ?? string.Empty;
            Input.SessionizeUrl = ProfileUser.SessionizeUrl ?? string.Empty;
            Input.HeadshotUrl = ProfileUser.HeadshotUrl ?? string.Empty;
            Input.SpeakerTypeId = ProfileUser.SpeakerTypeId;
            Input.SelectedExpertiseIds = UserExpertise.Select(ue => ue.ExpertiseId).ToArray();
            
            var socialMediaList = SocialMedia.ToList();
            Input.SocialMediaPlatforms = socialMediaList.Select(sm => sm.Platform).ToArray();
            Input.SocialMediaUrls = socialMediaList.Select(sm => sm.Url).ToArray();
        }

        return null;
    }

    private async Task UpdateUserExpertiseAsync()
    {
        await _userManager.EmptyAndAddExpertiseForUserAsync(ProfileUser.Id, Input.SelectedExpertiseIds);
    }

    private async Task UpdateSocialMediaAsync()
    {

        var socialMedias = new List<Domain.Models.SocialMedia>();
        
        for (int i = 0; i < Math.Min(Input.SocialMediaPlatforms.Length, Input.SocialMediaUrls.Length); i++)
        {
            if (!string.IsNullOrWhiteSpace(Input.SocialMediaPlatforms[i]) && 
                !string.IsNullOrWhiteSpace(Input.SocialMediaUrls[i]))
            {
                socialMedias.Add(new Domain.Models.SocialMedia
                {
                    UserId = ProfileUser.Id,
                    Platform = Input.SocialMediaPlatforms[i].Trim(),
                    Url = Input.SocialMediaUrls[i].Trim(),
                    CreatedDate = DateTime.UtcNow
                });
            }
        }
        
        await _userManager.EmptyAndAddSocialMediaForUserAsync(ProfileUser.Id,socialMedias);
    }
}