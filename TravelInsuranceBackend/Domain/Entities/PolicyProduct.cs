using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Enums;

namespace Domain.Entities
{
    public class PolicyProduct
    {
        public int PolicyProductId { get; set; }
        public string PolicyName { get; set; } = string.Empty;
        public string PolicyType { get; set; } = string.Empty;  // Single Trip / Multi-Trip / Family / Student
        public string PlanTier { get; set; } = string.Empty;    // Silver / Gold / Premium
        public string CoverageDetails { get; set; } = string.Empty;
        public string ExclusionDetails { get; set; } = string.Empty;
        public decimal CoverageLimit { get; set; }
        public decimal BasePremium { get; set; }
        public int Tenure { get; set; }                         // in days
        public decimal ClaimLimit { get; set; }
        public string DestinationZone { get; set; } = string.Empty;
        public PolicyProductStatus Status { get; set; } = PolicyProductStatus.Available;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
