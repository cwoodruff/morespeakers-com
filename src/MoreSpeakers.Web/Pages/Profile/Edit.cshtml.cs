using System.ComponentModel.DataAnnotations;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using System.Text.RegularExpressions;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Models.ViewModels;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Pages.Profile;

[Authorize]
public class EditModel(
    IExpertiseManager expertiseManager,
    IUserManager userManager,
    ISocialMediaSiteManager socialMediaSiteManager,
    IFileUploadService fileUploadService,
    ILogger<EditModel> logger) : PageModel
{
    [BindProperty] public ProfileEditInputModel Input { get; set; } = new();

    [BindProperty] public PasswordChangeInputModel PasswordInput { get; set; } = new();

    public User ProfileUser { get; set; } = null!;
    public IEnumerable<Expertise> AvailableExpertise { get; set; } = new List<Expertise>();
    public IEnumerable<UserExpertise> UserExpertise { get; set; } = new List<UserExpertise>();
    public IEnumerable<SocialMediaSite> SocialMediaSites { get; set; } = new List<SocialMediaSite>();


    // Properties for HTMX state management
    public string ActiveTab { get; set; } = "profile";
    public bool HasValidationErrors { get; set; }
    public string ValidationMessage { get; set; } = string.Empty;
    public string SuccessMessage { get; set; } = string.Empty;


public async Task<IActionResult> OnGetAsync()
    {
        var result = await LoadUserDataAsync();
        if (result != null)
        {
            return result;
        }

        SocialMediaSites = await socialMediaSiteManager.GetAllAsync();
        ActiveTab = "profile";
        return Page();
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        // Attempt to validate the input model
        var validationErrors = ValidateProfileEditInputModel(Input);
        if (validationErrors.Count != 0)
        {
            HasValidationErrors = true;
            ValidationMessage = "Please correct the errors below.</br><ul>";
            foreach (var error in validationErrors)
            {
                ValidationMessage += $"<li>{error.ErrorMessage}</li>";
            }
            ValidationMessage += "</ul>";
            return Partial("_ProfileEditForm", this);
        }
        
        var result = await LoadUserDataAsync();
        if (result != null)
        {
            HasValidationErrors = true;
            ValidationMessage = "An error occurred while updating your profile. Please try again.";
            logger.LogError("Error updating user profile for user '{UserId}'. Could not load the profile", ProfileUser.Id);
            return Partial("_ProfileEditForm", this);
        }

        ActiveTab = "profile";

        try
        {
            // Handle headshot upload if provided
            if (Input.HeadshotFile is { Length: > 0 })
            {
                if (fileUploadService.IsValidImageFile(Input.HeadshotFile))
                {
                    // Delete old headshot if exists
                    if (!string.IsNullOrEmpty(ProfileUser.HeadshotUrl) && 
                        ProfileUser.HeadshotUrl.StartsWith("/uploads/headshots/"))
                    {
                        var oldFileName = Path.GetFileName(ProfileUser.HeadshotUrl);
                        fileUploadService.DeleteHeadshotAsync(oldFileName);
                    }

                    // Upload new headshot
                    var fileName = await fileUploadService.UploadHeadshotAsync(Input.HeadshotFile, ProfileUser.Id);
                    if (fileName != null)
                    {
                        ProfileUser.HeadshotUrl = fileUploadService.GetHeadshotPath(fileName);
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
            ProfileUser.SpeakerTypeId = (int)Input.SpeakerTypeId;
            ProfileUser.UpdatedDate = DateTime.UtcNow;
            
            // Save the profile (user information)
            await userManager.SaveAsync(ProfileUser);

            // Update expertise
            await userManager.EmptyAndAddExpertiseForUserAsync(ProfileUser.Id, Input.SelectedExpertiseIds);

            // Update social media
            var socialDictionary = ParseSocialMediaPairs(Request.Form);
            await userManager.EmptyAndAddUserSocialMediaSiteForUserAsync(ProfileUser.Id, socialDictionary.Values.ToDictionary());
            
            // Now that everything is saved, reload the user profile to get the updated values
            result = await LoadUserDataAsync();
            if (result != null)
            {
                HasValidationErrors = true;
                ValidationMessage = "An error occurred while updating your profile. Please try again.";
                logger.LogError("Error updating user profile for user '{UserId}'. Could not load the profile", ProfileUser.Id);
                return Partial("_ProfileEditForm", this);
            }
            UserExpertise = await userManager.GetUserExpertisesForUserAsync(ProfileUser.Id);
            Input.SelectedExpertiseIds = UserExpertise.Select(ue => ue.ExpertiseId).ToArray();
            Input.UserSocialMediaSites = ProfileUser.UserSocialMediaSites.ToList();
            SocialMediaSites = await socialMediaSiteManager.GetAllAsync();
            
            HasValidationErrors = false;
            SuccessMessage = "Profile updated successfully!";
            
            return Partial("_ProfileEditForm", this);
        }
        catch (Exception ex)
        {
            HasValidationErrors = true;
            ValidationMessage = "An error occurred while updating your profile. Please try again.";
            logger.LogError(ex, "Error updating user profile for user '{UserId}'", ProfileUser.Id);
            return Partial("_ProfileEditForm", this);
        }
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        var result = await LoadUserDataAsync();
        if (result != null)
        {
            HasValidationErrors = true;
            ValidationMessage = "An error occurred while changing your password. Please try again.";
            
            logger.LogError("Error changing user password for user '{UserId}'. Could not load the profile", ProfileUser.Id);
            return Partial("_PasswordChangeForm", this);
        }

        SocialMediaSites = await socialMediaSiteManager.GetAllAsync();
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
            var changeResult = await userManager.ChangePasswordAsync(
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
            
            logger.LogError(ex, "Error changing user password for user '{UserId}'", ProfileUser.Id);
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

        SocialMediaSites = await socialMediaSiteManager.GetAllAsync();
        ActiveTab = tab;
        
        return tab switch
        {
            "password" => Partial("_PasswordChangeForm", this),
            _ => Partial("_ProfileEditForm", this)
        };
    }

    // Note implemented yet, but will be used in the future for uploading headshots
    public async Task<IActionResult> OnPostUploadHeadshotAsync()
    {
        ActiveTab = "profile";

        if (Input.HeadshotFile == null || Input.HeadshotFile.Length == 0)
        {
            HasValidationErrors = true;
            ValidationMessage = "Please select an image file to upload.";
            return Partial("_HeadshotUpload", this);
        }

        try
        {
            if (fileUploadService.IsValidImageFile(Input.HeadshotFile))
            {
                // Delete old headshot if exists
                if (!string.IsNullOrEmpty(ProfileUser.HeadshotUrl) && 
                    ProfileUser.HeadshotUrl.StartsWith("/uploads/headshots/"))
                {
                    var oldFileName = Path.GetFileName(ProfileUser.HeadshotUrl);
                    fileUploadService.DeleteHeadshotAsync(oldFileName);
                }

                // Upload new headshot
                var fileName = await fileUploadService.UploadHeadshotAsync(Input.HeadshotFile, ProfileUser.Id);
                if (fileName != null)
                {
                    ProfileUser.HeadshotUrl = fileUploadService.GetHeadshotPath(fileName);
                    ProfileUser.UpdatedDate = DateTime.UtcNow;
                    // NOTE: Placeholder for saving to DB
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
            
            logger.LogError(ex, "Error uploading user headshot for user '{UserId}'", ProfileUser.Id);
            
            return Partial("_HeadshotUpload", this);
        }
    }

    private async Task<IActionResult?> LoadUserDataAsync()
    {
        User? identityUser = null;

        try
        {
            identityUser = await userManager.GetUserAsync(User);
            if (identityUser == null)
            {
                return Challenge();
            }
            
            // Load profile with all the user's data
            var userProfile = await userManager.GetAsync(identityUser.Id);
            if (userProfile == null)
            {
                logger.LogError("Error loading profile page. Could not find user. UserId: '{UserId}'", identityUser.Id);
                return RedirectToPage("/Profile/LoadingProblem",
                    new { UserId = identityUser.Id });
            }

            ProfileUser = userProfile;
            AvailableExpertise = await expertiseManager.GetAllAsync();
            
            // Populate the Input model if not already populated
            if (string.IsNullOrEmpty(Input.FirstName))
            {
                Input.FirstName = ProfileUser.FirstName;
                Input.LastName = ProfileUser.LastName;
                Input.PhoneNumber = ProfileUser.PhoneNumber ?? string.Empty;
                Input.Bio = ProfileUser.Bio;
                Input.Goals = ProfileUser.Goals;
                Input.SessionizeUrl = ProfileUser.SessionizeUrl ?? string.Empty;
                Input.HeadshotUrl = ProfileUser.HeadshotUrl ?? string.Empty;
                
                if (Enum.IsDefined(typeof(SpeakerTypeEnum), ProfileUser.SpeakerTypeId))
                {
                    Input.SpeakerTypeId = (SpeakerTypeEnum)ProfileUser.SpeakerTypeId;
                }
                else
                {
                    Input.SpeakerTypeId = SpeakerTypeEnum.NewSpeaker;
                }
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading user data for user '{UserId}'", identityUser?.Id);
        }

        return null;
    }

    /// <summary>
    /// Parses the posted form collection for indexed pairs like
    /// Input.SocialId[7] and Input.SocialMediaSiteId[7] and returns
    /// a dictionary keyed by the index with the matched values.
    /// </summary>
    /// <param name="form">The posted form collection (e.g., Request.Form)</param>
    /// <param name="leftKeyPrefix">The left key prefix (e.g., "Input.SocialId")</param>
    /// <param name="rightKeyPrefix">The right key prefix (e.g., "Input.SocialMediaSiteId")</param>
    /// <returns>
    /// Dictionary where the key is the index (e.g., 7) and value is a tuple of (leftValue, rightValue).
    /// Only indices present for both sides are included.
    /// </returns>
    private static Dictionary<int, (string leftValue, string rightValue)> ParseIndexedPairs(
        IFormCollection form,
        string leftKeyPrefix,
        string rightKeyPrefix)
    {
        var leftPattern = new Regex($"^" + Regex.Escape(leftKeyPrefix) + "\\[(\\d+)\\]$",
            RegexOptions.Compiled);
        var rightPattern = new Regex($"^" + Regex.Escape(rightKeyPrefix) + "\\[(\\d+)\\]$",
            RegexOptions.Compiled);

        var leftByIndex = new Dictionary<int, string>();
        var rightByIndex = new Dictionary<int, string>();

        foreach (var key in form.Keys)
        {
            var leftMatch = leftPattern.Match(key);
            if (leftMatch.Success && int.TryParse(leftMatch.Groups[1].Value, out var li))
            {
                var v = form[key].ToString();
                leftByIndex[li] = v;
                continue;
            }

            var rightMatch = rightPattern.Match(key);
            if (rightMatch.Success && int.TryParse(rightMatch.Groups[1].Value, out var ri))
            {
                var v = form[key].ToString();
                rightByIndex[ri] = v;
            }
        }

        var result = new Dictionary<int, (string leftValue, string rightValue)>();
        foreach (var idx in leftByIndex.Keys)
        {
            if (!rightByIndex.TryGetValue(idx, out var rightVal))
            {
                continue; // only include indices present on both sides
            }

            var leftVal = leftByIndex[idx];

            // Skip completely empty pairs
            if (string.IsNullOrWhiteSpace(leftVal) && string.IsNullOrWhiteSpace(rightVal))
            {
                continue;
            }

            result[idx] = (leftVal.Trim(), rightVal.Trim());
        }

        return result;
    }

    /// <summary>
    /// Specialized helper for Social Media fields. It reads pairs of
    /// Input.SocialId[n] and Input.SocialMediaSiteId[n] from the provided form
    /// and returns a dictionary keyed by index with (SiteId, SocialId).
    /// </summary>
    private static Dictionary<int, (int SiteId, string SocialId)> ParseSocialMediaPairs(IFormCollection form)
    {
        var pairs = ParseIndexedPairs(form, "Input.SocialId", "Input.SocialMediaSiteId");
        var result = new Dictionary<int, (int SiteId, string SocialId)>();

        foreach (var kvp in pairs)
        {
            // kvp.Value.rightValue is the SocialMediaSiteId, kvp.Value.leftValue is SocialId
            if (!int.TryParse(kvp.Value.rightValue, out var siteId))
            {
                continue; // ignore invalid site ids
            }

            var socialId = kvp.Value.leftValue.Trim();
            if (string.IsNullOrWhiteSpace(socialId))
            {
                continue; // ignore rows without a social id
            }

            result[kvp.Key] = (siteId, socialId);
        }

        return result;
    }

    public async Task<IActionResult> OnGetAddSocialMediaRowAsync(int socialMediaSitesCount = 0)
    {
        try
        {
            socialMediaSitesCount++;
            var model = new UserSocialMediaSiteRow
            {
                UserSocialMediaSite = null,
                SocialMediaSites = await socialMediaSiteManager.GetAllAsync(),
                ItemNumber = socialMediaSitesCount
            };
            return Partial("_UserSocialMediaSiteRow", model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add a social media row");
        }
        return Content("");
    }
}