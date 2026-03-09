using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class PolicyRequest
    {
        public int PolicyRequestId { get; set; }
        public int PolicyProductId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string? AgentId { get; set; }

        public string Destination { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        
        public string TravellerName { get; set; } = string.Empty;
        public int TravellerAge { get; set; }
        public string PassportNumber { get; set; } = string.Empty;
        
        public string KycType { get; set; } = string.Empty;
        public string KycNumber { get; set; } = string.Empty;

        // Risk Assessment
        public int RiskScore { get; set; }
        public int RiskAgeScore { get; set; }
        public int RiskDestinationScore { get; set; }
        public int RiskDurationScore { get; set; }
        public int RiskTierScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending"; // Pending / Approved / Rejected / Purchased
        public string? RejectionReason { get; set; }
        public string? AgentNotes { get; set; }
        
        public decimal CalculatedPremium { get; set; }
        
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        // Navigation
        public PolicyProduct? PolicyProduct { get; set; }
        public ApplicationUser? Customer { get; set; }
        public ApplicationUser? Agent { get; set; }
        public ICollection<PolicyRequestDocument> Documents { get; set; } = new List<PolicyRequestDocument>();
    }
}
