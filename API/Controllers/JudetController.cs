using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;

[ApiController]
[Route("api/[controller]")]
public class JudetController : ControllerBase
{
    private readonly IJudetService _service;
    public JudetController(IJudetService service) => _service = service;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JudetDto>>> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }
}