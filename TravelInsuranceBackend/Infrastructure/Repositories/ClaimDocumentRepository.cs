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
    public class ClaimDocumentRepository : IClaimDocumentRepository
    {
        private readonly AppDbContext _context;

        public ClaimDocumentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ClaimDocument document)
        {
            await _context.ClaimDocuments.AddAsync(document);
        }

        public async Task<List<ClaimDocument>> GetByClaimIdAsync(int claimId)
        {
            return await _context.ClaimDocuments
                .Where(d => d.ClaimId == claimId)
                .ToListAsync();
        }

        public async Task<ClaimDocument?> GetByIdAsync(int documentId)
        {
            return await _context.ClaimDocuments
                .FirstOrDefaultAsync(d => d.ClaimDocumentId == documentId);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}

