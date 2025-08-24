using Application.Services.Authentication;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Authentication;
using Shared.Exceptions;

namespace API.Controllers.Authentication;

[ApiController]
[Route("api/[controller]")]
public class PersoaneController : ControllerBase
{
    private readonly IPersoanaService _persoanaService;

    public PersoaneController(IPersoanaService persoanaService)
    {
        _persoanaService = persoanaService;
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
            return StatusCode(500, new { message = "Eroare intern? de server", details = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _persoanaService.GetByIdAsync(id);
            
            if (!result.IsSuccess)
            {
                return BadRequest(new { errors = result.Errors });
            }

            if (result.Value == null)
            {
                return NotFound(new { message = "Persoana nu a fost g?sit?" });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Eroare intern? de server", details = ex.Message });
        }
    }

    [HttpGet]
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
            return StatusCode(500, new { message = "Eroare intern? de server", details = ex.Message });
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
            return StatusCode(500, new { message = "Eroare intern? de server", details = ex.Message });
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
            return StatusCode(500, new { message = "Eroare intern? de server", details = ex.Message });
        }
    }
}