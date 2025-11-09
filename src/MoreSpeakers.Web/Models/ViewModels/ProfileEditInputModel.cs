using System.ComponentModel.DataAnnotations;

using MoreSpeakers.Domain.Validation;

namespace MoreSpeakers.Web.Models.ViewModels;

public class ProfileEditInputModel
{
    [Required]
    [StringLength(100)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = string.Empty;

    [PhoneWithCountryCode]
    [Phone]
    [Display(Name = "Country Code + Phone Number")]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(6000)]
    [Display(Name = "Bio")]
    [DataType(DataType.MultilineText)]
    public string Bio { get; set; } = string.Empty;

    [StringLength(2000)]
    [Display(Name = "Goals")]
    [DataType(DataType.MultilineText)]
    public string Goals { get; set; } = string.Empty;

    [Url]
    [StringLength(500)]
    [Display(Name = "Sessionize Profile URL")]
    public string SessionizeUrl { get; set; } = string.Empty;

    [Url]
    [StringLength(500)]
    [Display(Name = "Headshot URL")]
    public string HeadshotUrl { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Speaker Type")]
    public int SpeakerTypeId { get; set; }

    [Display(Name = "Areas of Expertise")] public int[] SelectedExpertiseIds { get; set; } = Array.Empty<int>();

    [Display(Name = "Social Media Platforms")]
    public string[] SocialMediaPlatforms { get; set; } = Array.Empty<string>();

    [Display(Name = "Social Media URLs")] public string[] SocialMediaUrls { get; set; } = Array.Empty<string>();

    [Display(Name = "Upload Headshot")] public IFormFile? HeadshotFile { get; set; }
}
