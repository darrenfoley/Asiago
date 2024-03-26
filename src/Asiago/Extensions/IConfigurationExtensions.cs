using Asiago.Common.Exceptions;

namespace Asiago.Extensions
{
    internal static class IConfigurationExtensions
    {
        public static T GetRequiredValue<T>(this IConfiguration config, string key)
        {
            if (config.GetValue<T>(key) is T value)
            {
                return value;
            }
            throw new ConfigurationException($"The environment variable {key} must be set");
        }
    }
}
