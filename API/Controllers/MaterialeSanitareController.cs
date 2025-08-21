using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Shared.DTOs;
using Application.Services;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialeSanitareController : ControllerBase
    {
        private readonly IMaterialSanitarService _service;
        private readonly ILogger<MaterialeSanitareController> _logger;

        public MaterialeSanitareController(IMaterialSanitarService service, ILogger<MaterialeSanitareController> logger)
        {
            _service = service;
            _logger = logger;
        }

        // GET api/materialeSanitare
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MaterialSanitarDTO>>> Get()
        {
            try
            {
                var materiale = await _service.GetAllAsync();
                return Ok(materiale);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la obtinerea materialelor sanitare");
                return StatusCode(500, new { message = "Eroare interna la obtinerea materialelor sanitare" });
            }
        }

        // GET api/materialeSanitare/paged
        [HttpGet("paged")]
        public async Task<ActionResult<PagedResult<MaterialSanitarDTO>>> GetPaged(
            [FromQuery] string? search, 
            [FromQuery] string? categorie, 
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 25, 
            [FromQuery] string? sort = null)
        {
            try
            {
                var result = await _service.GetPagedAsync(search, categorie, page, pageSize, sort);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la obtinerea materialelor sanitare paginate");
                return StatusCode(500, new { message = "Eroare interna la obtinerea materialelor sanitare" });
            }
        }

        // GET api/materialeSanitare/grouped
        [HttpGet("grouped")]
        public async Task<ActionResult<IEnumerable<MaterialSanitarDTO>>> GetGrouped(
            [FromQuery] string? search, 
            [FromQuery] string? categorie, 
            [FromQuery] string? groupBy = null, 
            [FromQuery] string? sort = null)
        {
            try
            {
                var result = await _service.GetAllGroupedAsync(search, categorie, groupBy, sort);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la obtinerea materialelor sanitare grupate");
                return StatusCode(500, new { message = "Eroare interna la obtinerea materialelor sanitare" });
            }
        }

        // GET api/materialeSanitare/paged-grouped
        [HttpGet("paged-grouped")]
        public async Task<ActionResult<PagedResult<MaterialSanitarDTO>>> GetPagedGrouped(
            [FromQuery] string? search, 
            [FromQuery] string? categorie, 
            [FromQuery] string? groupBy = null, 
            [FromQuery] string? sort = null,
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 25)
        {
            try
            {
                var result = await _service.GetPagedGroupedAsync(search, categorie, groupBy, sort, page, pageSize);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la obtinerea materialelor sanitare paginate si grupate");
                return StatusCode(500, new { message = "Eroare interna la obtinerea materialelor sanitare" });
            }
        }

        // GET api/materialeSanitare/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MaterialSanitarDTO>> GetById(int id)
        {
            try
            {
                var material = await _service.GetByIdAsync(id);
                if (material == null)
                    return NotFound(new { message = $"Materialul sanitar cu ID {id} nu a fost gasit" });

                return Ok(material);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la obtinerea materialului sanitar cu ID {Id}", id);
                return StatusCode(500, new { message = "Eroare interna la obtinerea materialului sanitar" });
            }
        }

        // POST api/materialeSanitare
        [HttpPost]
        [Authorize]
        public async Task<ActionResult> Create([FromBody] CreateMaterialSanitarDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Datele materialului sanitar sunt obligatorii" });

                var id = await _service.CreateAsync(dto);
                return Ok(new { Id = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la crearea materialului sanitar");
                return StatusCode(500, new { message = "Eroare interna la crearea materialului sanitar" });
            }
        }

        // PUT api/materialeSanitare/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult> Update(int id, [FromBody] UpdateMaterialSanitarDTO dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest(new { message = "Datele materialului sanitar sunt obligatorii" });

                if (id != dto.Id)
                    return BadRequest(new { message = "ID-ul din URL nu corespunde cu ID-ul din corpul cererii" });

                var success = await _service.UpdateAsync(dto);
                if (success)
                    return Ok(new { message = "Material sanitar actualizat cu succes" });
                else
                    return NotFound(new { message = "Materialul sanitar nu a fost gasit" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la actualizarea materialului sanitar cu ID {Id}", id);
                return StatusCode(500, new { message = "Eroare interna la actualizarea materialului sanitar" });
            }
        }

        // DELETE api/materialeSanitare/{id}
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<ActionResult> Delete(int id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);
                if (success)
                    return Ok(new { message = "Material sanitar sters cu succes" });
                else
                    return NotFound(new { message = "Materialul sanitar nu a fost gasit" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Eroare la stergerea materialului sanitar cu ID {Id}", id);
                return StatusCode(500, new { message = "Eroare interna la stergerea materialului sanitar" });
            }
        }
    }
}