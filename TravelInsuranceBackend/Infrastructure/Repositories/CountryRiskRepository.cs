using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class CountryRiskRepository : ICountryRiskRepository
    {
        private readonly AppDbContext _context;

        public CountryRiskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<CountryRisk>> GetAllAsync()
        {
            return await _context.CountryRisks.ToListAsync();
        }

        public async Task<IEnumerable<CountryRisk>> GetActiveAsync()
        {
            return await _context.CountryRisks
                .Where(c => c.IsActive)
                .ToListAsync();
        }

        public async Task<CountryRisk?> GetByIdAsync(int id)
        {
            return await _context.CountryRisks.FindAsync(id);
        }

        public async Task<CountryRisk?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return null;
            var normalized = name.Trim();
            return await _context.CountryRisks
                .FirstOrDefaultAsync(c => c.Name.ToLower() == normalized.ToLower() || c.Name == normalized);
        }

        public async Task<CountryRisk> AddAsync(CountryRisk countryRisk)
        {
            _context.CountryRisks.Add(countryRisk);
            await _context.SaveChangesAsync();
            return countryRisk;
        }

        public async Task UpdateAsync(CountryRisk countryRisk)
        {
            _context.CountryRisks.Update(countryRisk);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var country = await _context.CountryRisks.FindAsync(id);
            if (country != null)
            {
                _context.CountryRisks.Remove(country);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> IsCountryUsedInPoliciesAsync(string countryName)
        {
            // Check if country is used in Policies or PolicyRequests
            bool usedInPolicies = await _context.Policies.AnyAsync(p => p.Destination.ToLower() == countryName.ToLower());
            if (usedInPolicies) return true;

            bool usedInRequests = await _context.PolicyRequests.AnyAsync(r => r.Destination.ToLower() == countryName.ToLower());
            return usedInRequests;
        }
    }
}
