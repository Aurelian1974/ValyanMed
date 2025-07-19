using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Dapper;
using Shared.DTOs;

namespace Infrastructure.Repositories
{
    public class LocalitateRepository : ILocalitateRepository
    {
        private readonly IDbConnection _db;

        public LocalitateRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<LocalitateDto>> GetByJudetAsync(int idJudet)
        {
            return await _db.QueryAsync<LocalitateDto>(
                "dbo.Localitate_GetByJudet",
                new { IdJudet = idJudet },
                commandType: CommandType.StoredProcedure
            );
        }
    }
}
