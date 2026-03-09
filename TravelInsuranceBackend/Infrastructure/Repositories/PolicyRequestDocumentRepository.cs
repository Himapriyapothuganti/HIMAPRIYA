using Application.Interfaces.Repositories;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class PolicyRequestDocumentRepository : IPolicyRequestDocumentRepository
    {
        private readonly AppDbContext _context;

        public PolicyRequestDocumentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PolicyRequestDocument> GetByIdAsync(int id)
        {
            return await _context.PolicyRequestDocuments.FindAsync(id);
        }

        public async Task<IEnumerable<PolicyRequestDocument>> GetByRequestIdAsync(int requestId)
        {
            return await _context.PolicyRequestDocuments
                .Where(d => d.PolicyRequestId == requestId)
                .ToListAsync();
        }

        public async Task<PolicyRequestDocument> AddAsync(PolicyRequestDocument document)
        {
            await _context.PolicyRequestDocuments.AddAsync(document);
            return document;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
