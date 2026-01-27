using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using MoreSpeakers.Domain.Interfaces;
using MoreSpeakers.Domain.Models;
using MoreSpeakers.Web.Models;
using MoreSpeakers.Web.Models.ViewModels;
using MoreSpeakers.Web.Services;

namespace MoreSpeakers.Web.Pages.Profile;

[Authorize]
public class EditModel(
    IExpertiseManager expertiseManager,
    IUserManager userManager,
    ISocialMediaSiteManager socialMediaSiteManager,
    ISectorManager sectorManager,
    ILogger<EditModel> logger) : PageModel
{
    [BindProperty] public UserProfileViewModel Input { get; set; } = new();

    [BindProperty] public PasswordChangeInputModel PasswordInput { get; set; } = new();

    public User ProfileUser { get; set; } = null!;
    public IEnumerable<Expertise> AvailableExpertises { get; set; } = [];
    public IEnumerable<ExpertiseCategory> ExpertiseCategories { get; set; } = [];
    public IEnumerable<Sector> Sectors { get; set; } = [];
    public IEnumerable<SocialMediaSite> SocialMediaSites { get; set; } = [];
    
    public IEnumerable<UserPasskey> UserPasskeys { get; set; } = [];

    // Properties for HTMX state management
    public string ActiveTab { get; set; } = "profile";
    public bool HasValidationErrors { get; set; }
    public string ValidationMessage { get; set; } = string.Empty;
    public string SuccessMessage { get; set; } = string.Empty;

    // Properties required for NewExpertise setup
    public NewExpertiseCreatedResponse NewExpertiseResponse { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {

        var user = await UpdateModelFromUserAsync(User);
        if (user is null)
        {
            // User Not Found
            logger.LogError("Error loading profile page. Could not find user");
            return RedirectToPage("/Profile/LoadingProblem",
                new { UserId = Guid.Empty });
        }

        ProfileUser = user;
        AvailableExpertises = await expertiseManager.GetAllExpertisesAsync();
        ExpertiseCategories = await expertiseManager.GetAllCategoriesAsync();
        Sectors = await sectorManager.GetAllSectorsAsync();
        SocialMediaSites = await socialMediaSiteManager.GetAllAsync();
        UserPasskeys = await userManager.GetUserPasskeysAsync(user.Id);
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

        var userProfile = await UpdateUserFromModelAsync();
        if (userProfile is null)
        {
            HasValidationErrors = true;
            ValidationMessage = "An error occurred while updating your profile. Please try again.";
            logger.LogError("Error updating user profile for user. Could not load the profile");
            return Partial("_ProfileEditForm", this);
        }

        ActiveTab = "profile";

        try
        {
            // Save the profile (user information)
            userProfile = await userManager.SaveAsync(userProfile);

            // Now that everything is saved, reload the user profile to get the updated values
            var wasSuccessful = UpdateModelFromUserAsync(userProfile);
            if (!wasSuccessful)
            {
                HasValidationErrors = true;
                ValidationMessage = "An error occurred while updating your profile. Please try again.";
                logger.LogError("Error updating user profile for user. Could not load the profile");
                return Partial("_ProfileEditForm", this);
            }

            ProfileUser = userProfile;
            AvailableExpertises = await expertiseManager.GetAllExpertisesAsync();
            ExpertiseCategories = await expertiseManager.GetAllCategoriesAsync();
            Sectors = await sectorManager.GetAllSectorsAsync();
            SocialMediaSites = await socialMediaSiteManager.GetAllAsync();
            UserPasskeys = await userManager.GetUserPasskeysAsync(userProfile.Id);

            HasValidationErrors = false;
            SuccessMessage = "Profile updated successfully!";

            return Partial("_ProfileEditForm", this);
        }
        catch (Exception ex)
        {
            HasValidationErrors = true;
            ValidationMessage = "An error occurred while updating your profile. Please try again.";
            logger.LogError(ex, "Error updating user profile for user '{UserId}'", userProfile.Id);
            return Partial("_ProfileEditForm", this);
        }
    }

    public async Task<IActionResult> OnPostChangePasswordAsync()
    {
        var identityUser = await userManager.GetUserAsync(User);
        if (identityUser is null)
        {
            HasValidationErrors = true;
            ValidationMessage = "An error occurred while changing your password. Please try again.";

            logger.LogError("Error changing user password for user. Could not load the profile");
            return Partial("_PasswordChangeForm", this);
        }

        SocialMediaSites = await socialMediaSiteManager.GetAllAsync();
        UserPasskeys = await userManager.GetUserPasskeysAsync(identityUser.Id);
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
                identityUser,
                PasswordInput.CurrentPassword,
                PasswordInput.NewPassword);

            if (changeResult.Succeeded)
            {
                identityUser.MustChangePassword = false;
                await userManager.SaveAsync(identityUser);

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

            logger.LogError(ex, "Error changing user password for user '{UserId}'", identityUser.Id);
            return Partial("_PasswordChangeForm", this);
        }
    }

    private static List<ValidationResult> ValidateProfileEditInputModel(UserProfileViewModel model)
        => ValidateModel(model);

    private static List<ValidationResult> ValidatePasswordInputModel(PasswordChangeInputModel model)
        => ValidateModel(model);

    private static List<ValidationResult> ValidateModel(object model)
    {
        var context = new ValidationContext(model, null, null);
        var results = new List<ValidationResult>();

        bool isValid = Validator.TryValidateObject(model, context, results, true);

        return !isValid ? results : [];
    }

    public async Task<IActionResult> OnGetTabAsync(string tab)
    {
        var user = await UpdateModelFromUserAsync(User);
        if (user is null)
        {
            // User Not Found
            logger.LogError("Error loading profile page. Could not find user");
            return RedirectToPage("/Profile/LoadingProblem",
                new { UserId = Guid.Empty });
        }

        ProfileUser = user;
        AvailableExpertises = await expertiseManager.GetAllExpertisesAsync();
        ExpertiseCategories = await expertiseManager.GetAllCategoriesAsync();
        Sectors = await sectorManager.GetAllSectorsAsync();
        SocialMediaSites = await socialMediaSiteManager.GetAllAsync();
        UserPasskeys = await userManager.GetUserPasskeysAsync(user.Id);
        ActiveTab = tab;

        return tab switch
        {
            "password" => Partial("_PasswordChangeForm", this),
            "passkeys" => Partial("_Passkeys", this),
            _ => Partial("_ProfileEditForm", this)
        };
    }

    /// <summary>
    /// Returns a populated user object based on the current user's identity with updates to the data from the Input model.
    /// </summary>
    /// <returns></returns>
    private async Task<User?> UpdateUserFromModelAsync()
    {
        User? identityUser = null;

        try
        {
            identityUser = await userManager.GetUserAsync(User);
            if (identityUser == null)
            {
                return null;
            }

            // Load profile with all the user's data
            var userProfile = await userManager.GetAsync(identityUser.Id);
            if (userProfile == null)
            {
                logger.LogError("Error loading profile page. Could not find user. UserId: '{UserId}'", identityUser.Id);
                return null;
            }

            userProfile.FirstName = Input.FirstName!;
            userProfile.LastName = Input.LastName!;
            userProfile.PhoneNumber = Input.PhoneNumber;
            userProfile.Bio = Input.Bio!;
            userProfile.Goals = Input.Goals!;
            userProfile.SessionizeUrl = Input.SessionizeUrl;
            userProfile.SpeakerTypeId = (int)Input.SpeakerTypeId;
            userProfile.UpdatedDate = DateTime.UtcNow;
            userProfile.HeadshotUrl = Input.HeadshotUrl;

            // User Expertise
            userProfile.UserExpertise.Clear();
            foreach (var ue in Input.SelectedExpertiseIds)
            {
                userProfile.UserExpertise.Add(new UserExpertise { ExpertiseId = ue });
            }

            // Social Media Sites
            var socialDictionary = SocialMediaSiteHelper.ParseSocialMediaPairs(Request.Form);
            userProfile.UserSocialMediaSites.Clear();
            foreach (var kvp in socialDictionary)
            {
                userProfile.UserSocialMediaSites.Add(new UserSocialMediaSite
                    {
                        UserId = identityUser.Id,
                        SocialMediaSiteId = kvp.Value.SiteId,
                        SocialId = kvp.Value.SocialId,
                        User = userProfile,
                        SocialMediaSite = new SocialMediaSite { Id = kvp.Value.SiteId, Icon = string.Empty, Name = string.Empty, UrlFormat = string.Empty }
                    }
                );
            }

            return userProfile;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading user data for user '{UserId}'", identityUser?.Id);
        }

        return null;
    }

    /// <summary>
    /// Updates the model with the current user's data.
    /// </summary>
    /// <param name="signedInUser"></param>
    /// <returns>A <see cref="Domain.Models.User"/>, if successful, otherwise null</returns>
    private async Task<User?> UpdateModelFromUserAsync(ClaimsPrincipal signedInUser)
    {

        User? identityUser = null;

        try
        {
            identityUser = await userManager.GetUserAsync(signedInUser);
            if (identityUser == null)
            {
                return null;
            }

            // Load profile with all the user's data
            var userProfile = await userManager.GetAsync(identityUser.Id);
            if (userProfile != null)
            {
                if (UpdateModelFromUserAsync(userProfile))
                {
                    return userProfile;
                }
            }

            logger.LogError("Error loading profile page. Could not find user. UserId: '{UserId}'", identityUser.Id);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading user data for user '{UserId}'", identityUser?.Id);
        }

        return null;
    }


    /// <summary>
    /// Updates the model with the current user's data.
    /// </summary>
    /// <param name="user"></param>
    private bool UpdateModelFromUserAsync(User user)
    {
        try
        {
            Input.FirstName = user.FirstName;
            Input.LastName = user.LastName;
            Input.PhoneNumber = user.PhoneNumber ?? string.Empty;
            Input.Bio = user.Bio;
            Input.Goals = user.Goals;
            Input.SessionizeUrl = user.SessionizeUrl ?? string.Empty;
            Input.HeadshotUrl = user.HeadshotUrl ?? string.Empty;
            Input.SelectedExpertiseIds = [.. user.UserExpertise.Select(ue => ue.ExpertiseId)];
            Input.UserSocialMediaSites = [.. user.UserSocialMediaSites];

            Input.SpeakerTypeId = Enum.IsDefined(typeof(SpeakerTypeEnum), user.SpeakerTypeId)
                ? (SpeakerTypeEnum)user.SpeakerTypeId
                : SpeakerTypeEnum.NewSpeaker;
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error loading user data for user '{UserId}'", user.Id);
        }

        return false;
    }


    public async Task<IActionResult> OnGetAddSocialMediaRowAsync()
    {
        try
        {
            var alreadySelectedIds = new List<int>();
            foreach (var key in Request.Query.Keys.Where(key => key.StartsWith("Input.SocialMediaSiteId")))
            {
                if (int.TryParse(Request.Query[key], out var id))
                {
                    alreadySelectedIds.Add(id);
                }
            }

            var socialMediaSites = await socialMediaSiteManager.GetAllAsync();
            var filteredSites = socialMediaSites
                .Where(s => !alreadySelectedIds.Contains(s.Id))
                .ToList();
            var model = new UserSocialMediaSiteRowViewModel
            {
                UserSocialMediaSite = null,
                SocialMediaSites = filteredSites,
                ItemNumber = alreadySelectedIds.Count
            };
            return Partial("_UserSocialMediaSiteRow", model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to add a social media row");
        }
        return Content("");
    }


    public async Task<IActionResult> OnPostValidateNewExpertiseAsync()
    {
        if (string.IsNullOrWhiteSpace(Input.NewExpertise))
        {
            return new JsonResult(new NewExpertiseResponse
            {
                IsValid = false
            });
        }

        var expertiseName = Input.NewExpertise;
        var trimmedName = expertiseName.Trim();

        // Search to see if the name exists
        var existingExpertise = await expertiseManager.DoesExpertiseWithNameExistsAsync(trimmedName);
        if (existingExpertise)
        {
            return new JsonResult(new NewExpertiseResponse
            {
                IsValid = false,
                Message = $"Expertise '{expertiseName}' already exists."
            });
        }

        // Check for similar expertise (fuzzy matching for suggestions)
        var similarExpertise = await expertiseManager.FuzzySearchForExistingExpertise(trimmedName, 5);

        List<Expertise> expertises = [.. similarExpertise];
        if (expertises.Count != 0)
        {

            var suggestions = string.Join(", ", expertises.Select(s => s.Name));
            return new JsonResult(new NewExpertiseResponse
            {
                IsValid = false,
                Message = $"Similar expertise found: {suggestions}. Consider selecting from existing options.",
            });
        }

        return new JsonResult(new NewExpertiseResponse
        {
            IsValid = true
        });
    }

    public async Task<IActionResult> OnPostSubmitNewExpertiseAsync()
    {
        var profileUser = await UpdateModelFromUserAsync(User);
        ExpertiseCategories = await expertiseManager.GetAllCategoriesAsync();
        Sectors = await sectorManager.GetAllSectorsAsync();

        if (profileUser is not null)
        {
            ProfileUser = profileUser;
        }
        if (string.IsNullOrWhiteSpace(Input.NewExpertise))
        {
            this.NewExpertiseResponse = new NewExpertiseCreatedResponse()
            {
                SavingExpertiseFailed = true, SaveExpertiseMessage = "No expertise name was provided.",
                ExpertiseCategories = ExpertiseCategories,
                Sectors = Sectors
            };
            return Partial("_ProfileEditForm", this);
        }

        var expertiseName = Input.NewExpertise;
        var trimmedName = expertiseName.Trim();
        var expertiseCategoryId = Input.NewExpertiseCategoryId;

        try
        {
            // Search to see if the name exists
            var existingExpertise = await expertiseManager.DoesExpertiseWithNameExistsAsync(trimmedName);
            if (existingExpertise)
            {
                this.NewExpertiseResponse = new NewExpertiseCreatedResponse()
                {
                    SavingExpertiseFailed = true, SaveExpertiseMessage =  $"Expertise '{expertiseName}' already exists.",
                    ExpertiseCategories = ExpertiseCategories,
                    Sectors = Sectors
                };
                return Partial("_ProfileEditForm", this);
            }

            // Attempt to save the expertise
            var expertiseId =
                await expertiseManager.CreateExpertiseAsync(name: trimmedName, expertiseCategoryId: expertiseCategoryId);

            if (expertiseId == 0)
            {
                this.NewExpertiseResponse = new NewExpertiseCreatedResponse
                {
                    SavingExpertiseFailed = true, SaveExpertiseMessage =  $"Failed to create the expertise '{expertiseName}'.",
                    ExpertiseCategories = ExpertiseCategories,
                    Sectors = Sectors
                };
                return Partial("_ProfileEditForm", this);
            }
            AvailableExpertises = await expertiseManager.GetAllExpertisesAsync();
            SocialMediaSites = await socialMediaSiteManager.GetAllAsync();
            Input.SelectedExpertiseIds = [.. Input.SelectedExpertiseIds.Concat([expertiseId])];

            this.NewExpertiseResponse = new NewExpertiseCreatedResponse
            {
                SavingExpertiseFailed = false, SaveExpertiseMessage =  string.Empty,
                ExpertiseCategories = ExpertiseCategories,
                Sectors = Sectors
            };
            return Partial("_ProfileEditForm", this);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error submitting new expertise");
            AvailableExpertises = await expertiseManager.GetAllExpertisesAsync();
            this.NewExpertiseResponse = new NewExpertiseCreatedResponse
            {
                SavingExpertiseFailed = true, SaveExpertiseMessage =  $"Failed to create the expertise '{expertiseName}'.",
                ExpertiseCategories = ExpertiseCategories,
                Sectors = Sectors
            };
            return Partial("_ProfileEditForm", this);
        }
    }
}