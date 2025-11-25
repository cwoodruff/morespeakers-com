namespace MoreSpeakers.Domain.Tests;

public class PhoneWithCountryCodeAttributeTests
{
    [Fact]
    public void ValidatesSuccessWithCountryCode()
    {
        var attribute = new Validation.PhoneWithCountryCodeAttribute();
        var result = attribute.IsValid("+1 1234567890");
        Assert.True(result);
    }

    [Fact]
    public void ValidatesFailureWithoutCountryCode()
    {
        var attribute = new Validation.PhoneWithCountryCodeAttribute();
        var result = attribute.IsValid("1234567890");
        Assert.False(result);
    }

    [Fact]
    public void ValidatesSuccessWithEmpty()
    {
        var attribute = new Validation.PhoneWithCountryCodeAttribute();
        var result = attribute.IsValid(string.Empty);
        Assert.True(result);
    }

    [Fact]
    public void ValidatesSuccessWithNull()
    {
        var attribute = new Validation.PhoneWithCountryCodeAttribute();
        var result = attribute.IsValid(null);
        Assert.True(result);
    }
}