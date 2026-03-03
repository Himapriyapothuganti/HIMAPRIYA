using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class PolicyRepository :IPolicyRepository
    {
        private readonly AppDbContext _context;

        public PolicyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Policy> GetByIdAsync(int id)
        {
            return await _context.Policies.FindAsync(id)
                   ?? throw new Exception("Policy not found.");
        }

        public async Task<List<Policy>> GetAllAsync()
        {
            return await _context.Policies.ToListAsync();
        }

        public async Task<List<Policy>> GetByCustomerIdAsync(string customerId)
        {
            return await _context.Policies
                .Where(p => p.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<List<Policy>> GetByAgentIdAsync(string agentId)
        {
            return await _context.Policies
                .Where(p => p.AgentId == agentId)
                .ToListAsync();
        }

        public async Task<Policy> CreateAsync(Policy policy)
        {
            _context.Policies.Add(policy);
            await _context.SaveChangesAsync();
            return policy;
        }

        public async Task<Policy> UpdateAsync(Policy policy)
        {
            _context.Policies.Update(policy);
            await _context.SaveChangesAsync();
            return policy;
        }
    }

}

