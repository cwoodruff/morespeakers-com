using AutoMapper;

namespace MoreSpeakers.Data.MappingProfiles;

public class MoreSpeakersProfile : Profile
{
    public MoreSpeakersProfile()
    {
        CreateMap<Models.Expertise, Domain.Models.Expertise>().ReverseMap();
        CreateMap<Models.Mentorship, Domain.Models.Mentorship>().ReverseMap();
        CreateMap<Models.MentorshipExpertise, Domain.Models.MentorshipExpertise>().ReverseMap();
        CreateMap<Models.MentorshipStatus, Domain.Models.MentorshipStatus>().ReverseMap();
        CreateMap<Models.MentorshipType, Domain.Models.MentorshipType>().ReverseMap();
        CreateMap<Models.SocialMedia, Domain.Models.SocialMedia>().ReverseMap();
        CreateMap<Models.SpeakerType, Domain.Models.SpeakerType>().ReverseMap();
        CreateMap<Models.User, Domain.Models.User>().ReverseMap();
        CreateMap<Models.UserExpertise, Domain.Models.UserExpertise>().ReverseMap();
    }
}