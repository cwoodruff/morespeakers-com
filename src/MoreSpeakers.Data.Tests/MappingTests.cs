using AutoMapper;

using Microsoft.Extensions.Logging;

namespace MoreSpeakers.Data.Tests;

public class MappingTests
{
    [Fact]
    public void MappingProfile_IsValid()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfiles.MoreSpeakersProfile>();
        }, new LoggerFactory());

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