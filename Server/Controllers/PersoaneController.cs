[ApiController]
[Route("api/[controller]")]
public class PersoaneController : ControllerBase
{
    private readonly IPersoaneRepository _persoaneRepository;

    public PersoaneController(IPersoaneRepository persoaneRepository)
    {
        _persoaneRepository = persoaneRepository;
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<PersoanaDto>>> Search([FromQuery] string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return new List<PersoanaDto>();

        var persons = await _persoaneRepository.SearchAsync(term);
        return Ok(persons);
    }
}