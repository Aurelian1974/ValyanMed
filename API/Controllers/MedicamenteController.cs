using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Application.Services;
using API.Services;
using Microsoft.AspNetCore.Authorization;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicamenteController : ControllerBase
    {
        private readonly IMedicamentService _service;
        private readonly ICurrentUserService _currentUserService;
        
        public MedicamenteController(IMedicamentService service, ICurrentUserService currentUserService) 
        {
            _service = service;
            _currentUserService = currentUserService;
            Console.WriteLine("MedicamentController instantiated");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedicamentDTO>>> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<MedicamentDTO>>> GetPaged([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? sort = null, [FromQuery] string? groupBy = null)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 5000) pageSize = 5000; // safety cap
            var result = await _service.GetPagedAsync(search, status, page, pageSize, sort, groupBy);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MedicamentDTO>> GetById(int id)
        {
            var item = await _service.GetByIdAsync(id);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpPost]
        [Authorize] // Only protect write operations for now
        public async Task<ActionResult<int>> Create([FromBody] CreateMedicamentDTO dto)
        {
            Console.WriteLine($"=== CREATE MEDICAMENT ENTERED ===");
            
            // Check model state first
            if (!ModelState.IsValid)
            {
                Console.WriteLine("CREATE ModelState is INVALID:");
                foreach (var kvp in ModelState)
                {
                    Console.WriteLine($"  Key: {kvp.Key}");
                    foreach (var error in kvp.Value.Errors)
                    {
                        Console.WriteLine($"    Error: {error.ErrorMessage}");
                    }
                }
                return BadRequest(ModelState);
            }
            
            // ALWAYS set the current user, regardless of what's in the DTO
            var currentUser = _currentUserService.GetCurrentUserFullName();
            dto.UtilizatorActualizare = currentUser;
            Console.WriteLine($"CREATE: Setting UtilizatorActualizare to: '{currentUser}'");
            
            var id = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id}")]
        [Authorize] // Only protect write operations for now
        public async Task<ActionResult> Update(int id, [FromBody] UpdateMedicamentDTO dto)
        {
            Console.WriteLine($"=== UPDATE MEDICAMENT ENTERED ===");
            
            // Temporarily skip ModelState validation to see if that's the issue
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is INVALID - but continuing anyway for debugging:");
                foreach (var kvp in ModelState)
                {
                    Console.WriteLine($"  Key: {kvp.Key}");
                    foreach (var error in kvp.Value.Errors)
                    {
                        Console.WriteLine($"    Error: {error.ErrorMessage}");
                    }
                }
                // Don't return BadRequest here - continue to see what happens
            }
            
            try
            {
                Console.WriteLine($"=== UPDATE MEDICAMENT DEBUG ===");
                Console.WriteLine($"Request received for ID: {id}");
                Console.WriteLine($"DTO ID: {dto?.MedicamentID}");
                Console.WriteLine($"DTO is null: {dto == null}");
                Console.WriteLine($"User authenticated: {User.Identity?.IsAuthenticated}");
                Console.WriteLine($"DTO UtilizatorActualizare before setting: '{dto?.UtilizatorActualizare}'");
                
                if (dto == null)
                {
                    return BadRequest(new { Success = false, Message = "DTO is null" });
                }
                
                if (id != dto.MedicamentID)
                {
                    return BadRequest(new { Success = false, Message = "ID din URL nu corespunde cu ID din DTO" });
                }

                // ALWAYS set the current user, regardless of what's in the DTO
                var currentUser = _currentUserService.GetCurrentUserFullName();
                dto.UtilizatorActualizare = currentUser;
                
                Console.WriteLine($"Setting UtilizatorActualizare to: '{currentUser}'");
                    
                var ok = await _service.UpdateAsync(dto);
                
                if (ok)
                {
                    return Ok(new { Success = true, Message = "Medicament actualizat cu succes" });
                }
                else
                {
                    return NotFound(new { Success = false, Message = "Medicamentul nu a fost gasit" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception in Update: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { Success = false, Message = $"Eroare interna: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize] // Only protect write operations for now
        public async Task<ActionResult> Delete(int id)
            => Ok(await _service.DeleteAsync(id) ? "Deleted" : "Not found");

        [HttpGet("test-auth")]
        [Authorize]
        public ActionResult TestAuth()
        {
            Console.WriteLine($"=== TEST AUTH ENDPOINT ===");
            Console.WriteLine($"Request Headers:");
            foreach (var header in Request.Headers)
            {
                Console.WriteLine($"  {header.Key}: {string.Join(", ", header.Value.ToArray())}");
            }
            Console.WriteLine($"User authenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"User name: {User.Identity?.Name}");
            Console.WriteLine($"Claims count: {User.Claims.Count()}");
            
            foreach (var claim in User.Claims)
            {
                Console.WriteLine($"Claim: {claim.Type} = {claim.Value}");
            }
            
            var currentUser = _currentUserService.GetCurrentUserFullName();
            Console.WriteLine($"CurrentUserService returned: {currentUser}");
            
            return Ok(new { 
                Authenticated = User.Identity?.IsAuthenticated,
                UserName = User.Identity?.Name,
                CurrentUser = currentUser,
                Claims = User.Claims.Select(c => new { Type = c.Type, Value = c.Value })
            });
        }

        [HttpPut("test/{id}")]
        [Authorize]
        public ActionResult TestUpdate(int id)
        {
            Console.WriteLine($"=== TEST UPDATE ENDPOINT REACHED ===");
            Console.WriteLine($"ID: {id}");
            Console.WriteLine($"User authenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"User name: {User.Identity?.Name}");
            
            var currentUser = _currentUserService.GetCurrentUserFullName();
            Console.WriteLine($"CurrentUserService returned: '{currentUser}'");
            
            return Ok(new { 
                Message = "Test update endpoint reached",
                Id = id,
                User = currentUser,
                Authenticated = User.Identity?.IsAuthenticated
            });
        }
    }
}
