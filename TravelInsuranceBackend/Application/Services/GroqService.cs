using Application.DTOs;
using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace Application.Services
{
    public class GroqService : IGroqService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model = "llama-3.3-70b-versatile"; // High performance model

        public GroqService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GroqApiKey"] ?? "";
        }

        public async Task<RecommendationResponseDTO> GetRecommendationAsync(RecommendationRequestDTO request, List<PolicyProductResponseDTO> availablePlans)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new Exception("Groq API Key is not configured in appsettings.json.");
            }

            var plansContext = JsonSerializer.Serialize(availablePlans.Select(p => new {
                p.PolicyProductId,
                p.PolicyName,
                p.PlanTier,
                p.PolicyType,
                p.CoverageLimit,
                p.BasePremium,
                p.CoverageDetails,
                p.DestinationZone
            }));

            var prompt = $@"
            You are a Professional Travel Insurance Consultant.
            Based on the following trip details and available insurance plans, recommend exactly ONE plan that best fits the traveler's needs.

            TRAVELER DETAILS:
            - Destination: {request.Destination}
            - Duration: {request.DurationDays} days
            - Age: {request.Age}
            - Purpose: {request.Purpose}
            - Budget: {request.Budget}

            AVAILABLE PLANS:
            {plansContext}

            RESPONSE FORMAT (Strict JSON):
            {{
                ""policyProductId"": number,
                ""policyProductName"": ""string"",
                ""reason"": ""A professional, empathetic explanation of why this plan was chosen. Mention specific risks like medical costs in {{request.Destination}} or age-related concerns."",
                ""keyFeatures"": [""feature 1"", ""feature 2"", ""feature 3""],
                ""estimatedPremium"": number,
                ""matchScore"": ""High""
            }}

            Rules:
            1. Only recommend a plan from the list above.
            2. High medical limits for USA/Canada (Platinum).
            3. Silver for budget trips to Asia.
            4. Gold for comprehensive global travel.
            5. Ensure the reason is persuasive and realistic.
            ";

            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "system", content = "You are a helpful travel insurance expert." },
                    new { role = "user", content = prompt }
                },
                response_format = new { type = "json_object" },
                temperature = 0.5
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var response = await _httpClient.PostAsync("https://api.groq.com/openai/v1/chat/completions", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Groq API Error: {error}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var aiResponseContent = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            if (string.IsNullOrEmpty(aiResponseContent))
                throw new Exception("AI returned an empty response.");

            var recommendation = JsonSerializer.Deserialize<RecommendationResponseDTO>(aiResponseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (recommendation == null)
                throw new Exception("Failed to parse AI recommendation.");

            return recommendation;
        }
    }
}
