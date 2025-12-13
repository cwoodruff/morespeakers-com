namespace MoreSpeakers.Domain.Interfaces;

/// <summary>
/// The Pagination settings.
/// </summary>
public interface IPaginationSettings
{
    public int StartPage { get; set; }
    
    public int MinimalPageSize { get; set; }
    
    public int MaximizePageSize { get; set; }
}