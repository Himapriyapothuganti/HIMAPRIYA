using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CreatePolicyDTO
    {
        [Required]
        public int PolicyProductId { get; set; }

        [Required]
        public string Destination { get; set; } = string.Empty;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public string TravellerName { get; set; } = string.Empty;

        [Required]
        public int TravellerAge { get; set; }

        [Required]
        public string PassportNumber { get; set; } = string.Empty;


        // ── KYC ──────────────────────────────────────────
        [Required]
        public string KycType { get; set; } = string.Empty;  // PAN / Aadhaar / CKYC

        [Required]
        public string KycNumber { get; set; } = string.Empty;
    }
}
