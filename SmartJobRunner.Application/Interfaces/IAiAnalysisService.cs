using System.Threading.Tasks;

namespace SmartJobRunner.Application.Interfaces;

public interface IAiAnalysisService
{
    Task<string> AnalyzeFailureAsync(string jobName, string? errorMessage);
}
