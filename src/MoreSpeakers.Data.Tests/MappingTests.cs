using AutoMapper;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Xunit;

namespace MoreSpeakers.Data.Tests;

public class MappingTests
{
    private readonly ILoggerFactory _loggerFactory = new NullLoggerFactory();

    [Fact]
    public void MappingProfile_IsValid()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfiles.MoreSpeakersProfile>();
        }, _loggerFactory);

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