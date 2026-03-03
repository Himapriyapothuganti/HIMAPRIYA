using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class PolicyPaymentDTO
    {

        [Required]
        public int PolicyId { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = string.Empty; // Card / UPI / NetBanking
    }
}
