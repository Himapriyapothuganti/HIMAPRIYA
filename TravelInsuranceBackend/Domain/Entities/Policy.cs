using Domain.Enums;

namespace Domain.Entities
{
    public class Policy
    {
        public int PolicyId { get; set; }
        public int PolicyProductId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string? AgentId { get; set; }
        public string Destination { get; set; } = string.Empty;
        public string PolicyType { get; set; } = string.Empty;
        public string PlanTier { get; set; } = string.Empty;

        // ── Traveller Details ─────────────────────────────
        public string TravellerName { get; set; } = string.Empty;
        public int TravellerAge { get; set; }
        public string PassportNumber { get; set; } = string.Empty;

        // ── KYC ──────────────────────────────────────────
        public string KycType { get; set; } = string.Empty;   // PAN / Aadhaar / CKYC
        public string KycNumber { get; set; } = string.Empty;

        public decimal PremiumAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PolicyStatus Status { get; set; } = PolicyStatus.PendingPayment;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Navigation ────────────────────────────────────
        public PolicyProduct PolicyProduct { get; set; } = null!;
        public ICollection<Claim> Claims { get; set; } = new List<Claim>();
    }
}