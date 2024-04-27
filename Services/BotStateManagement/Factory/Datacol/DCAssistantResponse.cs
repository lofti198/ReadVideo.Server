using Newtonsoft.Json;

namespace ReadVideo.Server.Services.BotStateManagement.Factory.Datacol
{

    public class DCAssistantResponse
    {
        [JsonProperty("welcome")]
        [JsonConverter(typeof(StringToBooleanConverter))]
        public bool Welcome { get; set; } = false;

        [JsonProperty("critical")]
        [JsonConverter(typeof(StringToBooleanConverter))]
        public bool Critical { get; set; } = false;

        [JsonProperty("found")]
        [JsonConverter(typeof(StringToBooleanConverter))]
        public bool Found { get; set; } = false;

        [JsonProperty("response")]
        public string Response { get; set; } = string.Empty;

    }

    public class StringToBooleanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(bool);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string value = reader.Value as string;
            return !string.IsNullOrEmpty(value) && value != "0";
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            bool boolValue = (bool)value;
            writer.WriteValue(boolValue ? "..." : "0");
        }
    }
}
