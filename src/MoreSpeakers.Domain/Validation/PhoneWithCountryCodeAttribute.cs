using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace MoreSpeakers.Domain.Validation;

public partial class PhoneWithCountryCodeAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
        {
            return ValidationResult.Success; // Allow null or empty values if not required
        }

        // Regex to check for country code (e.g., +1, +44, etc.)
        var regex = PhoneWithCountryCodeRegex();
        return !regex.IsMatch(value.ToString() ?? string.Empty)
            ? new ValidationResult("The phone number must include a valid country code (e.g., +1).")
            : ValidationResult.Success;
    }

    [GeneratedRegex(@"^\+\d{1,3}\s?\d+$")]
    private static partial Regex PhoneWithCountryCodeRegex();
}
