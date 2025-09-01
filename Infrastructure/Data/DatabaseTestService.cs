using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public interface IDatabaseTestService
{
    Task<bool> TestConnectionAsync();
    Task<bool> TableExistsAsync(string tableName);
    Task<string> CreateTestTableAsync();
    Task<object> TestInsertPersonalAsync();
}

public class DatabaseTestService : IDatabaseTestService
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseTestService> _logger;

    public DatabaseTestService(IConfiguration configuration, ILogger<DatabaseTestService> logger)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        _logger = logger;
    }

    public async Task<bool> TestConnectionAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            _logger.LogInformation("Database connection successful");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database connection failed");
            return false;
        }
    }

    public async Task<bool> TableExistsAsync(string tableName)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_NAME = @TableName";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@TableName", tableName);

            var count = (int)await command.ExecuteScalarAsync();
            _logger.LogInformation("Table {TableName} exists: {Exists}", tableName, count > 0);
            return count > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if table {TableName} exists", tableName);
            return false;
        }
    }

    public async Task<string> CreateTestTableAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Check if PersonalMedical table exists
            var tableExists = await TableExistsAsync("PersonalMedical");
            if (!tableExists)
            {
                var createTableQuery = @"
                    CREATE TABLE PersonalMedical (
                        PersonalID UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID() PRIMARY KEY,
                        Nume NVARCHAR(100) NOT NULL,
                        Prenume NVARCHAR(100) NOT NULL,
                        Specializare NVARCHAR(100),
                        NumarLicenta NVARCHAR(50) UNIQUE,
                        Telefon NVARCHAR(20),
                        Email NVARCHAR(100),
                        Departament NVARCHAR(100),
                        Pozitie NVARCHAR(50),
                        EsteActiv BIT DEFAULT 1,
                        DataCreare DATETIME2 DEFAULT GETDATE()
                    )";

                using var command = new SqlCommand(createTableQuery, connection);
                await command.ExecuteNonQueryAsync();
                _logger.LogInformation("PersonalMedical table created successfully");
                return "PersonalMedical table created successfully";
            }
            else
            {
                _logger.LogInformation("PersonalMedical table already exists");
                return "PersonalMedical table already exists";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PersonalMedical table");
            return $"Error creating table: {ex.Message}";
        }
    }

    public async Task<object> TestInsertPersonalAsync()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var insertQuery = @"
                INSERT INTO PersonalMedical (Nume, Prenume, Pozitie, Specializare, NumarLicenta, Telefon, Email, Departament, EsteActiv)
                OUTPUT INSERTED.PersonalID
                VALUES (@Nume, @Prenume, @Pozitie, @Specializare, @NumarLicenta, @Telefon, @Email, @Departament, @EsteActiv)";

            using var command = new SqlCommand(insertQuery, connection);
            command.Parameters.AddWithValue("@Nume", "Test");
            command.Parameters.AddWithValue("@Prenume", "User");
            command.Parameters.AddWithValue("@Pozitie", "Doctor");
            command.Parameters.AddWithValue("@Specializare", "Medicina Generala");
            command.Parameters.AddWithValue("@NumarLicenta", $"TEST{DateTime.Now.Ticks}");
            command.Parameters.AddWithValue("@Telefon", "0721234567");
            command.Parameters.AddWithValue("@Email", $"test{DateTime.Now.Ticks}@test.com");
            command.Parameters.AddWithValue("@Departament", "Medicina interna");
            command.Parameters.AddWithValue("@EsteActiv", true);

            var insertedId = await command.ExecuteScalarAsync();
            _logger.LogInformation("Test record inserted successfully with ID: {Id}", insertedId);
            
            return new { success = true, insertedId = insertedId };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting test record");
            return new { success = false, error = ex.Message };
        }
    }
}