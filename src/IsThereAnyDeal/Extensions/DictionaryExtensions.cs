namespace Asiago.Extensions
{
    internal static class DictionaryExtensions
    {
        public static string ToQueryString(this Dictionary<string, string> queryParameters)
        {
            if (queryParameters.Count == 0)
            {
                return "";
            }

            var encodedQueryParameters = queryParameters.Select(item => $"{Uri.EscapeDataString(item.Key)}={Uri.EscapeDataString(item.Value)}");
            return $"?{string.Join("&", encodedQueryParameters)}";
        }
    }
}
