using Asiago.Common.Exceptions;

namespace Asiago.Extensions
{
    /// <summary>
    /// Extension methods on <see cref="IConfiguration"/>.
    /// </summary>
    internal static class IConfigurationExtensions
    {
        /// <summary>
        /// Extracts the value with the specified <paramref name="key"/> and converts it to type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type to convert the value to.</typeparam>
        /// <param name="config">The config.</param>
        /// <param name="key">The key of the value to extract and convert.</param>
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
