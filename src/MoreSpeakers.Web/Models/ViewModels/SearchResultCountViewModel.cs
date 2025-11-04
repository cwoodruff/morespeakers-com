namespace MoreSpeakers.Web.Models.ViewModels;

public class SearchResultCountViewModel
{
    /// <summary>
    /// The total number of search results.
    /// </summary>
    public int TotalResults { get; set; }
    
    /// <summary>
    /// Indicates whether any filters are currently applied to the search results.
    /// </summary>
    public bool AreFiltersApplied { get; set; }
}