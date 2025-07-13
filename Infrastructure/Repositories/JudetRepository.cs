using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Shared.DTOs;

public class JudetRepository : IJudetRepository
{
    private readonly IDbConnection _db;

    public JudetRepository(IDbConnection db)
    {
        _db = db;
    }

    public async Task<IEnumerable<JudetDto>> GetAllAsync()
    {
        return await _db.QueryAsync<JudetDto>(
            "dbo.GetAllJudete",
            commandType: CommandType.StoredProcedure
        );
    }
}