using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IPolicyRequestRepository
    {
        Task<PolicyRequest> GetByIdAsync(int id);
        Task<IEnumerable<PolicyRequest>> GetAllAsync();
        Task<IEnumerable<PolicyRequest>> GetByCustomerIdAsync(string customerId);
        Task<IEnumerable<PolicyRequest>> GetByAgentIdAsync(string agentId);
        Task<PolicyRequest> CreateAsync(PolicyRequest policyRequest);
        Task<PolicyRequest> UpdateAsync(PolicyRequest policyRequest);
        Task DeleteAsync(int id);
        Task<int> GetActiveRequestCountByAgentAsync(string agentId);
    }
}
