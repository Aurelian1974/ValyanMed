using Shared.DTOs.Medical;

namespace Application.Services.Medical;

public interface IJudetService
{
    Task<List<JudetDto>> GetAllAsync();
}
