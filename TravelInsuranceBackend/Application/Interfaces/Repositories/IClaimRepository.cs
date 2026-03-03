using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IClaimRepository
    {
        Task<Domain.Entities.Claim> GetByIdAsync(int id);
        Task<List<Domain.Entities.Claim>> GetAllAsync();
        Task<List<Domain.Entities.Claim>> GetByPolicyIdAsync(int policyId);
        Task<List<Domain.Entities.Claim>> GetByCustomerIdAsync(string customerId);
        Task<List<Domain.Entities.Claim>> GetByOfficerIdAsync(string officerId);
        Task<Domain.Entities.Claim> CreateAsync(Domain.Entities.Claim claim);
        Task<Domain.Entities.Claim> UpdateAsync(Domain.Entities.Claim claim);
        Task<int> GetActiveClaimCountByOfficerAsync(string officerId);
    }
}
