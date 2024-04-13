using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.Net.Http.Headers;

namespace ReadVideo.Server.Services
{
    public class TokenToServiceKeyConverter
    {
        public string Convert(string token)
        {
            token = token.ToLower();
            string retVal = token switch
            {
                "datacol" => "DC",
                "startspeaking" => "START_SPEAK",
                _ => "NO"  // Default case
            };
            return retVal;
        }
    }
}