using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Services
{
    public interface IVertexAiService
    {
        Task<string> AnalyzeClaimAsync(string jsonClaimData, List<string> filePaths);
        Task<string> AnalyzePolicyRequestAsync(string jsonRequestData, List<string> filePaths);
    }
}
