using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class PolicyRequestRepository : IPolicyRequestRepository
    {
        private readonly AppDbContext _context;

        public PolicyRequestRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PolicyRequest> GetByIdAsync(int id)
        {
            var request = await _context.PolicyRequests
                .Include(r => r.PolicyProduct)
                .Include(r => r.Customer)
                .Include(r => r.Agent)
                .FirstOrDefaultAsync(r => r.PolicyRequestId == id);

            if (request == null)
            {
                throw new Exception($"PolicyRequest with ID {id} not found.");
            }

            return request;
        }

        public async Task<IEnumerable<PolicyRequest>> GetAllAsync()
        {
            return await _context.PolicyRequests
                .Include(r => r.PolicyProduct)
                .Include(r => r.Customer)
                .Include(r => r.Agent)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PolicyRequest>> GetByCustomerIdAsync(string customerId)
        {
            return await _context.PolicyRequests
                .Include(r => r.PolicyProduct)
                .Include(r => r.Customer)
                .Include(r => r.Agent)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<PolicyRequest>> GetByAgentIdAsync(string agentId)
        {
            return await _context.PolicyRequests
                .Include(r => r.PolicyProduct)
                .Include(r => r.Customer)
                .Include(r => r.Agent)
                .Where(r => r.AgentId == agentId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();
        }

        public async Task<PolicyRequest> CreateAsync(PolicyRequest policyRequest)
        {
            await _context.PolicyRequests.AddAsync(policyRequest);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(policyRequest.PolicyRequestId);
        }

        public async Task<PolicyRequest> UpdateAsync(PolicyRequest policyRequest)
        {
            _context.PolicyRequests.Update(policyRequest);
            await _context.SaveChangesAsync();
            return policyRequest;
        }

        public async Task DeleteAsync(int id)
        {
            var request = await _context.PolicyRequests.FindAsync(id);
            if (request != null)
            {
                _context.PolicyRequests.Remove(request);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetActiveRequestCountByAgentAsync(string agentId)
        {
            return await _context.PolicyRequests
                .CountAsync(r => r.AgentId == agentId && r.Status == "Pending");
        }
    }
}
