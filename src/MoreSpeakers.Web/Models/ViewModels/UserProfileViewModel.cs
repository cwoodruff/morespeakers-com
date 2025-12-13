using System.ComponentModel.DataAnnotations;

using MoreSpeakers.Domain.Validation;

namespace MoreSpeakers.Web.Models.ViewModels;

public class UserProfileViewModel
{
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
    public Domain.Models.SpeakerTypeEnum SpeakerTypeId { get; set; }

    [Display(Name = "Areas of Expertise")] 
    public int[] SelectedExpertiseIds { get; set; } = [];
    
    [Display(Name = "New Expertise")] 
    public string NewExpertise { get; set; } = string.Empty;
        
    [Display(Name = "New Expertise Category")] 
    public int NewExpertiseCategoryId { get; set; } = 1;

    [Display(Name = "Social Media Sites")] 
    public List<Domain.Models.UserSocialMediaSite> UserSocialMediaSites { get; set; } = [];
}