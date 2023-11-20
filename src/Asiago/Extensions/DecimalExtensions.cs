using System.Globalization;

namespace Asiago.Extensions
{
    public static class DecimalExtensions
    {
        public static string ToFormattedPrice(this decimal d, string countryCode)
        {
            var cultureInfo = CultureInfo.GetCultureInfo($"{CultureInfo.InvariantCulture.TwoLetterISOLanguageName}-{countryCode}");
            RegionInfo regionInfo = new(cultureInfo.Name);
            return $"{d.ToString("C", cultureInfo)} {regionInfo.ISOCurrencySymbol}";
        }
    }
}
