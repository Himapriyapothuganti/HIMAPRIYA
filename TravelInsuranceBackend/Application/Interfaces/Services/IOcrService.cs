using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Application.DTOs;

namespace Application.Interfaces.Services
{
    public interface IOcrService
    {
        /// <summary>
        /// Processes a document (image/PDF) to extract identity data.
        /// </summary>
        Task<ExtractedDataDTO> ProcessDocumentAsync(IFormFile file);
    }
}
