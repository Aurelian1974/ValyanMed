using Shared.DTOs;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Repositories
{
    public class PersoanaRepository : IPersoanaRepository
    {
        private readonly string _connectionString;

        public PersoanaRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<PersoanaDTO>> GetAllAsync()
        {
            using IDbConnection connection = new SqlConnection(_connectionString);
            var persoane = await connection.QueryAsync<PersoanaDTO>("sp_GetAllPersonal", commandType: CommandType.StoredProcedure);
            foreach (var persoana in persoane)
            {
                persoana.Specialitate ??= persoana.PozitieOrganizatie ?? "Nedefinit";
                persoana.Departament ??= "Medicin? Intern?";
                persoana.DataAngajarii = persoana.DataCreare?.Date ?? System.DateTime.Now.AddYears(-1);
                persoana.Status ??= "Activ";
            }
            return persoane;
        }

        public async Task<PersoanaDTO> GetByIdAsync(int id)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            var persoana = await connection.QueryFirstOrDefaultAsync<PersoanaDTO>("sp_GetPersoanaById", parameters, commandType: CommandType.StoredProcedure);
            if (persoana != null)
            {
                persoana.Specialitate ??= persoana.PozitieOrganizatie ?? "Nedefinit";
                persoana.Departament ??= "Medicin? Intern?";
                persoana.DataAngajarii = persoana.DataCreare?.Date ?? System.DateTime.Now.AddYears(-1);
                persoana.Status ??= "Activ";
            }
            return persoana;
        }

        public async Task<int> CreateAsync(CreatePersoanaDTO persoana)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            var guid = System.Guid.NewGuid();
            parameters.Add("@Guid", guid);
            parameters.Add("@Nume", persoana.Nume);
            parameters.Add("@Prenume", persoana.Prenume);
            parameters.Add("@Judet", persoana.Judet);
            parameters.Add("@Localitate", persoana.Localitate);
            parameters.Add("@Strada", persoana.Strada);
            parameters.Add("@NumarStrada", persoana.NumarStrada);
            parameters.Add("@CodPostal", persoana.CodPostal);
            parameters.Add("@PozitieOrganizatie", persoana.PozitieOrganizatie ?? persoana.Specialitate);
            parameters.Add("@DataNasterii", persoana.DataNasterii);
            parameters.Add("@CNP", persoana.CNP);
            parameters.Add("@TipActIdentitate", persoana.TipActIdentitate);
            parameters.Add("@SerieActIdentitate", persoana.SerieActIdentitate);
            parameters.Add("@NumarActIdentitate", persoana.NumarActIdentitate);
            parameters.Add("@StareCivila", persoana.StareCivila);
            parameters.Add("@Gen", persoana.Gen);
            return await connection.ExecuteScalarAsync<int>("sp_CreatePersoana", parameters, commandType: CommandType.StoredProcedure);
        }

        public async Task<bool> UpdateAsync(UpdatePersoanaDTO persoana)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Id", persoana.Id);
            parameters.Add("@Nume", persoana.Nume);
            parameters.Add("@Prenume", persoana.Prenume);
            parameters.Add("@Judet", persoana.Judet);
            parameters.Add("@Localitate", persoana.Localitate);
            parameters.Add("@Strada", persoana.Strada);
            parameters.Add("@NumarStrada", persoana.NumarStrada);
            parameters.Add("@CodPostal", persoana.CodPostal);
            parameters.Add("@PozitieOrganizatie", persoana.PozitieOrganizatie ?? persoana.Specialitate);
            parameters.Add("@DataNasterii", persoana.DataNasterii);
            parameters.Add("@CNP", persoana.CNP);
            parameters.Add("@TipActIdentitate", persoana.TipActIdentitate);
            parameters.Add("@SerieActIdentitate", persoana.SerieActIdentitate);
            parameters.Add("@NumarActIdentitate", persoana.NumarActIdentitate);
            parameters.Add("@StareCivila", persoana.StareCivila);
            parameters.Add("@Gen", persoana.Gen);
            var result = await connection.ExecuteAsync("sp_UpdatePersoana", parameters, commandType: CommandType.StoredProcedure);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using IDbConnection connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            parameters.Add("@Id", id);
            var result = await connection.ExecuteAsync("sp_DeletePersoana", parameters, commandType: CommandType.StoredProcedure);
            return result > 0;
        }
    }
}
