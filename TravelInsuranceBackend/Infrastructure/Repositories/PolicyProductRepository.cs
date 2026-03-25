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
    public class PolicyProductRepository :IPolicyProductRepository
    {
        private readonly AppDbContext _context;

        public PolicyProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PolicyProduct> GetByIdAsync(int id)
        {
            return await _context.PolicyProducts.FindAsync(id);
        }

        public async Task<List<PolicyProduct>> GetAllAsync()
        {
            return await _context.PolicyProducts.ToListAsync();
        }

        public async Task<PolicyProduct> CreateAsync(PolicyProduct product)
        {
            _context.PolicyProducts.Add(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<PolicyProduct> UpdateAsync(PolicyProduct product)
        {
            _context.PolicyProducts.Update(product);
            await _context.SaveChangesAsync();
            return product;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = await _context.PolicyProducts.FindAsync(id);
            if (product == null) return false;

            _context.PolicyProducts.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsInPoliciesAsync(int id)
        {
            return await _context.Policies.AnyAsync(p => p.PolicyProductId == id);
        }
    }

}
