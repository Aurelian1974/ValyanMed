using Application.Services.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Authentication;
using Shared.Exceptions;

namespace API.Controllers.Authentication;

[ApiController]
[Route("api/[controller]")]
public class UtilizatoriController : ControllerBase
{
    private readonly IUtilizatorService _utilizatorService;
    private readonly IPersoanaService _persoanaService;

    public UtilizatoriController(IUtilizatorService utilizatorService, IPersoanaService persoanaService)
    {
        _utilizatorService = utilizatorService;
        _persoanaService = persoanaService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUtilizatorRequest request)
    {
        try
        {
            var result = await _utilizatorService.CreateAsync(request);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value, message = result.SuccessMessage });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
        catch (DuplicateException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Eroare intern? de server", details = ex.Message });
        }
    }

    // Endpoint temporar pentru crearea unui utilizator de test - NU PENTRU PRODUCȚIE!
    [HttpPost("create-test-user")]
    public async Task<IActionResult> CreateTestUser()
    {
        try
        {
            _debugInfo = "=== DETAILED TEST USER CREATION ===\n\n";
            
            _debugInfo += "1. Checking existing users...\n";
            
            // Verifică dacă deja există utilizatori
            var existingUsersResult = await _utilizatorService.GetAllAsync();
            if (existingUsersResult.IsSuccess && existingUsersResult.Value?.Any() == true)
            {
                var users = existingUsersResult.Value.ToList();
                var adminUser = users.FirstOrDefault(u => u.NumeUtilizator == "admin");
                
                if (adminUser != null)
                {
                    // Admin există deja
                    return Ok(new { 
                        message = "Utilizatorul 'admin' există deja în baza de date",
                        users = users.Select(u => new { u.Id, u.NumeUtilizator, u.Email }).ToList(),
                        adminExists = true,
                        adminUser = new { adminUser.Id, adminUser.NumeUtilizator, adminUser.Email },
                        credentials = new { username = "admin", password = "Admin123!" },
                        note = "Poți folosi admin/Admin123! pentru login",
                        instructions = "Try 'Test Real Login' cu credentialele de mai sus"
                    });
                }
                else
                {
                    // Există utilizatori dar nu admin - continuă să creezi admin
                    _debugInfo += $"2. Found {users.Count} users but no 'admin' user. Creating admin...\n";
                }
            }
            else
            {
                _debugInfo += "2. No users found, creating first user 'admin'...\n";
            }

            // Verifică dacă există persoane în baza de date
            _debugInfo += "3. Checking existing persons...\n";
            var existingPersonsResult = await _persoanaService.GetAllAsync();
            int persoanaId = 1;

            if (!existingPersonsResult.IsSuccess || !existingPersonsResult.Value?.Any() == true)
            {
                _debugInfo += "4. No persons found, creating test person...\n";
                
                // Creează o persoană de test mai întâi
                var testPersonRequest = new CreatePersoanaRequest(
                    "Admin",
                    "Test",
                    "București",
                    "București", 
                    "Strada Test",
                    "1",
                    "012345",
                    "Administrator Sistem",
                    DateTime.Now.AddYears(-30),
                    "1234567890123",
                    Shared.Enums.TipActIdentitate.CarteIdentitate,
                    "AB",
                    "123456",
                    Shared.Enums.StareCivila.Necasatorit,
                    Shared.Enums.Gen.Masculin
                );

                var personResult = await _persoanaService.CreateAsync(testPersonRequest);
                if (!personResult.IsSuccess)
                {
                    return BadRequest(new { 
                        step = "Creating test person",
                        errors = personResult.Errors,
                        message = "Nu s-a putut crea persoana de test",
                        debugInfo = _debugInfo
                    });
                }
                
                persoanaId = personResult.Value;
                _debugInfo += $"5. Test person created with ID: {persoanaId}\n";
            }
            else
            {
                var persons = existingPersonsResult.Value.ToList();
                persoanaId = persons.First().Id;
                _debugInfo += $"5. Using existing person with ID: {persoanaId}\n";
                _debugInfo += $"   Available persons: {persons.Count}\n";
            }

            _debugInfo += "6. Creating admin user...\n";

            // Creează utilizatorul admin
            var testUserRequest = new CreateUtilizatorRequest(
                "admin",
                "Admin123!",  // Parolă care respectă toate cerințele: A(mare) + dmin(mici) + 123(cifre) + !(special)
                "admin@valyanmed.com",
                "+40712345678",
                persoanaId
            );

            _debugInfo += "7. Calling UtilizatorService.CreateAsync for admin...\n";
            var result = await _utilizatorService.CreateAsync(testUserRequest);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { 
                    step = "Creating admin user",
                    errors = result.Errors,
                    message = "Nu s-a putut crea utilizatorul admin",
                    debugInfo = _debugInfo,
                    persoanaIdUsed = persoanaId,
                    requestData = new {
                        username = testUserRequest.NumeUtilizator,
                        email = testUserRequest.Email,
                        phone = testUserRequest.Telefon,
                        personId = testUserRequest.PersoanaId
                    }
                });
            }

            _debugInfo += $"8. Admin user created successfully with ID: {result.Value}\n";

            return Ok(new { 
                message = "Utilizatorul 'admin' a fost creat cu succes!", 
                userId = result.Value,
                persoanaId = persoanaId,
                credentials = new { username = "admin", password = "Admin123!" },
                instructions = "Acum poți folosi admin/Admin123! pentru login",
                debugInfo = _debugInfo
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                message = "Eroare la crearea utilizatorului admin", 
                details = ex.Message,
                stackTrace = ex.StackTrace,
                debugInfo = _debugInfo ?? "Debug info not available"
            });
        }
    }

    private string _debugInfo = "";

    // Debug endpoint pentru verificarea parolei - NU PENTRU PRODUCȚIE!
    [HttpPost("debug-password")]
    public async Task<IActionResult> DebugPassword([FromBody] DebugPasswordRequest request)
    {
        try
        {
            // Găsește utilizatorul
            var usersResult = await _utilizatorService.GetAllAsync();
            if (!usersResult.IsSuccess)
            {
                return BadRequest(new { message = "Nu se pot încărca utilizatorii" });
            }

            var user = usersResult.Value?.FirstOrDefault(u => u.NumeUtilizator == request.Username);
            if (user == null)
            {
                return NotFound(new { message = $"Utilizatorul '{request.Username}' nu a fost găsit" });
            }

            // Pentru debugging - în producție nu afișa niciodată hash-urile!
            return Ok(new 
            { 
                message = "Debug info pentru utilizator",
                username = user.NumeUtilizator,
                email = user.Email,
                storedHashPreview = user.ParolaHash?.Substring(0, Math.Min(20, user.ParolaHash.Length)) + "...",
                hashFormat = user.ParolaHash?.Split('.').Length == 3 ? "Valid format" : "Invalid format",
                hashFullLength = user.ParolaHash?.Length ?? 0,
                instructions = "Verifică în logs-urile API dacă VerifyPassword returnează true"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Eroare la debug password", details = ex.Message });
        }
    }

    public record DebugPasswordRequest(string Username);

    // Endpoint pentru recrearea utilizatorului admin cu parola corectă
    [HttpPost("recreate-admin")]
    public async Task<IActionResult> RecreateAdmin()
    {
        try
        {
            // Șterge utilizatorul admin existent dacă există
            var existingUsersResult = await _utilizatorService.GetAllAsync();
            if (existingUsersResult.IsSuccess && existingUsersResult.Value?.Any() == true)
            {
                var adminUser = existingUsersResult.Value.FirstOrDefault(u => u.NumeUtilizator == "admin");
                if (adminUser != null)
                {
                    await _utilizatorService.DeleteAsync(adminUser.Id);
                }
            }

            // Verifică dacă există persoane în baza de date
            var existingPersonsResult = await _persoanaService.GetAllAsync();
            int persoanaId = 1;

            if (existingPersonsResult.IsSuccess && existingPersonsResult.Value?.Any() == true)
            {
                persoanaId = existingPersonsResult.Value.First().Id;
            }
            else
            {
                // Creează o persoană de test
                var testPersonRequest = new CreatePersoanaRequest(
                    "Admin",
                    "Test", 
                    "București",
                    "București",
                    "Strada Test",
                    "1",
                    "012345",
                    "Administrator Sistem",
                    DateTime.Now.AddYears(-30),
                    "1234567890123",
                    Shared.Enums.TipActIdentitate.CarteIdentitate,
                    "AB",
                    "123456",
                    Shared.Enums.StareCivila.Necasatorit,
                    Shared.Enums.Gen.Masculin
                );

                var personResult = await _persoanaService.CreateAsync(testPersonRequest);
                if (!personResult.IsSuccess)
                {
                    return BadRequest(new { 
                        message = "Nu s-a putut crea persoana pentru admin",
                        errors = personResult.Errors
                    });
                }
                persoanaId = personResult.Value;
            }

            // Creează utilizatorul admin cu parola corectă
            var adminRequest = new CreateUtilizatorRequest(
                "admin",
                "Admin123!",
                "admin@valyanmed.com",
                "+40712345678",
                persoanaId
            );

            var result = await _utilizatorService.CreateAsync(adminRequest);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { 
                    message = "Nu s-a putut recrea utilizatorul admin",
                    errors = result.Errors
                });
            }

            return Ok(new { 
                message = "Utilizatorul admin a fost recreat cu succes!",
                userId = result.Value,
                credentials = new { username = "admin", password = "Admin123!" },
                instructions = "Acum poți folosi admin/Admin123! pentru login"
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { 
                message = "Eroare la recrearea utilizatorului admin", 
                details = ex.Message 
            });
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _utilizatorService.GetByIdAsync(id);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            if (result.Value == null)
            {
                return NotFound(new { message = "Utilizatorul nu a fost g?sit" });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Eroare intern? de server", details = ex.Message });
        }
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _utilizatorService.GetAllAsync();
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Eroare intern? de server", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUtilizatorRequest request)
    {
        try
        {
            if (id != request.Id)
            {
                return BadRequest(new { message = "ID-ul din URL nu corespunde cu cel din request" });
            }

            var result = await _utilizatorService.UpdateAsync(request);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(new { message = result.SuccessMessage });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (DuplicateException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Eroare intern? de server", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _utilizatorService.DeleteAsync(id);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(new { message = result.SuccessMessage });
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (BusinessRuleException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Eroare intern? de server", details = ex.Message });
        }
    }
}