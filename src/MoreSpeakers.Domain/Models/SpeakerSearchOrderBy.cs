using System.ComponentModel;

namespace MoreSpeakers.Domain.Models;

/// <summary>
/// The sort order for the speaker search.
/// </summary>
public enum SpeakerSearchOrderBy
{
    [Description("Name")]
    Name,
    [Description("Newest Speakers")]
    Newest,
    [Description("Most Expertise")]
    Expertise
}