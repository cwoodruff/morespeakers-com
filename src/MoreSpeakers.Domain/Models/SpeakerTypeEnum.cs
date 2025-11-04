using System.ComponentModel;

namespace MoreSpeakers.Domain.Models;

public enum SpeakerTypeEnum
{
    [Description("New Speaker")]
    NewSpeaker = 1,
    
    [Description("Experienced Speaker")]
    ExperiencedSpeaker = 2
}