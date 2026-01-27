using System.ComponentModel.DataAnnotations;

using MoreSpeakers.Domain.Extensions;

namespace MoreSpeakers.Domain.Models;

public class SpeakerType
{
    public int Id { get; set; }

    [Required] [MaxLength(50)] public string Name { get; set; } = string.Empty;

    [Required] [MaxLength(200)] public string Description { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<User> Users { get; set; } = [];

    public string ToFriendlyName()
    {
        return Enum.TryParse(Id.ToString(), out SpeakerTypeEnum parsedValue)
            ? parsedValue.GetDescription()
            : string.Empty;
    }
}