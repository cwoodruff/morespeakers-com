#nullable disable
using System.ComponentModel.DataAnnotations;

using MoreSpeakers.Domain.Validation;

namespace MoreSpeakers.Web.Areas.Identity.Pages.Account;

public partial class RegisterModel
{
    public class InputModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
            MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        // Additional User entity fields
        [Required]
        [StringLength(100)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [PhoneWithCountryCode]
        [Phone]
        [Display(Name = "Country Code + Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(6000)]
        [Display(Name = "Bio")]
        [DataType(DataType.MultilineText)]
        public string Bio { get; set; }

        [Required]
        [StringLength(2000)]
        [Display(Name = "Goals")]
        [DataType(DataType.MultilineText)]
        public string Goals { get; set; }

        [Url]
        [StringLength(500)]
        [Display(Name = "Sessionize Profile URL")]
        public string SessionizeUrl { get; set; }

        [StringLength(500)]
        [Display(Name = "Headshot URL")]
        public string HeadshotUrl { get; set; }

        [Required]
        [Display(Name = "Speaker Type")]
        public int SpeakerTypeId { get; set; }

        [Required]
        [Display(Name = "Areas of Expertise")]
        public int[] SelectedExpertiseIds { get; set; } = Array.Empty<int>();

        [Display(Name = "New Expertise")] 
        public string NewExpertise { get; set; } = string.Empty;

        [Display(Name = "Social Media Sites")] 
        public List<Domain.Models.UserSocialMediaSite> UserSocialMediaSites { get; set; } = [];
    }
}