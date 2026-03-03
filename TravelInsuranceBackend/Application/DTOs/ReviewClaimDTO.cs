using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class ReviewClaimDTO
    {
        [Required]
        public bool IsApproved { get; set; }

        // Required if approved
        public decimal? ApprovedAmount { get; set; }

        // Required if rejected
        public string? RejectionReason { get; set; }
    }
}
