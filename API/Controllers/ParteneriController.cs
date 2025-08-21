using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Shared.DTOs;
using Application.Services;
using API.Services;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParteneriController : ControllerBase
    {
        private readonly IPartenerService _service;
        private readonly ILogger<ParteneriController> _logger;
        private readonly ICurrentUserService _currentUserService;

        public ParteneriController(IPartenerService service, ILogger<ParteneriController> logger, ICurrentUserService currentUserService)
        {
            _service = service;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        // GET api/parteneri
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PartenerDTO>>> Get()
        {
            try
            {
                var parteneri = await _service.GetAllAsync();
                return Ok(parteneri);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la obtinerea partenerilor");
                return StatusCode(500, new { message = "Eroare interna la obtinerea partenerilor" });
            }
        }

        // GET api/parteneri/paged
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<PartenerDTO>>> GetPaged(
            [FromQuery] string? search, 
            [FromQuery] string? judet, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 25, 
            [FromQuery] string? sort = null)
        {
            try
            {
                var result = await _service.GetPagedAsync(search, judet, page, pageSize, sort);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la obtinerea partenerilor paginati");
                return StatusCode(500, new { message = "Eroare interna la obtinerea partenerilor" });
            }
        }

        // GET api/parteneri/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PartenerDTO>> GetById(int id)
        {
            try
            {
                var partener = await _service.GetByIdAsync(id);
                if (partener == null)
                    return NotFound(new { message = $"Partenerul cu ID {id} nu a fost gasit" });

                return Ok(partener);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la obtinerea partenerului cu ID {Id}", id);
                return StatusCode(500, new { message = "Eroare interna la obtinerea partenerului" });
            }
        }

        // POST api/parteneri
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] CreatePartenerDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Datele partenerului sunt obligatorii" });

                // Set current user as creator
                var currentUser = _currentUserService.GetCurrentUserFullName();
                dto.UtilizatorCreare = currentUser;

                var id = await _service.CreateAsync(dto);
                return Ok(new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la crearea partenerului");
                return StatusCode(500, new { message = "Eroare interna la crearea partenerului" });
            }
        }

        // PUT api/parteneri/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Update(int id, [FromBody] UpdatePartenerDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Datele partenerului sunt obligatorii" });

                if (id != dto.PartenerId)
                    return BadRequest(new { message = "ID-ul din URL nu corespunde cu ID-ul din corpul cererii" });

                // Set current user as updater
                var currentUser = _currentUserService.GetCurrentUserFullName();
                dto.UtilizatorActualizare = currentUser;

                var success = await _service.UpdateAsync(dto);
                if (success)
                    return Ok(new { message = "Partener actualizat cu succes" });
                else
                    return NotFound(new { message = "Partenerul nu a fost gasit" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la actualizarea partenerului cu ID {Id}", id);
                return StatusCode(500, new { message = "Eroare interna la actualizarea partenerului" });
            }
        }

        // DELETE api/parteneri/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);
                if (success)
                    return Ok(new { message = "Partener sters cu succes" });
                else
                    return NotFound(new { message = "Partenerul nu a fost gasit" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la stergerea partenerului cu ID {Id}", id);
                return StatusCode(500, new { message = "Eroare interna la stergerea partenerului" });
            }
        }
    }
}