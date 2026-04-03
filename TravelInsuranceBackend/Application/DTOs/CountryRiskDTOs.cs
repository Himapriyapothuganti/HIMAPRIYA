using System;

namespace Application.DTOs
{
    public class CountryRiskDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Multiplier { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateCountryRiskDTO
    {
        public string Name { get; set; } = string.Empty;
        public decimal Multiplier { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCountryRiskDTO
    {
        public decimal Multiplier { get; set; }
        public bool IsActive { get; set; }
    }

    public class PremiumCalculationRequestDTO
    {
        public int PolicyProductId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TravellerAge { get; set; }
        public string Destination { get; set; } = string.Empty;
        public int MemberCount { get; set; } = 1;
    }

    public class PremiumCalculationResponseDTO
    {
        public decimal EstimatedPremium { get; set; }
        public decimal DestinationMultiplier { get; set; }
        public decimal AgeLoading { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
    }
}
