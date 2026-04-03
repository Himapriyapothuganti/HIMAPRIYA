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

        public string? Dependents { get; set; }
        public string? UniversityName { get; set; }
        public string? StudentId { get; set; }
        public string? TripFrequency { get; set; }

        // Risk Assessment
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty;
        public string? RiskReasoning { get; set; }
        public decimal DestinationRiskMultiplier { get; set; }

        public string Status { get; set; } = "Pending"; // Pending / Approved / Rejected / Purchased
        public string? RejectionReason { get; set; }
        public string? AgentNotes { get; set; }
        
        public decimal CalculatedPremium { get; set; }
        
        // Resubmission Tracking
        public int ResubmissionCount { get; set; } = 0;
        public int MaxResubmissions { get; set; } = 2;
        public string? RequestedDocTypes { get; set; } // Flagged docs (e.g. "KYC,Passport")

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        public string? AiAnalysisJson { get; set; }
        
        // Navigation
        public PolicyProduct? PolicyProduct { get; set; }
        public ApplicationUser? Customer { get; set; }
        public ApplicationUser? Agent { get; set; }
        public ICollection<PolicyRequestDocument> Documents { get; set; } = new List<PolicyRequestDocument>();
    }
}
