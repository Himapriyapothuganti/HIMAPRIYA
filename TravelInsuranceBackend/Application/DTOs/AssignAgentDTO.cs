using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class AssignAgentDTO
    {
        [Required]
        public int PolicyId { get; set; }

        [Required]
        public string AgentId { get; set; } = string.Empty;
    }
}
