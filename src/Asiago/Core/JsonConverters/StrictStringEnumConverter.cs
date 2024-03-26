using Newtonsoft.Json.Converters;

namespace Asiago.Core.JsonConverters
{
    /// <summary>
    /// Converts an System.Enum to and from its name string value without allowing integer values.
    /// </summary>
    internal class StrictStringEnumConverter : StringEnumConverter
    {
        public StrictStringEnumConverter()
        {
            AllowIntegerValues = false;
        }
    }
}
