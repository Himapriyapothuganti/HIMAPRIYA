using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class RequestDocumentDTO
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }
}
