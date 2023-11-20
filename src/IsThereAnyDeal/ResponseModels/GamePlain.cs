using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Asiago.ResponseModels
{
    internal class GamePlain
    {
        [JsonProperty(".meta")]
        public GamePlainMeta Meta { get; set; } = null!;

        [JsonConverter(typeof(GamePlainDataJsonConvertor))]
        public GamePlainData? Data { get; set; }
    }

    internal class GamePlainMeta
    {
        // The api either returns a match type string like like "title" or, if there's no match, a falsey bool.
        // The deserialization will still be able to process this bool and will set this to the string "False".
        public string Match { get; set; } = null!;
        public bool Active { get; set; }
    }

    internal class GamePlainData
    {
        public string Plain { get; set; } = null!;
    }

    // The api either returns an object containing the plain or, if there's no match, an empty array.
    // We'll just set our Data value to null in the latter case.
    internal class GamePlainDataJsonConvertor : JsonConverter
    {
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);
            return token is JObject ? token.ToObject<GamePlainData>() : null;
        }

        public override bool CanConvert(Type objectType) => throw new NotImplementedException();
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer) => throw new NotImplementedException();
    }
}
