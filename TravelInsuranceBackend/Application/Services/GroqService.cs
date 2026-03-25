using Microsoft.Extensions.Configuration;

namespace Application.Services
{
    public class GroqService : Application.Interfaces.IGroqService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;

        public GroqService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GroqApiKey"] ?? "";
        }
    }
}
