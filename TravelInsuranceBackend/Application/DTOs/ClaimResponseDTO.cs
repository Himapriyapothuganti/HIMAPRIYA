using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ClaimResponseDTO
    {
        public int ClaimId { get; set; }
        public int PolicyId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? ClaimsOfficerId { get; set; }
        public string? ClaimsOfficerName { get; set; }
        public string ClaimType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal ClaimedAmount { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? RejectionReason { get; set; }

        public decimal SuggestedPayout { get; set; }
        public decimal DeductibleApplied { get; set; }
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public DateTime IncidentDate { get; set; }
        // ── Documents ─────────────────────────────────────
        public List<ClaimDocumentDTO> Documents { get; set; } = new();
    }
    public class ClaimDocumentDTO
    {
        public int ClaimDocumentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
        public string FileUrl { get; set; } = string.Empty;
    }
}
