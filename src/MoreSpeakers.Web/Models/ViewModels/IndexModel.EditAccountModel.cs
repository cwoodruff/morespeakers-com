using System.ComponentModel.DataAnnotations;

using MoreSpeakers.Domain.Validation;

namespace MoreSpeakers.Web.Pages.Account;

public partial class IndexModel
{
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

        [Required]
        [PhoneWithCountryCode]
        [Phone]
        [Display(Name = "Country Code + Phone Number")]
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