using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Shared.DTOs;
using Application.Services;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DispozitiveMedicaleController : ControllerBase
    {
        private readonly IDispozitivMedicalService _service;
        private readonly ILogger<DispozitiveMedicaleController> _logger;

        public DispozitiveMedicaleController(IDispozitivMedicalService service, ILogger<DispozitiveMedicaleController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET api/dispozitiveMedicale
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DispozitivMedicalDTO>>> Get()
        {
            try
            {
                var dispozitive = await _service.GetAllAsync();
                return Ok(dispozitive);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la obtinerea dispozitivelor medicale");
                return StatusCode(500, new { message = "Eroare interna la obtinerea dispozitivelor medicale" });
            }
        }

        // GET api/dispozitiveMedicale/paged
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<DispozitivMedicalDTO>>> GetPaged(
            [FromQuery] string? search, 
            [FromQuery] string? categorie, 
            [FromQuery] string? clasaRisc,
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 25, 
            [FromQuery] string? sort = null)
        {
            try
            {
                var result = await _service.GetPagedAsync(search, categorie, clasaRisc, page, pageSize, sort);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la obtinerea dispozitivelor medicale paginate");
                return StatusCode(500, new { message = "Eroare interna la obtinerea dispozitivelor medicale" });
            }
        }

        // GET api/dispozitiveMedicale/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<DispozitivMedicalDTO>> GetById(int id)
        {
            try
            {
                var dispozitiv = await _service.GetByIdAsync(id);
                if (dispozitiv == null)
                    return NotFound(new { message = $"Dispozitivul medical cu ID {id} nu a fost gasit" });

                return Ok(dispozitiv);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la obtinerea dispozitivului medical cu ID {Id}", id);
                return StatusCode(500, new { message = "Eroare interna la obtinerea dispozitivului medical" });
            }
        }

        // POST api/dispozitiveMedicale
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] CreateDispozitivMedicalDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Datele dispozitivului medical sunt obligatorii" });

                var id = await _service.CreateAsync(dto);
                return Ok(new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la crearea dispozitivului medical");
                return StatusCode(500, new { message = "Eroare interna la crearea dispozitivului medical" });
            }
        }

        // PUT api/dispozitiveMedicale/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateDispozitivMedicalDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Datele dispozitivului medical sunt obligatorii" });

                if (id != dto.Id)
                    return BadRequest(new { message = "ID-ul din URL nu corespunde cu ID-ul din corpul cererii" });

                var success = await _service.UpdateAsync(dto);
                if (success)
                    return Ok(new { message = "Dispozitiv medical actualizat cu succes" });
                else
                    return NotFound(new { message = "Dispozitivul medical nu a fost gasit" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la actualizarea dispozitivului medical cu ID {Id}", id);
                return StatusCode(500, new { message = "Eroare interna la actualizarea dispozitivului medical" });
            }
        }

        // DELETE api/dispozitiveMedicale/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);
                if (success)
                    return Ok(new { message = "Dispozitiv medical sters cu succes" });
                else
                    return NotFound(new { message = "Dispozitivul medical nu a fost gasit" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la stergerea dispozitivului medical cu ID {Id}", id);
                return StatusCode(500, new { message = "Eroare interna la stergerea dispozitivului medical" });
            }
        }

        // GET api/dispozitiveMedicale/grouped
        [HttpGet("grouped")]
        public async Task<ActionResult<IEnumerable<DispozitivMedicalDTO>>> GetGrouped(
            [FromQuery] string? search, 
            [FromQuery] string? categorie, 
            [FromQuery] string? clasaRisc,
            [FromQuery] string? groupBy = null, 
            [FromQuery] string? sort = null)
        {
            try
            {
                var result = await _service.GetAllGroupedAsync(search, categorie, clasaRisc, groupBy, sort);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la obtinerea dispozitivelor medicale grupate");
                return StatusCode(500, new { message = "Eroare interna la obtinerea dispozitivelor medicale" });
            }
        }

        // GET api/dispozitiveMedicale/paged-grouped
        [HttpGet("paged-grouped")]
        public async Task<ActionResult<PagedResult<DispozitivMedicalDTO>>> GetPagedGrouped(
            [FromQuery] string? search, 
            [FromQuery] string? categorie, 
            [FromQuery] string? clasaRisc,
            [FromQuery] string? groupBy = null, 
            [FromQuery] string? sort = null,
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 25)
        {
            try
            {
                var result = await _service.GetPagedGroupedAsync(search, categorie, clasaRisc, groupBy, sort, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la obtinerea dispozitivelor medicale paginate si grupate");
                return StatusCode(500, new { message = "Eroare interna la obtinerea dispozitivelor medicale" });
            }
        }
    }
}