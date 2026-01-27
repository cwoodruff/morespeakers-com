using AutoMapper;

namespace MoreSpeakers.Data.MappingProfiles;

public class MoreSpeakersProfile : Profile
{
    public MoreSpeakersProfile()
    {
        CreateMap<Models.Expertise, Domain.Models.Expertise>().ReverseMap();
        CreateMap<Models.ExpertiseCategory, Domain.Models.ExpertiseCategory>().ReverseMap();
        CreateMap<Models.Mentorship, Domain.Models.Mentorship>().ReverseMap();
        CreateMap<Models.MentorshipExpertise, Domain.Models.MentorshipExpertise>().ReverseMap();
        CreateMap<Models.MentorshipStatus, Domain.Models.MentorshipStatus>().ReverseMap();
        CreateMap<Models.MentorshipType, Domain.Models.MentorshipType>().ReverseMap();
        CreateMap<Models.Sector, Domain.Models.Sector>().ReverseMap();
        CreateMap<Models.SpeakerType, Domain.Models.SpeakerType>().ReverseMap();
        CreateMap<Models.User, Domain.Models.User>()
            .ForMember(dest => dest.MustChangePassword, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
            .ReverseMap();
        CreateMap<Models.UserExpertise, Domain.Models.UserExpertise>().ReverseMap();
        CreateMap<Models.SocialMediaSite, Domain.Models.SocialMediaSite>().ReverseMap();
        CreateMap<Models.UserSocialMediaSites, Domain.Models.UserSocialMediaSite>().ReverseMap();
        CreateMap<Models.EmailTemplate, Domain.Models.EmailTemplate>().ReverseMap();
    }
}