using MoreSpeakers.Domain.Models;

namespace MoreSpeakers.Web.Models;

/// <summary>
/// The response to a request to create a new expertise
/// </summary>
public class NewExpertiseResponse
{
    /// <summary>
    /// Indicates if we can create one or not
    /// </summary>
    public bool IsValid { get; set; }
    
    /// <summary>
    /// A message response
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// A list of expertises that may match
    /// </summary>
    public List<Expertise> Expertises { get; set; } = [];
}