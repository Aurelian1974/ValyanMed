using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Services;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersoaneController : ControllerBase
    {
        private readonly IPersoanaService _persoanaService;
        private readonly ILogger<PersoaneController> _logger;

        public PersoaneController(IPersoanaService persoanaService, ILogger<PersoaneController> logger)
        {
            _persoanaService = persoanaService ?? throw new ArgumentNullException(nameof(persoanaService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PersoanaDTO>>> GetAll()
        {
            try
            {
                var persoane = await _persoanaService.GetAllPersonalAsync();
                return Ok(persoane);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving persoane data");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error retrieving persoane data: {ex.Message}");
            }
        }
    }
}
