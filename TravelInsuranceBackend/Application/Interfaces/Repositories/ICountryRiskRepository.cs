using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface ICountryRiskRepository
    {
        Task<IEnumerable<CountryRisk>> GetAllAsync();
        Task<IEnumerable<CountryRisk>> GetActiveAsync();
        Task<CountryRisk?> GetByIdAsync(int id);
        Task<CountryRisk?> GetByNameAsync(string name);
        Task<CountryRisk> AddAsync(CountryRisk countryRisk);
        Task UpdateAsync(CountryRisk countryRisk);
        Task DeleteAsync(int id);
        Task<bool> IsCountryUsedInPoliciesAsync(string countryName);
    }
}
