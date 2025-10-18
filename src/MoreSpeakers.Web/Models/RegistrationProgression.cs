namespace MoreSpeakers.Web.Models;

//public enum RegistrationProgression
//{
//    Unknown,
//    /// <summary>1</summary>
//    SpeakerProfileNeeded,
//    /// <summary>2</summary>
//    RequiredInformationNeeded,
//    /// <summary>3</summary>
//    ExpertiseNeeded,
//    /// <summary>4</summary>
//    SocialMediaNeeded,
//}
public static class RegistrationProgressions
{
    /// <summary>1</summary>
    public const int SpeakerProfileNeeded = 1;
    /// <summary>2</summary>
    public const int RequiredInformationNeeded = 2;
    /// <summary>3</summary>
    public const int ExpertiseNeeded = 3;
    /// <summary>4</summary>
    public const int SocialMediaNeeded = 4;
    /// <summary>5</summary>
    public const int Complete = 5;

    public static readonly int[] All =
    [
        SpeakerProfileNeeded,
        RequiredInformationNeeded,
        ExpertiseNeeded,
        SocialMediaNeeded
    ];
    public static bool IsValid(int value) => All.Contains(value);
}
