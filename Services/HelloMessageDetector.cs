using Newtonsoft.Json;
using ReadVideo.Server.Data;
using ReadVideo.Server.Services.AIAssistants;
using System.Text.Json.Serialization;

namespace ReadVideo.Server.Services
{
    public class HelloMessageDetector : IHelloMessageDetector
    {
        private readonly IAssistant _assistant;

        public HelloMessageDetector(IAssistant assistant)
        {
            _assistant = assistant;
        }
        public async Task<bool>IsHelloMessage(string text)
        {
            string response = await _assistant.GetResponseAsync(text, "",
                Consts.GeneralOpenAIAssistantID_HelloDetector, "0");

            bool isComplex = ParseComplexValue(response);
            return !isComplex;
        }

        private static bool ParseComplexValue(string jsonString)
        {
            try
            {
                JsonStructure parsedObject = JsonConvert.DeserializeObject<JsonStructure>(jsonString);
                return parsedObject.Complex;
            }
            catch (Exception ex)
            {
                // Handle or log parsing errors
                Console.WriteLine("Error parsing JSON: " + ex.Message);
                return false; // Or handle this scenario as needed
            }
        }
        public class JsonStructure
        {
            [JsonPropertyName("complex")]
            public bool Complex { get; set; }
        }
    }
    
    public interface IHelloMessageDetector
    {
        Task<bool> IsHelloMessage(string text);
    }
}
