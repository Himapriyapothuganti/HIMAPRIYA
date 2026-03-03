using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class OfficerDashboardDTO
    {
        public string OfficerId { get; set; } = string.Empty;
        public string OfficerName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Claims Summary
        public int TotalAssignedClaims { get; set; }
        public int UnderReviewClaims { get; set; }
        public int PendingDocumentsClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public int ClosedClaims { get; set; }
        public decimal TotalApprovedAmount { get; set; }

        // Claims List
        public List<ClaimResponseDTO> AssignedClaims { get; set; } = new();
    }
}
