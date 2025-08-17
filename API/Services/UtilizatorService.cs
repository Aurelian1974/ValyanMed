using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using API.Models;

namespace API.Services
{
    public class UtilizatorService : IUtilizatorService
    {
        private readonly IConfiguration _config;
        private readonly string _connectionString;

        public UtilizatorService(IConfiguration config)
        {
            _config = config;
            _connectionString = _config.GetConnectionString("DefaultConnection");
        }

        public async Task<IEnumerable<UtilizatorDTO>> GetAllUtilizatoriAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            var utilizatori = await connection.QueryAsync<UtilizatorDTO>(
                "usp_Utilizator_GetAll",
                commandType: CommandType.StoredProcedure);
                
            return utilizatori;
        }

        public async Task<UtilizatorDTO> GetUtilizatorByIdAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var utilizator = await connection.QueryFirstOrDefaultAsync<UtilizatorDTO>(
                "usp_Utilizator_GetById",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
                
            return utilizator;
        }

        public async Task<int> CreateUtilizatorAsync(CreateUtilizatorDTO utilizatorDto)
        {
            // Hash password
            var parolaHash = BCrypt.Net.BCrypt.HashPassword(utilizatorDto.Parola);
            
            using var connection = new SqlConnection(_connectionString);
            var parameters = new
            {
                NumeUtilizator = utilizatorDto.NumeUtilizator,
                ParolaHash = parolaHash,
                Email = utilizatorDto.Email,
                Telefon = utilizatorDto.Telefon,
                PersoanaId = utilizatorDto.PersoanaId
            };

            // Execute stored procedure and get the identity value (assumes SP returns SCOPE_IDENTITY())
            var id = await connection.ExecuteScalarAsync<int>(
                "usp_Utilizator_Insert",
                parameters,
                commandType: CommandType.StoredProcedure);
                
            return id;
        }

        public async Task<bool> UpdateUtilizatorAsync(UpdateUtilizatorDTO utilizatorDto)
        {
            using var connection = new SqlConnection(_connectionString);
            var parameters = new DynamicParameters();
            
            parameters.Add("@Id", utilizatorDto.Id);
            parameters.Add("@NumeUtilizator", utilizatorDto.NumeUtilizator);
            parameters.Add("@Email", utilizatorDto.Email);
            parameters.Add("@Telefon", utilizatorDto.Telefon);
            parameters.Add("@PersoanaId", utilizatorDto.PersoanaId);
            
            // Only update password if provided
            if (!string.IsNullOrWhiteSpace(utilizatorDto.Parola))
            {
                parameters.Add("@ParolaHash", BCrypt.Net.BCrypt.HashPassword(utilizatorDto.Parola));
            }
            else
            {
                parameters.Add("@ParolaHash", null);
            }

            // Add return parameter to capture stored procedure return value
            parameters.Add("@ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            try
            {
                Console.WriteLine($"=== UPDATE USER DEBUG ===");
                Console.WriteLine($"ID: {utilizatorDto.Id}");
                Console.WriteLine($"NumeUtilizator: {utilizatorDto.NumeUtilizator}");
                Console.WriteLine($"Email: {utilizatorDto.Email}");
                Console.WriteLine($"Telefon: {utilizatorDto.Telefon}");
                Console.WriteLine($"PersoanaId: {utilizatorDto.PersoanaId}");
                Console.WriteLine($"HasPassword: {!string.IsNullOrWhiteSpace(utilizatorDto.Parola)}");
                
                await connection.ExecuteAsync(
                    "usp_Utilizator_Update",
                    parameters,
                    commandType: CommandType.StoredProcedure);
                    
                // Get the return value from stored procedure
                var returnValue = parameters.Get<int>("@ReturnValue");
                
                Console.WriteLine($"Stored procedure return value: {returnValue}");
                Console.WriteLine($"=== END DEBUG ===");
                
                // Handle specific return values from stored procedure
                switch (returnValue)
                {
                    case -1:
                        throw new Exception("Numele de utilizator exista deja");
                    case -2:
                        throw new Exception("Adresa de email exista deja");
                    case -3:
                        throw new Exception("Persoana asociata nu exista");
                    case -4:
                        throw new Exception("Utilizatorul nu a fost gasit sau nu s-au facut modificari");
                    case var value when value > 0:
                        return true; // Success - return value is number of affected rows
                    default:
                        return false;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"SQL Exception: {ex.Message}");
                Console.WriteLine($"SQL Number: {ex.Number}");
                Console.WriteLine($"SQL State: {ex.State}");
                Console.WriteLine($"SQL Severity: {ex.Class}");
                throw new Exception(ex.Message, ex);
            }
            catch (Exception ex) when (ex.Message.Contains("exista deja") || ex.Message.Contains("nu exista") || ex.Message.Contains("nu a fost gasit"))
            {
                // Re-throw our custom exceptions
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"General Exception: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> DeleteUtilizatorAsync(int id)
        {
            using var connection = new SqlConnection(_connectionString);
            var affectedRows = await connection.ExecuteAsync(
                "usp_Utilizator_Delete",
                new { Id = id },
                commandType: CommandType.StoredProcedure);
                
            return affectedRows > 0;
        }

        public async Task<AuthResult> AuthenticateAsync(string usernameOrEmail, string password)
        {
            using var connection = new SqlConnection(_connectionString);
            
            // Get user from database
            var user = await connection.QueryFirstOrDefaultAsync<UtilizatorDb>(
                "usp_Utilizator_Authenticate",
                new { NumeUtilizatorSauEmail = usernameOrEmail },
                commandType: CommandType.StoredProcedure);

            if (user == null)
            {
                return new AuthResult { Success = false, Message = "Utilizator negasit" };
            }

            // Verify password
            bool isValidPassword = BCrypt.Net.BCrypt.Verify(password, user.ParolaHash);
            if (!isValidPassword)
            {
                return new AuthResult { Success = false, Message = "Parola incorecta" };
            }

            // Get the full name from person table
            var person = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT Nume, Prenume FROM Persoana WHERE Id = @PersoanaId",
                new { PersoanaId = user.PersoanaId });

            string fullName = person != null ? $"{person.Nume} {person.Prenume}" : user.NumeUtilizator;

            return new AuthResult 
            { 
                User = user, 
                NumeComplet = fullName, 
                Success = true,
                Message = "Autentificare reușita"
            };
        }
    }
}
