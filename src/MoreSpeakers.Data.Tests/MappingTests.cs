using AutoMapper;

namespace MoreSpeakers.Data.Tests;

public class MappingTests
{
    [Fact]
    public void MappingProfile_IsValid()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfiles.MoreSpeakersProfile>();
        });

        try
        {
            // Throws an exception is something is bad
            configuration.AssertConfigurationIsValid();
            Assert.True(true);
        }
        catch (Exception)
        {
            Assert.True(false);
        }
    }
}