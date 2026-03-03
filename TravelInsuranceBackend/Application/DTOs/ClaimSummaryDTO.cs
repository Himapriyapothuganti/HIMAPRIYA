using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ClaimSummaryDTO
    {
        public int ClaimId { get; set; }
        public string ClaimType { get; set; } = string.Empty;
        public decimal ClaimedAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
    }
}
