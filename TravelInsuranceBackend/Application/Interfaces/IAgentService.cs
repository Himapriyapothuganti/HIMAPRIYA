using Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IAgentService
    {
        Task<AgentDashboardDTO> GetDashboardAsync(string agentId);
        Task<List<PolicyResponseDTO>> GetAssignedPoliciesAsync(string agentId);
        Task<PolicyResponseDTO> GetPolicyDetailAsync(string agentId, int policyId);
    }
}
