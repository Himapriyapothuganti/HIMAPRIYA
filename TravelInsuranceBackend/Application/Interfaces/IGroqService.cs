using Application.DTOs;

namespace Application.Interfaces
{
    public interface IGroqService
    {
        Task<RecommendationResponseDTO> GetRecommendationAsync(RecommendationRequestDTO request, List<PolicyProductResponseDTO> availablePlans);
    }
}
