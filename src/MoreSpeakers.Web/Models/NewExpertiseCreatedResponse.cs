using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Models;

public class NewExpertiseCreatedResponse
{
    /// <summary>
    /// Indicates if the saving of a new expertise failed
    /// </summary>
    /// <remarks>
    /// There are 3 states for this:
    /// null, meaning a save was not attempts
    /// true, save was successful
    /// false, save failed
    /// </remarks>
    public bool? SavingExpertiseFailed { get; set; }
    public string SaveExpertiseMessage { get; set; } = string.Empty;
    
    public IEnumerable<ExpertiseCategory> ExpertiseCategories { get; set; } = [];

    public IEnumerable<Sector> Sectors { get; set; } = [];
}