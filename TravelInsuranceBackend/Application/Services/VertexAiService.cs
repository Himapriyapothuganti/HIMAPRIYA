using Application.Interfaces.Services;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Application.Services
{
    public class VertexAiService : IVertexAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _projectId;
        private readonly string _location;

        public VertexAiService(
            HttpClient httpClient,
            IConfiguration configuration)
        {
            _httpClient = httpClient;
            _projectId = configuration[
                "VertexAI:ProjectId"]
                ?? "project-d7a3681b-c59c-4a32-aad";
            _location = configuration[
                "VertexAI:Location"]
                ?? "us-central1";
        }

        // ── shared helper ──────────────────
        private async Task<string> GetTokenAsync()
        {
            var credential = await GoogleCredential
                .GetApplicationDefaultAsync();
            if (credential.IsCreateScopedRequired)
                credential = credential.CreateScoped(
                    "https://www.googleapis.com/" +
                    "auth/cloud-platform");
            return await credential
                .UnderlyingCredential
                .GetAccessTokenForRequestAsync();
        }

        private string GetEndpoint() =>
            $"https://{_location}-aiplatform" +
            $".googleapis.com/v1/projects/" +
            $"{_projectId}/locations/{_location}" +
            $"/publishers/google/models/" +
            $"gemini-2.5-flash:generateContent";

        private async Task<string> SendRequestAsync(
            List<object> parts)
        {
            var token = await GetTokenAsync();

            var requestBody = new
            {
                contents = new[]
                {
                    new { role = "user", parts = parts }
                },
                generationConfig = new
                {
                    temperature = 0.2,
                    maxOutputTokens = 4096
                }
            };

            using var request = new HttpRequestMessage(
                HttpMethod.Post, GetEndpoint());
            request.Headers.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer", token);
            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8, "application/json");

            using var response = await _httpClient
                .SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content
                    .ReadAsStringAsync();
                Console.WriteLine(
                    $"Vertex AI Error: {err}");
                return $"AI Error " +
                    $"({response.StatusCode}): " +
                    $"Please check backend console.";
            }

            var responseJson = await response.Content
                .ReadAsStringAsync();

            using var doc = JsonDocument
                .Parse(responseJson);
            var partsEl = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts");

            var sb = new StringBuilder();
            foreach (var part in
                partsEl.EnumerateArray())
            {
                if (part.TryGetProperty(
                    "text", out var textProp))
                    sb.Append(textProp.GetString());
            }

            return sb.Length > 0
                ? sb.ToString()
                : "No summary generated.";
        }

        // ── Unified Analysis Method ────────
        public async Task<string>
        AnalyzeClaimAsync(
            string jsonClaimData,
            List<string> filePaths)
        {
            try
            {
                var parts = new List<object>
                {
                    new { text =
                        "You are a senior insurance claims analyst at Talk & Travel Insurance.\n" +
                        "Your task is to analyze the provided JSON claim data against any attached documents and return a STRICT JSON RESPONSE ONLY.\n\n" +
                        "### REQUIRED JSON STRUCTURE ###\n" +
                        "{\n" +
                        "  \"riskScore\": \"Low\" | \"Medium\" | \"High\",\n" +
                        "  \"riskReasoning\": \"Brief explanation of why this risk score was assigned\",\n" +
                        "  \"aiOpinion\": \"Senior analyst recommendation for approval, rejection, or further investigation\",\n" +
                        "  \"categories\": [\n" +
                        "    { \"title\": \"Identity Analysis\", \"summary\": \"Analysis of claimant identity vs document names\" },\n" +
                        "    { \"title\": \"Policy Alignment\", \"summary\": \"How the claim aligns with policy benefits and limits\" },\n" +
                        "    { \"title\": \"Evidence Quality\", \"summary\": \"Assessment of document timestamps, locations, and legitimacy\" },\n" +
                        "    { \"title\": \"Financial Validation\", \"summary\": \"Detailed check of requested amounts vs bill totals\" }\n" +
                        "  ],\n" +
                        "  \"documentComparison\": {\n" +
                        "    \"isConsistent\": true | false,\n" +
                        "    \"mismatchDetails\": \"Specific discrepancies found (e.g. Name or Date mismatch)\"\n" +
                        "  },\n" +
                        "  \"keyIssues\": [\"List up to 5 critical points found\"],\n" +
                        "  \"checkList\": [\"List manual verification steps for the officer\"],\n" +
                        "  \"chartData\": [\n" +
                        "    { \"label\": \"Requested Amount\", \"value\": number },\n" +
                        "    { \"label\": \"Verified Bill Total\", \"value\": number }\n" +
                        "  ]\n" +
                        "}\n\n" +
                        "### STRICT RULES - CRITICAL ###\n" +
                        "- Output ONLY the raw JSON string. DO NOT use markdown code blocks (```json). DO NOT use backticks.\n" +
                        "- Cross-reference every detail. If a name or policy number differs from the system record, flag as 'High' risk.\n" +
                        "- For 'chartData', use the actual numbers found in the claims data and documents. Ensure they are numbers, not strings.\n\n" +
                        "CLAIM JSON DATA:\n" + jsonClaimData }
                };

                if (filePaths != null)
                {
                    foreach (var path in filePaths)
                    {
                        if (!File.Exists(path))
                            continue;

                        var ext = Path
                            .GetExtension(path)
                            .ToLower();
                        var mime = ext switch
                        {
                            ".pdf"  => "application/pdf",
                            ".jpg"  => "image/jpeg",
                            ".jpeg" => "image/jpeg",
                            ".png"  => "image/png",
                            ".txt"  => "text/plain",
                            _ => "application/octet-stream"
                        };

                        var bytes = await File
                            .ReadAllBytesAsync(path);

                        parts.Add(new
                        {
                            inlineData = new
                            {
                                mimeType = mime,
                                data = Convert
                                    .ToBase64String(bytes)
                            }
                        });
                    }
                }

                return await SendRequestAsync(parts);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Vertex AI Exception: " +
                    $"{ex.Message}");
                if (ex.Message.Contains("403"))
                    return "AI Error (Forbidden): " +
                        "Check backend console.";
                return "AI analysis unavailable " +
                    "at the moment.";
            }
        }
    }
}
