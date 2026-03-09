using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class AdminDashboardDTO
    {
        // Users Summary
        public int TotalUsers { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalAgents { get; set; }
        public int TotalClaimsOfficers { get; set; }

        // Policy Summary
        public int TotalPolicies { get; set; }
        public int ActivePolicies { get; set; }
        public int PendingPaymentPolicies { get; set; }
        public int ExpiredPolicies { get; set; }
        public decimal TotalPremiumRevenue { get; set; }

        // Claims Summary
        public int TotalClaims { get; set; }
        public int SubmittedClaims { get; set; }
        public int UnderReviewClaims { get; set; }
        public int PendingDocumentsClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public int PaymentProcessedClaims { get; set; }
        public int ClosedClaims { get; set; }
        public decimal TotalClaimedAmount { get; set; }
        public decimal TotalApprovedAmount { get; set; }

        // Agent Performance
        public List<AgentPerformanceDTO> AgentPerformance { get; set; } = new();
    }

    public class AgentPerformanceDTO
    {
        public string AgentId { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalPolicies { get; set; }
        public int ActivePolicies { get; set; }
        public decimal TotalPremiumCollected { get; set; }
        public decimal CommissionEarned { get; set; }
    }
}

