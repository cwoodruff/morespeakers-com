using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Models.ViewModels;

public class ExpertiseListDisplayViewModel
{
    /// <summary>
    /// A list of all the expertise available for selection
    /// </summary>
    public IEnumerable<Expertise> AvailableExpertises { get; set; } = [];
    
    /// <summary>
    /// An array of the current expertise ids selected by the user
    /// </summary>
    public int[] SelectedExpertiseIds { get; set; } = [];
    
    /// <summary>
    /// The expertise categories
    /// </summary>
    public IEnumerable<ExpertiseCategory> ExpertiseCategories { get; set; } = [];
}