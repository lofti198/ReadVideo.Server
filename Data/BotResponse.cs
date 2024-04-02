namespace ReadVideo.Server.Data
{
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

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; } // CURRENT_TIME_STAMP
    }

}
