using System.Collections.Generic;

namespace Application.DTOs
{
    public class RecommendationRequestDTO
    {
        public string Destination { get; set; } = string.Empty;
        public int DurationDays { get; set; }
        public int Age { get; set; }
        public string Purpose { get; set; } = "Leisure"; // Leisure, Business, Adventure
        public string Budget { get; set; } = "Medium"; // Low, Medium, High
    }

    public class RecommendationResponseDTO
    {
        public int PolicyProductId { get; set; }
        public string PolicyProductName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public List<string> KeyFeatures { get; set; } = new List<string>();
        public decimal EstimatedPremium { get; set; }
        public string MatchScore { get; set; } = "High"; // High, Medium
    }
}
