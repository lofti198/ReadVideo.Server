namespace ReadVideo.Server.Data
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class BotResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } // For UNIQUE_GUID

        [JsonPropertyName("client_id")]
        public string ClientId { get; set; } // CLIENT_ID_FROM_REQUEST

        [JsonPropertyName("chat_id")]
        public string ChatId { get; set; } // CHAT_ID_FROM_REQUEST

        [JsonPropertyName("message")]
        public BotMessage Message { get; set; } // Nested message object

        [JsonPropertyName("event")]
        public string Event { get; set; } // "BOT_MESSAGE"

        public BotResponse()
        {
            // Initializing the Message object to ensure it's not null
            Message = new BotMessage();
        }
    }

    public class BotMessage
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "TEXT"; // Defaulting to "TEXT"

        [JsonPropertyName("text")]
        public string Text { get; set; } // The message text

        [JsonPropertyName("content")]
        public string Content { get; set; } // The message text

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; } // CURRENT_TIME_STAMP
    }

    // Extend BotMessage to include properties for a message with buttons
    public class BotMessageWithButtons : BotMessage
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } // Title for the buttons message

        [JsonPropertyName("force_reply")]
        public bool ForceReply { get; set; } // Whether a reply is forced

        [JsonPropertyName("buttons")]
        public List<Button> Buttons { get; set; } = new List<Button>(); // List of buttons
    }

    // Define a Button class
    public class Button
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } // Button text

        [JsonPropertyName("id")]
        public int Id { get; set; } // Button ID
    }


    public class BotMessageConverter : JsonConverter<BotMessage>
    {
        public override BotMessage Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException("Deserialization is not supported.");
        }

        public override void Write(Utf8JsonWriter writer, BotMessage value, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, (object)value, options);
        }
    }
}
