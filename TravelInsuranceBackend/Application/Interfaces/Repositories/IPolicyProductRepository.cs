using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IPolicyProductRepository
    {
        Task<PolicyProduct> GetByIdAsync(int id);
        Task<List<PolicyProduct>> GetAllAsync();
        Task<PolicyProduct> CreateAsync(PolicyProduct product);
        Task<PolicyProduct> UpdateAsync(PolicyProduct product);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsInPoliciesAsync(int id);
    }
}
