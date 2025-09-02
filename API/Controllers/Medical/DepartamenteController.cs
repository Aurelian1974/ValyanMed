using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Medical;
using Shared.Common;
using Dapper;
using Infrastructure.Data;

namespace API.Controllers.Medical;

[ApiController]
[Route("api/[controller]")]
public class DepartamenteController : ControllerBase
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<DepartamenteController> _logger;

    public DepartamenteController(
        ISqlConnectionFactory connectionFactory,
        ILogger<DepartamenteController> logger)
    {
        _connectionFactory = connectionFactory;
        _logger = logger;
    }

    [HttpGet("categorii")]
    public async Task<ActionResult<Result<IEnumerable<DepartamentOptionDto>>>> GetCategorii()
    {
        try
        {
            _logger.LogDebug("Getting categorii departamente");

            using var connection = _connectionFactory.CreateConnection();
            
            var categorii = await connection.QueryAsync<DepartamentDto>(
                "EXEC sp_Departamente_GetCategorii");

            var options = categorii.Select(c => new DepartamentOptionDto
            {
                Value = c.DepartamentID,
                Text = c.Nume,
                Tip = c.Tip,
                EsteActiv = true
            });

            return Ok(Result<IEnumerable<DepartamentOptionDto>>.Success(options));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categorii departamente");
            return StatusCode(500, Result<IEnumerable<DepartamentOptionDto>>.Failure("Eroare la înc?rcarea categoriilor"));
        }
    }

    [HttpGet("specializari/{categorieId:guid}")]
    public async Task<ActionResult<Result<IEnumerable<DepartamentOptionDto>>>> GetSpecializariByCategorie(Guid categorieId)
    {
        try
        {
            _logger.LogDebug("Getting specializari for categorie {CategorieId}", categorieId);

            using var connection = _connectionFactory.CreateConnection();
            
            var specializari = await connection.QueryAsync<DepartamentDto>(
                "EXEC sp_Departamente_GetSpecializariByCategorie @CategorieID",
                new { CategorieID = categorieId });

            var options = specializari.Select(s => new DepartamentOptionDto
            {
                Value = s.DepartamentID,
                Text = s.Nume,
                Tip = s.Tip,
                EsteActiv = true
            });

            return Ok(Result<IEnumerable<DepartamentOptionDto>>.Success(options));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting specializari for categorie {CategorieId}", categorieId);
            return StatusCode(500, Result<IEnumerable<DepartamentOptionDto>>.Failure("Eroare la înc?rcarea specializ?rilor"));
        }
    }

    [HttpGet("subspecializari/{specializareId:guid}")]
    public async Task<ActionResult<Result<IEnumerable<DepartamentOptionDto>>>> GetSubspecializariBySpecializare(Guid specializareId)
    {
        try
        {
            _logger.LogDebug("Getting subspecializari for specializare {SpecializareId}", specializareId);

            using var connection = _connectionFactory.CreateConnection();
            
            var subspecializari = await connection.QueryAsync<DepartamentDto>(
                "EXEC sp_Departamente_GetSubspecializariBySpecializare @SpecializareID",
                new { SpecializareID = specializareId });

            var options = subspecializari.Select(s => new DepartamentOptionDto
            {
                Value = s.DepartamentID,
                Text = s.Nume,
                Tip = s.Tip,
                EsteActiv = true
            });

            return Ok(Result<IEnumerable<DepartamentOptionDto>>.Success(options));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting subspecializari for specializare {SpecializareId}", specializareId);
            return StatusCode(500, Result<IEnumerable<DepartamentOptionDto>>.Failure("Eroare la înc?rcarea subspecializ?rilor"));
        }
    }

    [HttpGet("{departamentId:guid}/ierarhic")]
    public async Task<ActionResult<Result<DepartamentIerarhicDto?>>> GetDepartamentIerarhic(Guid departamentId)
    {
        try
        {
            _logger.LogDebug("Getting departament ierarhic for {DepartamentId}", departamentId);

            using var connection = _connectionFactory.CreateConnection();
            
            // Ob?ine datele de baz? ale departamentului
            var departament = await connection.QuerySingleOrDefaultAsync<DepartamentDto>(
                @"SELECT DepartamentID, Nume, Tip 
                  FROM Departamente 
                  WHERE DepartamentID = @DepartamentID",
                new { DepartamentID = departamentId });

            if (departament == null)
            {
                return Ok(Result<DepartamentIerarhicDto?>.Success(null));
            }

            // Ob?ine calea complet?
            var calieCompleta = await connection.QuerySingleOrDefaultAsync<string>(
                "EXEC sp_GetStramosi @NumeDepartament = @Nume, @TipDepartament = @Tip",
                new { Nume = departament.Nume, Tip = departament.Tip });

            // Ob?ine descenden?ii
            var descendenti = await connection.QueryAsync<DepartamentDto>(
                "EXEC sp_GetDescendenti @NumeDepartament = @Nume, @TipDepartament = @Tip",
                new { Nume = departament.Nume, Tip = departament.Tip });

            var ierarhic = new DepartamentIerarhicDto
            {
                DepartamentID = departament.DepartamentID,
                Nume = departament.Nume,
                Tip = departament.Tip,
                CalieCompleta = calieCompleta ?? departament.Nume,
                Copii = descendenti.Select(d => new DepartamentIerarhicDto
                {
                    DepartamentID = d.DepartamentID,
                    Nume = d.Nume,
                    Tip = d.Tip,
                    ParenteID = departamentId,
                    Nivel = 1,
                    CalieCompleta = $"{departament.Nume} > {d.Nume}",
                    Copii = new List<DepartamentIerarhicDto>()
                })
            };

            return Ok(Result<DepartamentIerarhicDto?>.Success(ierarhic));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting departament ierarhic for {DepartamentId}", departamentId);
            return StatusCode(500, Result<DepartamentIerarhicDto?>.Failure("Eroare la înc?rcarea departamentului"));
        }
    }

    [HttpGet("ierarhie")]
    public async Task<ActionResult<Result<IEnumerable<DepartamentIerarhicDto>>>> GetIerarhieCompleta()
    {
        try
        {
            _logger.LogDebug("Getting ierarhie completa departamente");

            using var connection = _connectionFactory.CreateConnection();
            
            // Ob?ine structura ierarhic? complet?
            var structura = await connection.QueryAsync<DepartamentHierarchyRaw>(
                @"SELECT 
                    d.DepartamentID,
                    d.Nume,
                    d.Tip,
                    h.AncestorID as ParentID,
                    h.Nivel
                  FROM Departamente d
                  LEFT JOIN DepartamenteIerarhie h ON d.DepartamentID = h.DescendantID AND h.Nivel = 1
                  ORDER BY 
                    CASE d.Tip 
                        WHEN 'Categorie' THEN 1 
                        WHEN 'Specialitate' THEN 2 
                        WHEN 'Subspecialitate' THEN 3 
                        ELSE 4 
                    END,
                    d.Nume");

            // Construie?te ierarhia
            var ierarhie = ConstruiesteIerarhie(structura);

            return Ok(Result<IEnumerable<DepartamentIerarhicDto>>.Success(ierarhie));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ierarhie completa departamente");
            return StatusCode(500, Result<IEnumerable<DepartamentIerarhicDto>>.Failure("Eroare la înc?rcarea ierarhiei"));
        }
    }

    [HttpGet("statistici")]
    public async Task<ActionResult<Result<object>>> GetStatisticiDepartamente()
    {
        try
        {
            _logger.LogDebug("Getting statistici departamente");

            using var connection = _connectionFactory.CreateConnection();
            
            var statistici = await connection.QueryAsync<dynamic>(
                "EXEC sp_PersonalMedical_GetStatisticiByDepartamente");

            return Ok(Result<object>.Success(statistici));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistici departamente");
            return StatusCode(500, Result<object>.Failure("Eroare la înc?rcarea statisticilor"));
        }
    }

    private IEnumerable<DepartamentIerarhicDto> ConstruiesteIerarhie(IEnumerable<DepartamentHierarchyRaw> structura)
    {
        var dict = new Dictionary<Guid, DepartamentIerarhicDto>();
        var roots = new List<DepartamentIerarhicDto>();

        // Primul pas: creeaz? toate nodurile
        foreach (var item in structura)
        {
            var node = new DepartamentIerarhicDto
            {
                DepartamentID = item.DepartamentID,
                Nume = item.Nume,
                Tip = item.Tip,
                ParenteID = item.ParentID,
                Nivel = item.Nivel ?? 0,
                CalieCompleta = item.Nume,
                Copii = new List<DepartamentIerarhicDto>()
            };
            
            dict[item.DepartamentID] = node;
        }

        // Al doilea pas: construie?te rela?iile ?i determi? root-urile
        foreach (var item in structura)
        {
            var node = dict[item.DepartamentID];
            
            if (item.ParentID.HasValue && dict.TryGetValue(item.ParentID.Value, out var parent))
            {
                ((List<DepartamentIerarhicDto>)parent.Copii).Add(node);
                node.CalieCompleta = $"{parent.CalieCompleta} > {node.Nume}";
            }
            else
            {
                roots.Add(node);
            }
        }

        return roots.OrderBy(r => r.Nume);
    }

    private class DepartamentHierarchyRaw
    {
        public Guid DepartamentID { get; set; }
        public string Nume { get; set; } = string.Empty;
        public string Tip { get; set; } = string.Empty;
        public Guid? ParentID { get; set; }
        public int? Nivel { get; set; }
    }
}