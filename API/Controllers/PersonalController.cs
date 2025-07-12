using API.DTOs;
using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonalController : ControllerBase
    {
        private readonly IPersoanaService _persoanaService;

        public PersonalController(IPersoanaService persoanaService)
        {
            _persoanaService = persoanaService;
        }

        /// <summary>
        /// Gets all personnel records
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PersoanaDTO>>> GetAllPersonal()
        {
            try
            {
                var personal = await _persoanaService.GetAllPersonalAsync();
                return Ok(personal);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error retrieving data: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets a personnel record by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PersoanaDTO>> GetPersonalById(int id)
        {
            try
            {
                var persoana = await _persoanaService.GetPersonaByIdAsync(id);
                if (persoana == null)
                    return NotFound($"Personnel with ID {id} not found");
                    
                return Ok(persoana);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error retrieving data: {ex.Message}");
            }
        }

        /// <summary>
        /// Creates a new personnel record
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<PersoanaDTO>> CreatePersonal(CreatePersoanaDTO personaDto)
        {
            try
            {
                if (personaDto == null)
                    return BadRequest("Personnel data is null");

                var id = await _persoanaService.CreatePersonaAsync(personaDto);
                
                // Get the created entity to return it
                var createdPersona = await _persoanaService.GetPersonaByIdAsync(id);
                
                return CreatedAtAction(nameof(GetPersonalById), new { id }, createdPersona);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error creating personnel record: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates an existing personnel record
        /// </summary>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PersoanaDTO>> UpdatePersonal(UpdatePersoanaDTO personaDto)
        {
            try
            {
                if (personaDto == null)
                    return BadRequest("Personnel data is null");
                    
                var result = await _persoanaService.UpdatePersonaAsync(personaDto);
                
                if (!result)
                    return NotFound($"Personnel with ID {personaDto.Id} not found");
                    
                // Get the updated entity to return it
                var updatedPersona = await _persoanaService.GetPersonaByIdAsync(personaDto.Id);
                return Ok(updatedPersona);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error updating personnel record: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes a personnel record
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeletePersonal(int id)
        {
            try
            {
                var result = await _persoanaService.DeletePersonaAsync(id);
                
                if (!result)
                    return NotFound($"Personnel with ID {id} not found");
                    
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    $"Error deleting personnel record: {ex.Message}");
            }
        }
    }
}