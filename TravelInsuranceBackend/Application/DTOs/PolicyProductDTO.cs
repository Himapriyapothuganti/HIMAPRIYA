using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class CreatePolicyProductDTO
    {
        [Required]
        public string PolicyName { get; set; } = string.Empty;

        [Required]
        public string PolicyType { get; set; } = string.Empty;

        [Required]
        public string PlanTier { get; set; } = string.Empty;

        [Required]
        public string CoverageDetails { get; set; } = string.Empty;

        public string? ExclusionDetails { get; set; }

        [Required]
        public decimal CoverageLimit { get; set; }

        [Required]
        public decimal BasePremium { get; set; }

        [Required]
        public int Tenure { get; set; }

        [Required]
        public decimal ClaimLimit { get; set; }

        [Required]
        public string DestinationZone { get; set; } = string.Empty;
    }

    public class PolicyProductResponseDTO
    {
        public int PolicyProductId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public string PolicyType { get; set; } = string.Empty;
        public string PlanTier { get; set; } = string.Empty;
        public string CoverageDetails { get; set; } = string.Empty;
        public string? ExclusionDetails { get; set; }
        public decimal CoverageLimit { get; set; }
        public decimal BasePremium { get; set; }
        public int Tenure { get; set; }
        public decimal ClaimLimit { get; set; }
        public string DestinationZone { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}