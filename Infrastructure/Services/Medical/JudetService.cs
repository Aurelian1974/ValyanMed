using Application.Services.Medical;
using Shared.DTOs.Medical;
using System.Data;
using Dapper;

namespace Infrastructure.Services.Medical;

public class JudetService : IJudetService
{
    private readonly IDbConnection _connection;
    
    public JudetService(IDbConnection connection)
    {
        _connection = connection;
    }

    public async Task<List<JudetDto>> GetAllAsync()
    {
        var result = await _connection.QueryAsync<JudetDto>("GetAllJudete", commandType: CommandType.StoredProcedure);
        return result.ToList();
    }
}
