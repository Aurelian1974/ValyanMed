using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;

public class PersoanaService : IPersoanaService
{
    private readonly IDbConnection _connection;

    public PersoanaService(IConfiguration configuration)
    {
        _connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
    }

    public async Task<List<PersoanaModel>> GetAllPersonalAsync()
    {
        var persoane = await _connection.QueryAsync<PersoanaModel>("sp_GetAllPersonal", commandType: CommandType.StoredProcedure);
        
        // For demo purposes, we'll add the missing properties
        // In a real app, you would join with other tables or have additional stored procedures
        foreach (var persoana in persoane)
        {
            // Set default values for UI display
            persoana.Specialitate = persoana.PozitieOrganizatie ?? "Nedefinit";
            persoana.Departament = "Medicină Internă"; // Default value
            persoana.DataAngajarii = persoana.DataCreare?.Date ?? DateTime.Now.AddYears(-1);
            persoana.Status = "Activ";
        }
        
        return persoane.AsList();
    }

    public async Task<PersoanaModel> GetPersoanaByIdAsync(int id)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        
        var persoana = await _connection.QueryFirstOrDefaultAsync<PersoanaModel>(
            "sp_GetPersoanaById", 
            parameters, 
            commandType: CommandType.StoredProcedure);
        
        if (persoana != null)
        {
            // Set default values for UI display
            persoana.Specialitate = persoana.PozitieOrganizatie ?? "Nedefinit";
            persoana.Departament = "Medicină Internă"; // Default value
            persoana.DataAngajarii = persoana.DataCreare?.Date ?? DateTime.Now.AddYears(-1);
            persoana.Status = "Activ";
        }
        
        return persoana;
    }

    public async Task<int> CreatePersoanaAsync(PersoanaModel persoana)
    {
        var parameters = new DynamicParameters();
        
        parameters.Add("@Guid", persoana.Guid);
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
        
        return await _connection.ExecuteScalarAsync<int>(
            "sp_CreatePersoana", 
            parameters, 
            commandType: CommandType.StoredProcedure);
    }

    public async Task<bool> UpdatePersoanaAsync(PersoanaModel persoana)
    {
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
        
        return await _connection.ExecuteAsync(
            "sp_UpdatePersoana", 
            parameters, 
            commandType: CommandType.StoredProcedure) > 0;
    }

    public async Task<bool> DeletePersoanaAsync(int id)
    {
        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);
        
        return await _connection.ExecuteAsync(
            "sp_DeletePersoana", 
            parameters, 
            commandType: CommandType.StoredProcedure) > 0;
    }
}