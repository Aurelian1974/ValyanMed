using Application.Services.Medical;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Medical;
using Shared.Common;
using Shared.Exceptions;

namespace API.Controllers.Medical;

[ApiController]
[Route("api/medical/[controller]")]
[Authorize]
public class PacientiController : ControllerBase
{
    private readonly IPacientService _pacientService;
    private readonly ILogger<PacientiController> _logger;

    public PacientiController(IPacientService pacientService, ILogger<PacientiController> logger)
    {
        _pacientService = pacientService;
        _logger = logger;
    }

    /// <summary>
    /// Obtine o lista paginata de pacienti cu optiuni de cautare si filtrare
    /// </summary>
    /// <param name="search">Termenul de cautare (nume, prenume, CNP, email)</param>
    /// <param name="judet">Filtru pentru judet</param>
    /// <param name="gen">Filtru pentru gen</param>
    /// <param name="page">Numarul paginii (implicit 1)</param>
    /// <param name="pageSize">Dimensiunea paginii (implicit 25)</param>
    /// <param name="sort">Criteriul de sortare (ex: NumeComplet:asc)</param>
    /// <returns>Lista paginata de pacienti</returns>
    [HttpGet]
    public async Task<IActionResult> GetPagedPatients(
        [FromQuery] string? search = null,
        [FromQuery] string? judet = null,
        [FromQuery] string? gen = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        [FromQuery] string? sort = null)
    {
        try
        {
            _logger.LogInformation("Getting paged patients - Search: {Search}, Judet: {Judet}, Gen: {Gen}, Page: {Page}, PageSize: {PageSize}", 
                search, judet, gen, page, pageSize);

            var searchQuery = new PacientiSearchQuery
            {
                Search = search,
                Judet = judet,
                Gen = gen,
                Page = page,
                PageSize = pageSize,
                Sort = sort
            };

            var result = await _pacientService.GetPagedAsync(searchQuery);
            
            if (!result.IsSuccess)
            {
                _logger.LogWarning("Failed to get paged patients: {Errors}", string.Join(", ", result.Errors));
                return BadRequest(new { errors = result.Errors });
            }

            _logger.LogInformation("Successfully retrieved {Count} patients out of {Total}", 
                result.Value?.Items?.Count() ?? 0, result.Value?.TotalCount ?? 0);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged patients");
            return StatusCode(500, new { message = "Eroare interna de server", details = ex.Message });
        }
    }

    /// <summary>
    /// Obtine un pacient dupa ID
    /// </summary>
    /// <param name="id">ID-ul pacientului</param>
    /// <returns>Datele pacientului</returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var result = await _pacientService.GetByIdAsync(id);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            if (result.Value == null)
            {
                return NotFound(new { message = "Pacientul nu a fost gasit" });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting patient by ID: {Id}", id);
            return StatusCode(500, new { message = "Eroare interna de server", details = ex.Message });
        }
    }

    /// <summary>
    /// Creeaza un pacient nou
    /// </summary>
    /// <param name="request">Datele pentru crearea pacientului</param>
    /// <returns>ID-ul pacientului creat</returns>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePacientRequest request)
    {
        try
        {
            var result = await _pacientService.CreateAsync(request);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return CreatedAtAction(nameof(GetById), new { id = result.Value }, 
                new { id = result.Value, message = result.SuccessMessage });
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
            _logger.LogError(ex, "Error creating patient");
            return StatusCode(500, new { message = "Eroare interna de server", details = ex.Message });
        }
    }

    /// <summary>
    /// Actualizeaza un pacient existent
    /// </summary>
    /// <param name="id">ID-ul pacientului</param>
    /// <param name="request">Datele actualizate</param>
    /// <returns>Rezultatul operatiunii</returns>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePacientRequest request)
    {
        try
        {
            if (id != request.PacientID)
            {
                return BadRequest(new { message = "ID-ul din URL nu corespunde cu cel din request" });
            }

            var result = await _pacientService.UpdateAsync(request);
            
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
            _logger.LogError(ex, "Error updating patient: {Id}", id);
            return StatusCode(500, new { message = "Eroare interna de server", details = ex.Message });
        }
    }

    /// <summary>
    /// Sterge un pacient
    /// </summary>
    /// <param name="id">ID-ul pacientului</param>
    /// <returns>Rezultatul operatiunii</returns>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _pacientService.DeleteAsync(id);
            
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
            _logger.LogError(ex, "Error deleting patient: {Id}", id);
            return StatusCode(500, new { message = "Eroare interna de server", details = ex.Message });
        }
    }

    /// <summary>
    /// Cauta pacienti pentru autocomplete
    /// </summary>
    /// <param name="term">Termenul de cautare</param>
    /// <returns>Lista de pacienti gasiti</returns>
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string term)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(term))
                return Ok(new List<PacientListDto>());

            var searchQuery = new PacientiSearchQuery
            {
                Search = term,
                PageSize = 10 // Limitam la 10 pentru autocomplete
            };

            var result = await _pacientService.GetPagedAsync(searchQuery);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            return Ok(result.Value?.Items ?? new List<PacientListDto>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching patients with term: {Term}", term);
            return StatusCode(500, new { message = "Eroare interna de server", details = ex.Message });
        }
    }
}