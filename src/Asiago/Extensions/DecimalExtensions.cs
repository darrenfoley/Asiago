using System.Globalization;

namespace Asiago.Extensions
{
    /// <summary>
    /// Extension methods on <see cref="decimal"/>.
    /// </summary>
    internal static class DecimalExtensions
    {
        /// <summary>
        /// Converts <paramref name="d"/> to a formatted price for the country indicated by <paramref name="countryCode"/>
        /// </summary>
        public static string ToFormattedPrice(this decimal d, string countryCode)
        {
            var cultureInfo = CultureInfo.GetCultureInfo($"{CultureInfo.InvariantCulture.TwoLetterISOLanguageName}-{countryCode}");
            RegionInfo regionInfo = new(cultureInfo.Name);
            return $"{d.ToString("C", cultureInfo)} {regionInfo.ISOCurrencySymbol}";
        }
    }
}
