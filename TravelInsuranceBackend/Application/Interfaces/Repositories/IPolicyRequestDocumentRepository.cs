using Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IPolicyRequestDocumentRepository
    {
        Task<PolicyRequestDocument> GetByIdAsync(int id);
        Task<IEnumerable<PolicyRequestDocument>> GetByRequestIdAsync(int requestId);
        Task<PolicyRequestDocument> AddAsync(PolicyRequestDocument document);
        Task SaveChangesAsync();
    }
}
