using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Models.ViewModels;

public class SpeakerResultsViewModel
{
    /// <summary>
    /// The speakers that match the search criteria.
    /// </summary>
    public IEnumerable<User> Speakers { get; init; } = new List<User>();
    
    /// <summary>
    /// The current logged-in user, if any.
    /// </summary>
    public User? CurrentUser { get; set; }

    /// <summary>
    /// The search term used to filter the results.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// The type of speaker filter selected.
    /// </summary>
    public int? SpeakerTypeFilter { get; set; }

    /// <summary>
    /// The expertise filter selected.
    /// </summary>
    public List<int>? ExpertiseFilter { get; set; }

    /// <summary>
    /// The sort order selected.
    /// </summary>
    public SpeakerSearchOrderBy? SortBy { get; set; }

    /// <summary>
    /// The total number of pages of results.
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// The current page number.
    /// </summary>
    public int CurrentPage { get; set; } = 1;

    /// <summary>
    /// The view type selected.
    /// </summary>
    public SpeakerResultsViewType ViewType { get; set; } = SpeakerResultsViewType.CardView;
}