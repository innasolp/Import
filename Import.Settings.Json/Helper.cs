using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;

namespace Import.Settings.Json;

internal static class Helper
{
    public static bool TryDeserialize<T>(this string json, out T? result)
    {
        try
        {
            result = JsonSerializer.Deserialize<T>(json);
            return result != null;
        }
        catch (JsonException)
        {
            result = default;
            return false;
        }
    }

    public static bool TryParse<T>(this string str, [MaybeNullWhen(false)] out T value)
        where T : IParsable<T>
    {
        return T.TryParse(str, CultureInfo.InvariantCulture, out value);
    }

    public static bool TryParseDynamic<T>(this string input, out T? result)
    {
        bool isParsable = typeof(T).GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IParsable<>));

        if (isParsable)
        {
            var method = typeof(T).GetMethod("TryParse",
                    [typeof(string), typeof(IFormatProvider), typeof(T).MakeByRefType()]);

            if (method != null)
            {
                var parameters = new object?[] { input, CultureInfo.InvariantCulture, null };
                bool success = (bool)method.Invoke(null, parameters)!;

                result = success ? (T)parameters[2]! : default;
                return success;
            }
        }

        result = default;
        return false;
    }
}