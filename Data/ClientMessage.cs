namespace ReadVideo.Server.Data
{
    using System.Text.Json.Serialization;

    public class ClientMessage
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("site_id")]
        public string SiteId { get; set; }

        [JsonPropertyName("client_id")]
        public string ClientId { get; set; }

        [JsonPropertyName("chat_id")]
        public string ChatId { get; set; }

        [JsonPropertyName("agents_online")]
        public bool AgentsOnline { get; set; }

        [JsonPropertyName("sender")]
        public Sender Sender { get; set; }

        [JsonPropertyName("message")]
        public Message Message { get; set; }

        [JsonPropertyName("channel")]
        public Channel Channel { get; set; }

        [JsonPropertyName("event")]
        public string Event { get; set; }
    }

    public class Sender
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("has_contacts")]
        public bool HasContacts { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }

        [JsonPropertyName("timestamp")]
        public long Timestamp { get; set; }
    }

    public class Channel
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }
    }


}
