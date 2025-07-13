using System.Collections.Generic;
using System.Threading.Tasks;
using Shared.DTOs;

public interface IJudetService
{
    Task<IEnumerable<JudetDto>> GetAllAsync();
}