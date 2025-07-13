using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.DTOs;

public interface IJudetRepository
{
    Task<IEnumerable<JudetDto>> GetAllAsync();
}