using System.ComponentModel;

namespace MoreSpeakers.Domain.Extensions;

public static class EnumExtensions
{
    public static string GetDescription<T>(this T enumValue) 
        where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum)
            return String.Empty;

        var description = enumValue.ToString();
        var fieldInfo = enumValue.GetType().GetField(enumValue.ToString() ?? string.Empty);

        if (fieldInfo != null)
        {
            var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attrs.Length > 0)
            {
                description = ((DescriptionAttribute)attrs[0]).Description;
            }
        }

        return description ?? String.Empty;
    }
}