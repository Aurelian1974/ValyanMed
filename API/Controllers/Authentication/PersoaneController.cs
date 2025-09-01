using Application.Services.Authentication;
using Microsoft.AspNetCore.Mvc;
using Shared.Common;
using Shared.DTOs.Authentication;
using Shared.Exceptions;

namespace API.Controllers.Authentication;

[ApiController]
[Route("api/[controller]")]
public class PersoaneController : ControllerBase
{
    private readonly IPersoanaService _persoanaService;
    private readonly ILogger _logger;

    public PersoaneController(IPersoanaService persoanaService, ILogger<PersoaneController> logger)
    {
        _persoanaService = persoanaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtine lista paginata de persoane cu filtrare
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<PersoanaListDto>>> GetPaged([FromQuery] PersoanaSearchQuery query)
    {
        try
        {
            // Debug: Log incoming query parameters
            _logger.LogInformation("GetPaged called with: Search={Search}, Judet={Judet}, Localitate={Localitate}, EsteActiv={EsteActiv}, Page={Page}, PageSize={PageSize}, Sort={Sort}", 
                query.Search, query.Judet, query.Localitate, query.EsteActiv, query.Page, query.PageSize, query.Sort);

            // Default values if not provided
            query.Page = query.Page <= 0 ? 1 : query.Page;
            query.PageSize = query.PageSize <= 0 ? 10 : query.PageSize;
            query.PageSize = query.PageSize > 100 ? 100 : query.PageSize; // Max page size

            var result = await _persoanaService.GetPagedAsync(query);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("GetPaged successful: {Count} items returned, total: {Total}", 
                    result.Value?.Items?.Count() ?? 0, result.Value?.TotalCount ?? 0);
                return Ok(result.Value);
            }

            _logger.LogWarning("Failed to get paged persoane: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged persoane");
            return StatusCode(500, new { message = "Eroare interna la obtinerea listei de persoane" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _persoanaService.GetByIdForDisplayAsync(id);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            if (result.Value == null)
            {
                return NotFound(new { message = "Persoana nu a fost gasita" });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting persoana by id {Id}", id);
            return StatusCode(500, new { message = "Eroare interna de server", details = ex.Message });
        }
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _persoanaService.GetAllAsync();
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all persoane");
            return StatusCode(500, new { message = "Eroare interna de server", details = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePersoanaRequest request)
    {
        try
        {
            var result = await _persoanaService.CreateAsync(request);
            
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
            _logger.LogError(ex, "Error creating persoana");
            return StatusCode(500, new { message = "Eroare interna de server", details = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePersoanaRequest request)
    {
        try
        {
            if (id != request.Id)
            {
                return BadRequest(new { message = "ID-ul din URL nu corespunde cu cel din request" });
            }

            var result = await _persoanaService.UpdateAsync(request);
            
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
            _logger.LogError(ex, "Error updating persoana {Id}", id);
            return StatusCode(500, new { message = "Eroare interna de server", details = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _persoanaService.DeleteAsync(id);
            
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
            _logger.LogError(ex, "Error deleting persoana {Id}", id);
            return StatusCode(500, new { message = "Eroare interna de server", details = ex.Message });
        }
    }
}