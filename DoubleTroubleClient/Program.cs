using System;
using System.Net.Http;
using System.Threading.Tasks;
using SFA.DAS.Http;
using SFA.DAS.Http.Configuration;
using SFA.DAS.Http.TokenGenerators;

namespace DoubleTroubleClient
{
    internal class Program
    {
        public static async Task Main(string[] args)
        {
            var jwtHttpClient = GetHttpClient();

            var result = await jwtHttpClient.GetAsync("api/todolist");
        }
        
        private static HttpClient GetHttpClient()
        {
            var config = new JwtClientConfiguration
            {
                BaseUrl = "https://localhost:44321/",
                ClientToken = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJkYXRhIjoiUm9sZTEiLCJpc3MiOiJodHRwOi8vZXhhbXBsZS5jb20iLCJhdWQiOiJkb3VibGVhdXRocG9jIiwiZXhwIjoxNTkwOTI3MTYwLCJuYmYiOjE1NTg1MjcxNjB9.y3547L3UyGby-EzGBWO4IQAX16wVQA0FnrwQHltlXdA"
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