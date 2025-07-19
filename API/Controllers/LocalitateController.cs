namespace API.Controllers
{
    // Server/Controllers/LocalitateController.cs
    using Microsoft.AspNetCore.Mvc;
    using Application.Services;
    using Shared.DTOs;

    [ApiController]
    [Route("api/[controller]")]
    public class LocalitateController : ControllerBase
    {
        private readonly ILocalitateService _service;

        public LocalitateController(ILocalitateService service)
        {
            _service = service;
        }

        [HttpGet("by-judet/{idJudet}")]
        public async Task<ActionResult<IEnumerable<LocalitateDto>>> GetByJudet(int idJudet)
        {
            var localitati = await _service.GetByJudetAsync(idJudet);
            return Ok(localitati);
        }
    }
}
