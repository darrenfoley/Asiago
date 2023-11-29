using Asiago.Exceptions;
using Microsoft.Extensions.Configuration;

namespace Asiago.Extensions
{
    internal static class IConfigurationRootExtensions
    {
        public static T GetRequiredValue<T>(this IConfigurationRoot config, string key)
        {
            if (config.GetValue<T>(key) is T value)
            {
                return value;
            }
            throw new ConfigurationException($"The environment variable {key} must be set");
        }
    }
}
