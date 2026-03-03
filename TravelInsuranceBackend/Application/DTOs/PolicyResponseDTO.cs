namespace Application.DTOs
{
    public class PolicyResponseDTO
    {
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? AgentId { get; set; }
        public string? AgentName { get; set; }
        public int PolicyProductId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public string PolicyType { get; set; } = string.Empty;
        public string PlanTier { get; set; } = string.Empty;
        public string Destination { get; set; } = string.Empty;

        // ── Traveller Details ─────────────────────────────
        public string TravellerName { get; set; } = string.Empty;
        public int TravellerAge { get; set; }
        public string PassportNumber { get; set; } = string.Empty;

        // ── KYC ──────────────────────────────────────────
        public string KycType { get; set; } = string.Empty;
        public string KycNumber { get; set; } = string.Empty;

        public decimal PremiumAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }


        // ── Claims (used in Agent Policy Detail) ─────────
        public List<ClaimSummaryDTO> Claims { get; set; } = new();

        public string CoverageDetails { get; set; } = string.Empty;
    }
}