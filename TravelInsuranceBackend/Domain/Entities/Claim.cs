using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Claim
    {
        public int ClaimId { get; set; }
        public int PolicyId { get; set; }
        public string CustomerId { get; set; } = string.Empty;
        public string ClaimType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal ClaimedAmount { get; set; }
        public decimal? ApprovedAmount { get; set; }
        public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;
        public string? ClaimsOfficerId { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }

        // Navigation properties
        public Policy Policy { get; set; } = null!;
    }
}
