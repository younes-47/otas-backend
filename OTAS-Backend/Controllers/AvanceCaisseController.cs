using Microsoft.AspNetCore.Mvc;
using OTAS.Interfaces.IRepository;
using OTAS.Models;

namespace OTAS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvanceCaisseController : ControllerBase
    {
        private readonly IAvanceCaisseRepository _avanceCaisseRepository;
        public AvanceCaisseController(IAvanceCaisseRepository avanceCaisseRepository)
        {
            _avanceCaisseRepository = avanceCaisseRepository;
        }

        // Requester
        [HttpGet("{AvanceCaisseId}")]
        public IActionResult GetAvanceCaisseById(int id)
        {
            var AC = _avanceCaisseRepository.GetAvanceCaisseById(id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(AC);
        }

        // Requester
        [HttpGet("Table")]
        public IActionResult GetAvancesCaisseByStatus(int status)
        {
            var ACs = _avanceCaisseRepository.GetAvancesCaisseByStatus(status);

            if(ACs.Count() <= 0)
                return NoContent();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(ACs);
        }

        //Decider
        [HttpGet("Decide/Table")]
        public IActionResult GetAvancesCaisseByRequesterUsername(int requesterUserId)
        {
            var ACs = _avanceCaisseRepository.GetAvancesCaisseByRequesterUserId(requesterUserId);

            if(ACs.Count() <= 0)
                return NoContent();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(ACs);
        }



    }
}
