namespace MoreSpeakers.Domain.Models;

/// <summary>
/// The results of a speaker search.
/// </summary>
public class SpeakerSearchResult
{
    public required IEnumerable<User> Speakers { get; set; }
    public int RowCount { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage {get; set;}
}