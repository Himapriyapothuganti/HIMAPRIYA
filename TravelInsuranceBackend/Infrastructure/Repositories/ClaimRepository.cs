using Application.Interfaces.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ClaimRepository :IClaimRepository
    {
        // REPOSITORY (Data Access Layer)
        // This class talks directly to the database using EF Core
        // It has NO business logic — only database operations
        private readonly AppDbContext _context;

        public ClaimRepository(AppDbContext context)
        {
            // AppDbContext is injected — it is the EF Core gateway to SQL Server
            _context = context;
        }

        public async Task<Domain.Entities.Claim> GetByIdAsync(int id)
        {
            return await _context.Claims.FindAsync(id);
        }

        public async Task<List<Domain.Entities.Claim>> GetAllAsync()
        {
            return await _context.Claims.ToListAsync();
        }

        public async Task<List<Domain.Entities.Claim>> GetByPolicyIdAsync(int policyId)
        {
            return await _context.Claims
                .Where(c => c.PolicyId == policyId)
                .ToListAsync();
        }

        public async Task<List<Domain.Entities.Claim>> GetByCustomerIdAsync(string customerId)
        {
            return await _context.Claims
                .Where(c => c.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<List<Domain.Entities.Claim>> GetByOfficerIdAsync(string officerId)
        {
            return await _context.Claims
                .Where(c => c.ClaimsOfficerId == officerId)
                .ToListAsync();
        }

        public async Task<Domain.Entities.Claim> CreateAsync(Domain.Entities.Claim claim)
        {
            // Stage the new claim record in EF Core's change tracker
            _context.Claims.Add(claim);

            // THIS fires the actual SQL INSERT into the Claims table in SQL Server
            await _context.SaveChangesAsync();

            return claim;
        }

        public async Task<Domain.Entities.Claim> UpdateAsync(Domain.Entities.Claim claim)
        {
            _context.Claims.Update(claim);
            await _context.SaveChangesAsync();
            return claim;
        }

        public async Task<int> GetActiveClaimCountByOfficerAsync(string officerId)
        {
            return await _context.Claims
                .CountAsync(c => c.ClaimsOfficerId == officerId
                    && c.Status != Domain.Enums.ClaimStatus.Closed
                    && c.Status != Domain.Enums.ClaimStatus.Rejected);
        }
    }
}
    

