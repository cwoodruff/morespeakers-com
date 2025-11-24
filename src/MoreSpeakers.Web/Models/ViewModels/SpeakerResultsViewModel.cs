namespace MoreSpeakers.Web.Models.ViewModels;

public class SpeakerResultsViewModel
{
    /// <summary>
    ///  Indicates whether the search was for speakers or mentors.
    /// </summary>
    public SearchType SearchType { get; set; }
    /// <summary>
    /// The speakers that match the search criteria.
    /// </summary>
    public IEnumerable<Domain.Models.User> Speakers { get; init; } = new List<Domain.Models.User>();
    
    /// <summary>
    /// The current logged-in user, if any.
    /// </summary>
    public Domain.Models.User? CurrentUser { get; set; }
    
    /// <summary>
    /// The total number of pages of results.
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// The current page number.
    /// </summary>
    public int CurrentPage { get; set; } = 1;
    
    
}