using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class AgentDashboardDTO
    {
        public string AgentId { get; set; } = string.Empty;
        public string AgentName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Summary
        public int TotalPoliciesAssigned { get; set; }
        public int ActivePolicies { get; set; }
        public int PendingPaymentPolicies { get; set; }
        public int ExpiredPolicies { get; set; }
        public decimal TotalPremiumCollected { get; set; }
        public decimal TotalCommissionEarned { get; set; }

        // Policy list
        public List<PolicyResponseDTO> AssignedPolicies { get; set; } = new();
    }
}
