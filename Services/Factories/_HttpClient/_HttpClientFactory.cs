//namespace ReadVideo.Server.Services.Factories.HttpClient
//{
//    using System;
//    using System.Collections.Concurrent;
//    using System.Net.Http;
//    using Microsoft.Extensions.Logging;

//    public class HttpClientFactory : IHttpClientFactory
//    {
//        private readonly ConcurrentDictionary<string, HttpClient> _clients = new ConcurrentDictionary<string, HttpClient>();
//        private readonly ILogger<HttpClientFactory> _logger;

//        public HttpClientFactory(ILogger<HttpClientFactory> logger)
//        {
//            _logger = logger;
//        }

//        public HttpClient CreateClient(string name)
//        {
//            if (string.IsNullOrEmpty(name))
//            {
//                throw new ArgumentException("Client name must be provided", nameof(name));
//            }

//            return _clients.GetOrAdd(name, _ =>
//            {
//                _logger.LogInformation($"Creating new HttpClient instance for {name}.");
//                var client = new HttpClient
//                {
//                    // Configure the HttpClient instance if needed (e.g., BaseAddress, DefaultRequestHeaders)
//                };

//                // Optional: Dispose of HttpClient when it's no longer needed
//                // This example does not implement a disposal strategy

//                return client;
//            });
//        }
//    }

//}
