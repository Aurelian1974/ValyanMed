using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Medical;
using Application.Services.Medical;

namespace API.Controllers.Medical;

[ApiController]
[Route("api/medical/judete")]
public class JudeteController : ControllerBase
{
    private readonly IJudetService _judetService;
    public JudeteController(IJudetService judetService)
    {
        _judetService = judetService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<JudetDto>>> GetAll()
    {
        var judete = await _judetService.GetAllAsync();
        return Ok(judete);
    }
}
