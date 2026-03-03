using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
     public interface IPolicyRepository
    {
        Task<Policy> GetByIdAsync(int id);
        Task<List<Policy>> GetAllAsync();
        Task<List<Policy>> GetByCustomerIdAsync(string customerId);
        Task<List<Policy>> GetByAgentIdAsync(string agentId);
        Task<Policy> CreateAsync(Policy policy);
        Task<Policy> UpdateAsync(Policy policy);
    }
}
