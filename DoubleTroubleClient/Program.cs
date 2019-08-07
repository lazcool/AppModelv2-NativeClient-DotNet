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
            var jwtHttpClient = GetJwtHttpClient();

            var result = await jwtHttpClient.GetAsync("api/todolist");

            var aadHttpClient = CreateAadHttpClient();
            
            var result2 = await aadHttpClient.GetAsync("api/todolist");

            var noAuthHttpClient = CreateNoAuthHttpClient();

            var result3 = await noAuthHttpClient.GetAsync("api/todolist");
        }
        
        public static HttpClient GetJwtHttpClient()
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
        
        public static HttpClient CreateAadHttpClient()
        {
            var config = new AzureActiveDirectoryClientConfiguration
            {
                ApiBaseUrl = "https://localhost:44321/",
                //application (client) id of client
                ClientId = "b96f7d5c-4d3f-429f-ac8d-84e749ddc379",
                // client secret of client
                ClientSecret = "stick it in ere",
                IdentifierUri = "api://2420de76-7412-4610-8e2c-86fce1c72195",
                Tenant = "069530a2-5bbd-4901-ba99-ecd2a0e9a41c"
            };
            
            var httpClient = new HttpClientBuilder()
                .WithDefaultHeaders()
                .WithBearerAuthorisationHeader(new AzureActiveDirectoryBearerTokenGenerator(config))
                .Build();

            httpClient.BaseAddress = new Uri(config.ApiBaseUrl);

            return httpClient;
        }
        
        public static HttpClient CreateNoAuthHttpClient()
        {
            var httpClient = new HttpClientBuilder()
                .WithDefaultHeaders()
                .Build();

            httpClient.BaseAddress = new Uri("https://localhost:44321/");

            return httpClient;
        }
    }
    
    public class JwtClientConfiguration : IJwtClientConfiguration
    {
        public string BaseUrl { get; set; }
        public string ClientToken { get; set; }
    }

    public class AzureActiveDirectoryClientConfiguration : IAzureActiveDirectoryClientConfiguration
    {
        public string ApiBaseUrl { get; set; }
        public string Tenant { get; set;}
        public string ClientId { get; set;}
        public string ClientSecret { get; set;}
        public string IdentifierUri { get; set;}
    }
}