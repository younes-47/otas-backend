using Microsoft.AspNetCore.Mvc;
using OTAS.Interfaces.IRepository;
using OTAS.Models;
using OTAS.Repository;

namespace OTAS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AvanceVoyageController : ControllerBase
    {
        private readonly IAvanceVoyageRepository _avanceVoyageRepository;
        public AvanceVoyageController(IAvanceVoyageRepository avanceVoyageRepository)
        {
            _avanceVoyageRepository = avanceVoyageRepository;
        }


        [HttpGet("{AvanceVoyageId}")]
        public async Task<IActionResult> GetAvanceVoyageById(int id)
        {
            var AV = await _avanceVoyageRepository.GetAvanceVoyageById(id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(AV);
        }

        [HttpGet("Table")]
        public async Task<IActionResult> GetAvancesVoyageByStatus(int status)
        {
            var AVs = await _avanceVoyageRepository.GetAvancesVoyageByStatus(status);

            if(AVs.Count <= 0)
                return NoContent();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(AVs);
        }

        [HttpGet("OrdreMission-{ordreMissionId}")]
        public async Task<IActionResult> GetAvancesVoyageByOrdreMissionId(int ordreMissionId)
        {
            var AVs = await _avanceVoyageRepository.GetAvancesVoyageByOrdreMissionId(ordreMissionId);

            if (AVs.Count <= 0)
                return NoContent();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            return Ok(AVs);
        }

    }
}
