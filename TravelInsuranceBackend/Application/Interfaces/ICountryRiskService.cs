using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ICountryRiskService
    {
        Task<IEnumerable<CountryRiskDTO>> GetAllAsync();
        Task<IEnumerable<CountryRiskDTO>> GetActiveAsync();
        Task<CountryRiskDTO> GetByIdAsync(int id);
        Task<CountryRiskDTO> CreateAsync(CreateCountryRiskDTO dto);
        Task UpdateAsync(int id, UpdateCountryRiskDTO dto);
        Task DeleteAsync(int id);
    }
}
