using System;
using System.Net.Http;
using SFA.DAS.Http;
using SFA.DAS.Http.Configuration;
using SFA.DAS.Http.TokenGenerators;

namespace DoubleTroubleClient
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var jwtHttpClient = GetHttpClient();
        }
        
        private static HttpClient GetHttpClient()
        {
            var config = new JwtClientConfiguration
            {
                BaseUrl = "https://localhost:44321/",
                ClientToken = "don't check in!"
            };

            var httpClient = new HttpClientBuilder()
                .WithBearerAuthorisationHeader(new JwtBearerTokenGenerator(config))
                .WithDefaultHeaders()
                .Build();

            httpClient.BaseAddress = new Uri(config.BaseUrl);
            
            return httpClient;
        }
    }
    
    public class JwtClientConfiguration : IJwtClientConfiguration
    {
        public string BaseUrl { get; set; }
        public string ClientToken { get; set; }
    }

}