using Microsoft.AspNetCore.Mvc;
using Application.Services.Common;
using Shared.DTOs.Common;

namespace API.Controllers.Common;

[ApiController]
[Route("api/[controller]")]
public class LocationController : ControllerBase
{
    private readonly ILocationService _locationService;
    private readonly ILogger<LocationController> _logger;

    public LocationController(ILocationService locationService, ILogger<LocationController> logger)
    {
        _locationService = locationService;
        _logger = logger;
    }

    /// <summary>
    /// Get all counties (judete)
    /// </summary>
    [HttpGet("judete")]
    public async Task<ActionResult<List<JudetDto>>> GetJudete()
    {
        try
        {
            var judete = await _locationService.GetJudeteAsync();
            return Ok(judete);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving counties");
            return StatusCode(500, new { message = "Eroare la obtinerea judetelor" });
        }
    }

    /// <summary>
    /// Get all localities (localitati)
    /// </summary>
    [HttpGet("localitati")]
    public async Task<ActionResult<List<LocalitateDto>>> GetLocalitati()
    {
        try
        {
            var localitati = await _locationService.GetLocalitatiAsync();
            return Ok(localitati);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localities");
            return StatusCode(500, new { message = "Eroare la obtinerea localitatilor" });
        }
    }

    /// <summary>
    /// Get localities by county name
    /// </summary>
    [HttpGet("localitati/judet/{judetNume}")]
    public async Task<ActionResult<List<LocalitateDto>>> GetLocalitatiByJudet(string judetNume)
    {
        try
        {
            var localitati = await _locationService.GetLocalitatiByJudetAsync(judetNume);
            return Ok(localitati);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localities for county {County}", judetNume);
            return StatusCode(500, new { message = "Eroare la obtinerea localitatilor pentru judet" });
        }
    }

    /// <summary>
    /// Get localities by county ID
    /// </summary>
    [HttpGet("localitati/judet-id/{judetId:int}")]
    public async Task<ActionResult<List<LocalitateDto>>> GetLocalitatiByJudetId(int judetId)
    {
        try
        {
            var localitati = await _locationService.GetLocalitatiByJudetIdAsync(judetId);
            return Ok(localitati);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving localities for county ID {CountyId}", judetId);
            return StatusCode(500, new { message = "Eroare la obtinerea localitatilor pentru judet" });
        }
    }

    /// <summary>
    /// Get all counties with their localities
    /// </summary>
    [HttpGet("judete-cu-localitati")]
    public async Task<ActionResult<List<JudetWithLocalitatiDto>>> GetJudeteWithLocalitati()
    {
        try
        {
            var data = await _locationService.GetJudeteWithLocalitatiAsync();
            return Ok(data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving counties with localities");
            return StatusCode(500, new { message = "Eroare la obtinerea judetelor cu localitati" });
        }
    }
}