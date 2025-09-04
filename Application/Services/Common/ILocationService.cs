using Shared.DTOs.Common;

namespace Application.Services.Common;

public interface ILocationService
{
    Task<List<JudetDto>> GetJudeteAsync();
    Task<List<LocalitateDto>> GetLocalitatiAsync();
    Task<List<LocalitateDto>> GetLocalitatiByJudetAsync(string judetNume);
    Task<List<LocalitateDto>> GetLocalitatiByJudetIdAsync(int judetId);
    Task<List<JudetWithLocalitatiDto>> GetJudeteWithLocalitatiAsync();
}