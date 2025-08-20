using Microsoft.AspNetCore.Mvc;
using Application.Services;
using Shared.DTOs;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicamenteController : ControllerBase
    {
        private readonly IMedicamentService _service;
        public MedicamenteController(IMedicamentService service) => _service = service;

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
        public async Task<ActionResult<int>> Create([FromBody] CreateMedicamentDTO dto)
        {
            var id = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut]
        public async Task<ActionResult> Update([FromBody] UpdateMedicamentDTO dto)
        {
            var ok = await _service.UpdateAsync(dto);
            return ok ? Ok() : NotFound();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var ok = await _service.DeleteAsync(id);
            return ok ? NoContent() : NotFound();
        }
    }
}
