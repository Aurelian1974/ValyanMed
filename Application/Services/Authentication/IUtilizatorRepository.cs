using Shared.Common;
using Shared.Models.Authentication;

namespace Application.Services.Authentication;

public interface IUtilizatorRepository
{
    Task<Result<int>> CreateAsync(Utilizator utilizator);
    Task<Result<Utilizator?>> GetByIdAsync(int id);
    Task<Result<Utilizator?>> GetByUsernameOrEmailAsync(string usernameOrEmail);
    Task<Result<IEnumerable<Utilizator>>> GetAllAsync();
    Task<Result> UpdateAsync(Utilizator utilizator);
    Task<Result> DeleteAsync(int id);
    Task<Result<bool>> ExistsByUsernameAsync(string username, int? excludeId = null);
    Task<Result<bool>> ExistsByEmailAsync(string email, int? excludeId = null);
}