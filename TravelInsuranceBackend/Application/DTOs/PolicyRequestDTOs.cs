using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    // The form data DTO, documents handled separately via [FromForm] IFormFile
    public class CreatePolicyRequestDTO
    {
        public int PolicyProductId { get; set; }
        public string Destination { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string TravellerName { get; set; } = string.Empty;
        [Range(1, 120, ErrorMessage = "Traveller age must be between 1 and 120.")]
        public int TravellerAge { get; set; }
        public string PassportNumber { get; set; } = string.Empty;
        public string KycType { get; set; } = string.Empty;
        public string KycNumber { get; set; } = string.Empty;
        
        public string? Dependents { get; set; }
        public string? UniversityName { get; set; }
        public string? StudentId { get; set; }
        public string? TripFrequency { get; set; }
    }

    public class PolicyRequestDocumentDTO
    {
        public int PolicyRequestDocumentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public string DocumentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public string FileUrl { get; set; } = string.Empty;
    }

    public class PolicyRequestResponseDTO
    {
        // NO Risk scores here ever
        public int PolicyRequestId { get; set; }
        public int PolicyProductId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public string PolicyType { get; set; } = string.Empty;
        public string PlanTier { get; set; } = string.Empty;
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
        
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }
        
        public decimal CalculatedPremium { get; set; }
        
        // Resubmission Info
        public int ResubmissionCount { get; set; }
        public int MaxResubmissions { get; set; }
        public string? RequestedDocTypes { get; set; } 

        public DateTime RequestedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }

    public class AgentPolicyRequestResponseDTO : PolicyRequestResponseDTO
    {
        public string CustomerName { get; set; } = string.Empty;
        
        // AI Risk Insight
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = string.Empty; // Low/Medium/High
        public string? RiskReasoning { get; set; }
        public decimal CountryRiskMultiplier { get; set; }
        public string CountryRiskLevel { get; set; } = string.Empty;
        public string? AiAnalysisJson { get; set; } // Rich JSON from Vertex AI
        
        public string? AgentNotes { get; set; }
        public List<PolicyRequestDocumentDTO> Documents { get; set; } = new();
    }

    public class ReviewPolicyRequestDTO
    {
        public string Status { get; set; } = string.Empty; // Approved / Rejected / Needs Revision
        public string? RejectionReason { get; set; }
        public string? AgentNotes { get; set; }
        public string? RequestedDocTypes { get; set; } // e.g. "KYC,Passport"
    }

    public class PayPolicyRequestDTO
    {
        public int PolicyRequestId { get; set; }
        public string PaymentMethod { get; set; } = "Credit Card";
    }

    public class DependentDTO
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string Relationship { get; set; } = string.Empty;
        public string? OtherRelationship { get; set; }
    }
}
