using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using Dapper;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DbCheckController : ControllerBase
    {
        private readonly string _connectionString;

        public DbCheckController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet("ping")]
        public async Task<IActionResult> Ping()
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var cmd = connection.CreateCommand();
                cmd.CommandText = @"SELECT DB_NAME() AS DatabaseName, SUSER_SNAME() AS LoginName, @@SERVERNAME AS ServerName, @@VERSION AS SqlServerVersion";
                await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.SingleRow);

                if (await reader.ReadAsync())
                {
                    return Ok(new
                    {
                        success = true,
                        database = reader["DatabaseName"],
                        login = reader["LoginName"],
                        server = reader["ServerName"],
                        version = reader["SqlServerVersion"]
                    });
                }

                return Ok(new { success = true, message = "Connection opened, information unavailable." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Attempts a write operation inside a transaction that is rolled back
        // to validate write permissions without leaving any artifacts.
        [HttpPost("write-test")]
        public async Task<IActionResult> WriteTest()
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                await using var transaction = await connection.BeginTransactionAsync();

                try
                {
                    await using var cmd = connection.CreateCommand();
                    cmd.Transaction = (SqlTransaction)transaction;
                    // Attempt to create a table (DDL) to verify modification rights. This will be rolled back.
                    cmd.CommandText = @"CREATE TABLE dbo.__ValyanMed_WriteTest (Id INT NOT NULL);";
                    await cmd.ExecuteNonQueryAsync();

                    // Optionally insert a row to validate DML as well
                    cmd.CommandText = @"INSERT INTO dbo.__ValyanMed_WriteTest (Id) VALUES (1);";
                    await cmd.ExecuteNonQueryAsync();

                    // Rollback so no changes persist
                    await transaction.RollbackAsync();

                    return Ok(new { success = true, message = "Write test succeeded (transaction rolled back)." });
                }
                catch (Exception innerEx)
                {
                    // Ensure rollback if possible
                    try { if (transaction.Connection != null) await transaction.RollbackAsync(); } catch { /* ignore */ }
                    return StatusCode(500, new { success = false, message = "Write test failed.", error = innerEx.Message });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // Execute Partener table setup
        [HttpPost("setup-partener")]
        public async Task<IActionResult> SetupPartener()
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var results = new List<string>();

                // 1. Create table
                var createTableSql = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Partener' AND schema_id = SCHEMA_ID('dbo'))
                BEGIN
                    CREATE TABLE [dbo].[Partener] (
                        [PartenerId]           INT              IDENTITY (1, 1) NOT NULL,
                        [PartenerGuid]         UNIQUEIDENTIFIER DEFAULT (newsequentialid()) NOT NULL,
                        [CodIntern]            NVARCHAR (50)    NOT NULL,
                        [Denumire]             NVARCHAR (200)   NOT NULL,
                        [CodFiscal]            NVARCHAR (50)    NULL,
                        [Judet]                NVARCHAR (100)   NULL,
                        [Localitate]           NVARCHAR (100)   NULL,
                        [Adresa]               NVARCHAR (500)   NULL,
                        [DataCreare]           DATETIME2        DEFAULT (GETDATE()) NOT NULL,
                        [DataActualizare]      DATETIME2        DEFAULT (GETDATE()) NOT NULL,
                        [UtilizatorCreare]     NVARCHAR (100)   NULL,
                        [UtilizatorActualizare] NVARCHAR (100)  NULL,
                        [Activ]                BIT              DEFAULT (1) NOT NULL,
                        
                        CONSTRAINT [PK_Partener] PRIMARY KEY CLUSTERED ([PartenerId] ASC),
                        CONSTRAINT [UQ_Partener_Guid] UNIQUE ([PartenerGuid]),
                        CONSTRAINT [UQ_Partener_CodIntern] UNIQUE ([CodIntern])
                    );
                    
                    CREATE NONCLUSTERED INDEX [IX_Partener_CodFiscal] ON [dbo].[Partener] ([CodFiscal]);
                    CREATE NONCLUSTERED INDEX [IX_Partener_Denumire] ON [dbo].[Partener] ([Denumire]);
                    CREATE NONCLUSTERED INDEX [IX_Partener_Judet] ON [dbo].[Partener] ([Judet]);
                    CREATE NONCLUSTERED INDEX [IX_Partener_Activ] ON [dbo].[Partener] ([Activ]);
                    
                    SELECT 'Table created' as Result;
                END
                ELSE
                BEGIN
                    SELECT 'Table already exists' as Result;
                END";

                await using var createCmd = connection.CreateCommand();
                createCmd.CommandText = createTableSql;
                var tableResult = await createCmd.ExecuteScalarAsync();
                results.Add($"Table: {tableResult}");

                // 2. Create sp_Partener_GetAll
                var createGetAllSql = @"
                IF OBJECT_ID('dbo.sp_Partener_GetAll','P') IS NULL
                    EXEC('CREATE PROCEDURE dbo.sp_Partener_GetAll AS BEGIN SET NOCOUNT ON; END');
                
                EXEC('ALTER PROCEDURE dbo.sp_Partener_GetAll
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT PartenerId, PartenerGuid, CodIntern, Denumire, CodFiscal, Judet, Localitate, Adresa,
                           DataCreare, DataActualizare, UtilizatorCreare, UtilizatorActualizare, Activ
                    FROM dbo.Partener WITH (NOLOCK)
                    WHERE Activ = 1
                    ORDER BY Denumire ASC;
                END');";

                await using var getAllCmd = connection.CreateCommand();
                getAllCmd.CommandText = createGetAllSql;
                await getAllCmd.ExecuteNonQueryAsync();
                results.Add("sp_Partener_GetAll created");

                // 3. Create sp_Partener_GetById
                var createGetByIdSql = @"
                IF OBJECT_ID('dbo.sp_Partener_GetById','P') IS NULL
                    EXEC('CREATE PROCEDURE dbo.sp_Partener_GetById AS BEGIN SET NOCOUNT ON; END');
                
                EXEC('ALTER PROCEDURE dbo.sp_Partener_GetById
                    @PartenerId INT
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SELECT PartenerId, PartenerGuid, CodIntern, Denumire, CodFiscal, Judet, Localitate, Adresa,
                           DataCreare, DataActualizare, UtilizatorCreare, UtilizatorActualizare, Activ
                    FROM dbo.Partener WITH (NOLOCK)
                    WHERE PartenerId = @PartenerId;
                END');";

                await using var getByIdCmd = connection.CreateCommand();
                getByIdCmd.CommandText = createGetByIdSql;
                await getByIdCmd.ExecuteNonQueryAsync();
                results.Add("sp_Partener_GetById created");

                // Test the setup
                await using var testCmd = connection.CreateCommand();
                testCmd.CommandText = "SELECT COUNT(*) FROM dbo.Partener";
                var count = await testCmd.ExecuteScalarAsync();
                results.Add($"Partener table contains {count} records");

                return Ok(new
                {
                    success = true,
                    message = "Partener table and basic procedures created successfully",
                    details = results
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message, details = ex.ToString() });
            }
        }

        // Test data access from Partener table
        [HttpGet("test-partener")]
        public async Task<IActionResult> TestPartener()
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                    SELECT TOP 5 
                        PartenerId, 
                        CodIntern, 
                        Denumire, 
                        CodFiscal, 
                        Judet,
                        Localitate,
                        Activ,
                        DataCreare
                    FROM dbo.Partener 
                    ORDER BY PartenerId";
                
                await using var reader = await cmd.ExecuteReaderAsync();
                
                var parteneri = new List<object>();
                while (await reader.ReadAsync())
                {
                    parteneri.Add(new
                    {
                        id = reader["PartenerId"],
                        codIntern = reader["CodIntern"],
                        denumire = reader["Denumire"],
                        codFiscal = reader["CodFiscal"],
                        judet = reader["Judet"],
                        localitate = reader["Localitate"],
                        activ = reader["Activ"],
                        dataCreare = reader["DataCreare"]
                    });
                }

                // Also get count
                cmd.CommandText = "SELECT COUNT(*) FROM dbo.Partener";
                await using var countReader = await cmd.ExecuteReaderAsync();
                int totalCount = 0;
                if (await countReader.ReadAsync())
                {
                    totalCount = countReader.GetInt32(0);
                }

                return Ok(new
                {
                    success = true,
                    totalParteneri = totalCount,
                    sampleData = parteneri,
                    message = $"Successfully retrieved {parteneri.Count} sample records from {totalCount} total partners"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message, details = ex.ToString() });
            }
        }

        // Test judete and localitati data
        [HttpGet("test-judete")]
        public async Task<IActionResult> TestJudete()
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Test judete
                await using var judetCmd = connection.CreateCommand();
                judetCmd.CommandText = @"
                    SELECT TOP 5 
                        IdJudet, 
                        Nume,
                        CodJudet,
                        Siruta
                    FROM dbo.Judet 
                    ORDER BY Nume";
                
                await using var judetReader = await judetCmd.ExecuteReaderAsync();
                
                var judete = new List<object>();
                while (await judetReader.ReadAsync())
                {
                    judete.Add(new
                    {
                        id = judetReader["IdJudet"],
                        nume = judetReader["Nume"],
                        codJudet = judetReader["CodJudet"],
                        siruta = judetReader["Siruta"]
                    });
                }

                // Get judete count
                judetCmd.CommandText = "SELECT COUNT(*) FROM dbo.Judet";
                await using var judetCountReader = await judetCmd.ExecuteReaderAsync();
                int judetCount = 0;
                if (await judetCountReader.ReadAsync())
                {
                    judetCount = judetCountReader.GetInt32(0);
                }

                // Test localitati for first judet if exists
                var localitati = new List<object>();
                int localitateCount = 0;
                if (judete.Count > 0)
                {
                    var firstJudetId = judete[0].GetType().GetProperty("id")?.GetValue(judete[0]);
                    
                    judetCmd.CommandText = @"
                        SELECT TOP 5 
                            IdOras, 
                            Nume,
                            IdJudet
                        FROM dbo.Localitate 
                        WHERE IdJudet = @IdJudet
                        ORDER BY Nume";
                    judetCmd.Parameters.Clear();
                    judetCmd.Parameters.AddWithValue("@IdJudet", firstJudetId);
                    
                    await using var localitateReader = await judetCmd.ExecuteReaderAsync();
                    while (await localitateReader.ReadAsync())
                    {
                        localitati.Add(new
                        {
                            id = localitateReader["IdOras"],  // Corected from IdLocalitate to IdOras
                            nume = localitateReader["Nume"],
                            idJudet = localitateReader["IdJudet"]
                        });
                    }

                    // Get localitati count for this judet
                    judetCmd.CommandText = "SELECT COUNT(*) FROM dbo.Localitate WHERE IdJudet = @IdJudet";
                    await using var localitateCountReader = await judetCmd.ExecuteReaderAsync();
                    if (await localitateCountReader.ReadAsync())
                    {
                        localitateCount = localitateCountReader.GetInt32(0);
                    }
                }

                return Ok(new
                {
                    success = true,
                    totalJudete = judetCount,
                    sampleJudete = judete,
                    totalLocalitati = localitateCount,
                    sampleLocalitati = localitati,
                    message = $"Successfully retrieved {judete.Count} sample judete from {judetCount} total and {localitati.Count} sample localitati from {localitateCount} total"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message, details = ex.ToString() });
            }
        }

        // Test dispozitive medicale data
        [HttpGet("test-dispozitive")]
        public async Task<ActionResult> TestDispozitive()
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Test if procedures exist
                var procedureCheck = await connection.QueryAsync<string>(@"
                    SELECT name FROM sys.procedures 
                    WHERE name IN ('sp_DispozitiveMedicale_GetAll', 'sp_DispozitiveMedicale_GetPaged', 'sp_DispozitiveMedicale_Create')
                ");

                // Test table structure
                var columns = await connection.QueryAsync<dynamic>(@"
                    SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'DispozitiveMedicale' 
                      AND TABLE_SCHEMA = 'dbo'
                    ORDER BY ORDINAL_POSITION
                ");

                // Get sample data
                var sampleData = await connection.QueryAsync<dynamic>(@"
                    SELECT TOP 3 * FROM dbo.DispozitiveMedicale ORDER BY DataCreare DESC
                ");

                // Get count
                var totalCount = await connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM dbo.DispozitiveMedicale");

                return Ok(new
                {
                    success = true,
                    procedures = procedureCheck.ToList(),
                    columns = columns.ToList(),
                    totalCount = totalCount,
                    sampleData = sampleData.ToList(),
                    message = $"DispozitiveMedicale: {procedureCheck.Count()} procedures found, {totalCount} records total"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message, details = ex.ToString() });
            }
        }

        // Test materiale sanitare data
        [HttpGet("test-materiale")]
        public async Task<ActionResult> TestMateriale()
        {
            try
            {
                await using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                // Test if procedures exist
                var procedureCheck = await connection.QueryAsync<string>(@"
                    SELECT name FROM sys.procedures 
                    WHERE name IN ('sp_MaterialeSanitare_GetAll', 'sp_MaterialeSanitare_GetPaged', 'sp_MaterialeSanitare_Create')
                ");

                // Test table structure
                var columns = await connection.QueryAsync<dynamic>(@"
                    SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'MaterialeSanitare' 
                      AND TABLE_SCHEMA = 'dbo'
                    ORDER BY ORDINAL_POSITION
                ");

                // Get sample data
                var sampleData = await connection.QueryAsync<dynamic>(@"
                    SELECT TOP 3 * FROM dbo.MaterialeSanitare ORDER BY DataCreare DESC
                ");

                // Get count
                var totalCount = await connection.QueryFirstAsync<int>("SELECT COUNT(*) FROM dbo.MaterialeSanitare");

                return Ok(new
                {
                    success = true,
                    procedures = procedureCheck.ToList(),
                    columns = columns.ToList(),
                    totalCount = totalCount,
                    sampleData = sampleData.ToList(),
                    message = $"MaterialeSanitare: {procedureCheck.Count()} procedures found, {totalCount} records total"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message, details = ex.ToString() });
            }
        }
    }
}
