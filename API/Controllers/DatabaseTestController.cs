using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Shared.Common;
using Shared.DTOs.Medical;
using Application.Services.Medical;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseTestController : ControllerBase
{
    private readonly IDatabaseTestService _databaseTestService;
    private readonly ILogger<DatabaseTestController> _logger;

    public DatabaseTestController(IDatabaseTestService databaseTestService, ILogger<DatabaseTestController> logger)
    {
        _databaseTestService = databaseTestService;
        _logger = logger;
    }

    [HttpGet("connection")]
    public async Task<ActionResult<object>> TestConnection()
    {
        try
        {
            var canConnect = await _databaseTestService.TestConnectionAsync();
            return Ok(new { 
                success = canConnect, 
                message = canConnect ? "Database connection successful" : "Database connection failed",
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing database connection");
            return StatusCode(500, new { 
                success = false, 
                message = "Error testing connection", 
                error = ex.Message,
                timestamp = DateTime.UtcNow 
            });
        }
    }

    [HttpGet("table/{tableName}")]
    public async Task<ActionResult<object>> CheckTable(string tableName)
    {
        try
        {
            var exists = await _databaseTestService.TableExistsAsync(tableName);
            return Ok(new { 
                tableName = tableName,
                exists = exists, 
                message = exists ? $"Table '{tableName}' exists" : $"Table '{tableName}' does not exist",
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking table {TableName}", tableName);
            return StatusCode(500, new { 
                tableName = tableName,
                exists = false, 
                message = "Error checking table", 
                error = ex.Message,
                timestamp = DateTime.UtcNow 
            });
        }
    }

    [HttpPost("create-personal-table")]
    public async Task<ActionResult<object>> CreatePersonalTable()
    {
        try
        {
            var result = await _databaseTestService.CreateTestTableAsync();
            return Ok(new { 
                success = true, 
                message = result,
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PersonalMedical table");
            return StatusCode(500, new { 
                success = false, 
                message = "Error creating table", 
                error = ex.Message,
                timestamp = DateTime.UtcNow 
            });
        }
    }

    [HttpPost("test-insert")]
    public async Task<ActionResult<object>> TestInsert()
    {
        try
        {
            var result = await _databaseTestService.TestInsertPersonalAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test insert endpoint");
            return StatusCode(500, new { 
                success = false, 
                message = "Error in test insert endpoint", 
                error = ex.Message,
                timestamp = DateTime.UtcNow 
            });
        }
    }

    [HttpPost("test-result-serialization")]
    public ActionResult<object> TestResultSerialization()
    {
        try
        {
            // Test serialization of Result<Guid>
            var successResult = Result<Guid>.Success(Guid.NewGuid(), "Test success message");
            var failureResult = Result<Guid>.Failure("Test error 1", "Test error 2");
            
            // Test serialization of Result
            var successResultSimple = Result.Success("Simple success");
            var failureResultSimple = Result.Failure("Simple error 1", "Simple error 2");

            return Ok(new { 
                success = true,
                message = "Serialization test successful",
                testResults = new {
                    successResultWithGuid = successResult,
                    failureResultWithGuid = failureResult,
                    successResultSimple = successResultSimple,
                    failureResultSimple = failureResultSimple
                },
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in serialization test endpoint");
            return StatusCode(500, new { 
                success = false, 
                message = "Serialization test failed", 
                error = ex.Message,
                timestamp = DateTime.UtcNow 
            });
        }
    }

    [HttpPost("test-personal-create")]
    public async Task<ActionResult<Result<Guid>>> TestPersonalCreate()
    {
        try
        {
            var testRequest = new CreatePersonalMedicalRequest
            {
                Nume = "Test",
                Prenume = "Serialization",
                Pozitie = "Doctor",
                Specializare = "Test Specialization",
                NumarLicenta = $"TEST{DateTime.Now.Ticks}",
                Telefon = "0721111111",
                Email = $"test{DateTime.Now.Ticks}@test.com",
                Departament = "Test Department",
                EsteActiv = true
            };

            _logger.LogInformation("Testing personal creation with request: {@Request}", testRequest);

            // Simulate the same call that the API controller would make
            var personalMedicalService = HttpContext.RequestServices.GetRequiredService<IPersonalMedicalService>();
            var result = await personalMedicalService.CreateAsync(testRequest);

            _logger.LogInformation("Personal creation result: {@Result}", result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in test personal create endpoint");
            return StatusCode(500, Result<Guid>.Failure($"Test personal create failed: {ex.Message}"));
        }
    }

    [HttpGet("test-deserialize/{testJson}")]
    public ActionResult<object> TestDeserialize(string testJson)
    {
        try
        {
            _logger.LogInformation("Testing deserialization of: {Json}", testJson);
            
            // Decode base64 if needed
            string decodedJson;
            try
            {
                var bytes = Convert.FromBase64String(testJson);
                decodedJson = System.Text.Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                decodedJson = Uri.UnescapeDataString(testJson);
            }
            
            _logger.LogInformation("Decoded JSON: {DecodedJson}", decodedJson);
            
            var options = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
            };
            
            var result = System.Text.Json.JsonSerializer.Deserialize<Result<Guid>>(decodedJson, options);
            
            return Ok(new { 
                success = true,
                message = "Deserialization successful",
                originalJson = decodedJson,
                deserializedResult = new {
                    IsSuccess = result?.IsSuccess,
                    Value = result?.Value,
                    ErrorsCount = result?.Errors?.Count ?? 0,
                    Errors = result?.Errors,
                    SuccessMessage = result?.SuccessMessage
                },
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Deserialization test failed for: {Json}", testJson);
            return StatusCode(500, new { 
                success = false, 
                message = "Deserialization failed", 
                error = ex.Message,
                stackTrace = ex.StackTrace,
                timestamp = DateTime.UtcNow 
            });
        }
    }
}