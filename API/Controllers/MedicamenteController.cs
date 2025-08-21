using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using API.Services; // for ICurrentUserService
using Application.Services; // for IMedicamentService

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicamenteController : ControllerBase
    {
        private readonly IMedicamentService _service;
        private readonly ILogger<MedicamenteController> _logger;
        private readonly ICurrentUserService _currentUserService;

        public MedicamenteController(IMedicamentService service, ILogger<MedicamenteController> logger, ICurrentUserService currentUserService)
        {
            _service = service;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        // Existing default GET that also returns paged data
        [HttpGet]
        public async Task<ActionResult<PagedResult<MedicamentDTO>>> Get([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? sort = null)
            => Ok(await _service.GetPagedAsync(search, status, page, pageSize, sort, null));

        // Explicit alias used by the client: GET api/medicamente/paged
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<MedicamentDTO>>> GetPaged([FromQuery] string? search, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, [FromQuery] string? sort = null, [FromQuery] string? groupBy = null)
            => Ok(await _service.GetPagedAsync(search, status, page, pageSize, sort, groupBy));

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] CreateMedicamentDTO dto)
            => Ok(new { Id = await _service.CreateAsync(dto) });

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateMedicamentDTO dto)
        {
            try
            {
                if (dto == null)
                {
                    return BadRequest(new { Success = false, Message = "DTO is null" });
                }
                
                if (id != dto.MedicamentID)
                {
                    return BadRequest(new { Success = false, Message = "ID din URL nu corespunde cu ID din DTO" });
                }

                var currentUser = _currentUserService.GetCurrentUserFullName();
                dto.UtilizatorActualizare = currentUser;
                
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
                _logger.LogError(ex, "Exception in Update");
                return StatusCode(500, new { Success = false, Message = $"Eroare interna: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
            => Ok(await _service.DeleteAsync(id) ? "Deleted" : "Not found");

        [HttpGet("test-auth")]
        [Authorize]
        public ActionResult TestAuth()
        {
            var currentUser = _currentUserService.GetCurrentUserFullName();
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
            var currentUser = _currentUserService.GetCurrentUserFullName();
            return Ok(new { 
                Id = id,
                Authenticated = User.Identity?.IsAuthenticated,
                UserName = User.Identity?.Name,
                CurrentUser = currentUser
            });
        }

        // GET api/medicamente/grouped
        [HttpGet("grouped")]
        public async Task<ActionResult<IEnumerable<MedicamentDTO>>> GetGrouped(
            [FromQuery] string? search, 
            [FromQuery] string? status, 
            [FromQuery] string? groupBy = null, 
            [FromQuery] string? sort = null)
        {
            try
            {
                var result = await _service.GetAllGroupedAsync(search, status, groupBy, sort);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la obtinerea medicamentelor grupate");
                return StatusCode(500, new { message = "Eroare interna la obtinerea medicamentelor" });
            }
        }
    }
}
