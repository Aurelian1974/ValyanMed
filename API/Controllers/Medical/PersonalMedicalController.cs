using Application.Services.Medical;
using Microsoft.AspNetCore.Mvc;
using Shared.Common;
using Shared.DTOs.Medical;
using FluentValidation;

namespace API.Controllers.Medical;

[ApiController]
[Route("api/[controller]")]
public class PersonalMedicalController : ControllerBase
{
    private readonly IPersonalMedicalService _personalMedicalService;
    private readonly ILogger<PersonalMedicalController> _logger;

    public PersonalMedicalController(
        IPersonalMedicalService personalMedicalService,
        ILogger<PersonalMedicalController> logger)
    {
        _personalMedicalService = personalMedicalService;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint pentru DataGrid cu suport pentru grupare server-side
    /// </summary>
    [HttpPost("datagrid")]
    public async Task<ActionResult<PersonalMedicalDataGridResult>> GetDataGrid([FromBody] PersonalMedicalDataGridRequest request)
    {
        try
        {
            _logger.LogInformation("GetDataGrid called with grouping: {HasGroups}, Groups: {Groups}", 
                request.Groups?.Any() == true, 
                request.Groups?.Any() == true ? string.Join(", ", request.Groups.Select(g => $"{g.Property}:{g.SortOrder}")) : "none");

            var result = await _personalMedicalService.GetDataGridAsync(request);
            
            if (result.IsSuccess)
            {
                var gridResult = result.Value!;
                _logger.LogInformation("GetDataGrid successful: IsGrouped={IsGrouped}, Count={Count}, Groups={GroupCount}", 
                    gridResult.IsGrouped, gridResult.Count, gridResult.Groups?.Count() ?? 0);
                return Ok(gridResult);
            }

            _logger.LogWarning("Failed to get DataGrid data: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error in GetDataGrid: {Errors}", string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetDataGrid");
            return StatusCode(500, new { message = "Eroare interna la obtinerea datelor pentru DataGrid" });
        }
    }

    /// <summary>
    /// Ob?ine lista paginat? de personal medical cu filtrare
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<PagedResult<PersonalMedicalListDto>>> GetPaged([FromQuery] PersonalMedicalSearchQuery query)
    {
        try
        {
            // Debug: Log incoming query parameters
            _logger.LogInformation("GetPaged called with: Search={Search}, Departament={Departament}, Pozitie={Pozitie}, EsteActiv={EsteActiv}, Page={Page}, PageSize={PageSize}, Sort={Sort}", 
                query.Search, query.Departament, query.Pozitie, query.EsteActiv, query.Page, query.PageSize, query.Sort);

            var result = await _personalMedicalService.GetPagedAsync(query);
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("GetPaged successful: {Count} items returned, total: {Total}", 
                    result.Value?.Items?.Count() ?? 0, result.Value?.TotalCount ?? 0);
                return Ok(result.Value);
            }

            _logger.LogWarning("Failed to get paged personal medical: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error getting paged personal medical: {Errors}", string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged personal medical");
            return StatusCode(500, new { message = "Eroare interna la obtinerea listei de personal medical" });
        }
    }

    /// <summary>
    /// Ob?ine lista agregat? de personal medical (num?, data ultimului acces) pentru grupuri
    /// </summary>
    [HttpGet("group-aggregates")]
    public async Task<ActionResult<Result<IEnumerable<PersonalMedicalGroupAggregateDto>>>> GetGroupAggregates([FromQuery] PersonalMedicalGroupAggregateQuery query)
    {
        try
        {
            var result = await _personalMedicalService.GetGroupAggregatesAsync(query);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            _logger.LogWarning("Failed to get group aggregates for personal medical: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting group aggregates for personal medical");
            return StatusCode(500, Result<IEnumerable<PersonalMedicalGroupAggregateDto>>.Failure("Eroare interna la ob?inerea agregatelor de grup"));
        }
    }

    /// <summary>
    /// Ob?ine detaliile unui personal medical dup? ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PersonalMedicalDetailDto>> GetById(Guid id)
    {
        try
        {
            var result = await _personalMedicalService.GetByIdAsync(id);
            
            if (result.IsSuccess)
            {
                if (result.Value == null)
                {
                    return NotFound(new { message = "Personal medical nu a fost gasit" });
                }

                return Ok(result.Value);
            }

            _logger.LogWarning("Failed to get personal medical by id {Id}: {Errors}", id, string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personal medical by id {Id}", id);
            return StatusCode(500, new { message = "Eroare interna la obtinerea detaliilor personalului medical" });
        }
    }

    /// <summary>
    /// Creeaz? un nou personal medical
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Result<Guid>>> Create([FromBody] CreatePersonalMedicalRequest request)
    {
        try
        {
            _logger.LogInformation("Create PersonalMedical called with request: {@Request}", request);
            
            var result = await _personalMedicalService.CreateAsync(request);
            
            _logger.LogInformation("PersonalMedical create result: {@Result}", new {
                IsSuccess = result.IsSuccess,
                Value = result.Value,
                ErrorsCount = result.Errors?.Count ?? 0,
                Errors = result.Errors
            });
            
            if (result.IsSuccess)
            {
                _logger.LogInformation("PersonalMedical created successfully with ID: {PersonalId}", result.Value);
                
                // Test serializare înainte de a returna
                try
                {
                    var serialized = System.Text.Json.JsonSerializer.Serialize(result, new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() }
                    });
                    _logger.LogInformation("Serialization test successful: {SerializedResult}", serialized);
                }
                catch (Exception serEx)
                {
                    _logger.LogError(serEx, "Serialization test failed");
                }
                
                return CreatedAtAction(nameof(GetById), new { id = result.Value }, result);
            }

            _logger.LogWarning("Failed to create personal medical: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error creating personal medical: {Errors}", string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating personal medical");
            return StatusCode(500, new { message = "Eroare interna la crearea personalului medical" });
        }
    }

    /// <summary>
    /// Actualizeaz? un personal medical existent
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Result>> Update(Guid id, [FromBody] UpdatePersonalMedicalRequest request)
    {
        try
        {
            if (id != request.PersonalID)
            {
                return BadRequest(new { errors = new[] { "ID-ul din URL nu corespunde cu ID-ul din request" } });
            }

            var result = await _personalMedicalService.UpdateAsync(request);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            _logger.LogWarning("Failed to update personal medical {Id}: {Errors}", id, string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Validation error updating personal medical {Id}: {Errors}", id, string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
            return BadRequest(new { errors = ex.Errors.Select(e => e.ErrorMessage) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating personal medical {Id}", id);
            return StatusCode(500, new { message = "Eroare interna la actualizarea personalului medical" });
        }
    }

    /// <summary>
    /// ?terge (dezactiveaz?) un personal medical
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Result>> Delete(Guid id)
    {
        try
        {
            var result = await _personalMedicalService.DeleteAsync(id);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            if (result.Errors.Any(e => e.Contains("nu a fost gasit")))
            {
                return NotFound(new { errors = result.Errors });
            }

            if (result.Errors.Any(e => e.Contains("programari active")))
            {
                return Conflict(new { errors = result.Errors });
            }

            _logger.LogWarning("Failed to delete personal medical {Id}: {Errors}", id, string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting personal medical {Id}", id);
            return StatusCode(500, new { message = "Eroare interna la stergerea personalului medical" });
        }
    }

    /// <summary>
    /// Ob?ine lista doctorilor activi pentru dropdown-uri
    /// </summary>
    [HttpGet("active-doctors")]
    public async Task<ActionResult<Result<IEnumerable<PersonalMedicalListDto>>>> GetActiveDoctors()
    {
        try
        {
            var result = await _personalMedicalService.GetActiveDoctorsAsync();
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            _logger.LogWarning("Failed to get active doctors: {Errors}", string.Join(", ", result.Errors));
            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active doctors");
            return StatusCode(500, Result<IEnumerable<PersonalMedicalListDto>>.Failure("Eroare interna la obtinerea listei de doctori activi"));
        }
    }

    /// <summary>
    /// Schimb? statusul unui personal medical (activ/inactiv)
    /// </summary>
    [HttpPatch("{id:guid}/toggle-status")]
    public async Task<ActionResult<Result>> ToggleStatus(Guid id)
    {
        try
        {
            // Ob?ine personalul curent pentru a-i schimba statusul
            var getResult = await _personalMedicalService.GetByIdAsync(id);
            
            if (!getResult.IsSuccess)
            {
                return BadRequest(new { errors = getResult.Errors });
            }

            if (getResult.Value == null)
            {
                return NotFound(new { message = "Personal medical nu a fost gasit" });
            }

            var updateRequest = new UpdatePersonalMedicalRequest
            {
                PersonalID = getResult.Value.PersonalID,
                Nume = getResult.Value.Nume,
                Prenume = getResult.Value.Prenume,
                Pozitie = getResult.Value.Pozitie,
                Specializare = getResult.Value.Specializare,
                Departament = getResult.Value.Departament,
                NumarLicenta = getResult.Value.NumarLicenta,
                Telefon = getResult.Value.Telefon,
                Email = getResult.Value.Email,
                EsteActiv = !getResult.Value.EsteActiv // Toggle status
            };

            var result = await _personalMedicalService.UpdateAsync(updateRequest);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            _logger.LogWarning("Failed to toggle status for personal medical {Id}: {Errors}", id, string.Join(", ", result.Errors));
            return BadRequest(new { errors = result.Errors });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling status for personal medical {Id}", id);
            return StatusCode(500, new { message = "Eroare interna la schimbarea statusului personalului medical" });
        }
    }
}