using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OTAS.Data;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdreMissionController : ControllerBase
    {
        private readonly IOrdreMissionService _ordreMissionService;
        private readonly IOrdreMissionRepository _ordreMissionRepository;

        public OrdreMissionController(IOrdreMissionService ordreMissionService, IOrdreMissionRepository ordreMissionRepository)
        {
            _ordreMissionService = ordreMissionService;
            _ordreMissionRepository = ordreMissionRepository;
        }

        /* This get /Table endpoint is within the Request section and it is shown for all the roles
         * since everyone can perform a request, it shows all the Missions ordered */
        //[HttpGet("Table")]
        //public IActionResult OrdreMissionTable(int status)
        //{
        //    var oms = _mapper.Map<List<OrdreMissionDTO>>(_ordreMissionRepository.GetOrdresMissionByRequesterUsername());

        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    return Ok(oms);

        //}



        //Requester + decider
        [HttpPost("Create")]
        public async Task<IActionResult> AddOrdreMission([FromBody] OrdreMissionPostDTO ordreMissionRequest)
        {  
            if(!ModelState.IsValid) return BadRequest(ModelState);

            ServiceResult omResult = await _ordreMissionService.CreateOrdreMissionWithAvanceVoyageAsDraft(ordreMissionRequest);

            if (!omResult.Success) return Ok(omResult.Message);

            return Ok(omResult.Message); 
        }

        //Requester + decider
        [HttpPut("Submit")]
        public async Task<IActionResult> SubmitOrdreMission(int ordreMissionId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _ordreMissionRepository.FindOrdreMissionByIdAsync(ordreMissionId) == null) return NotFound("OrdreMission is not found!");

            ServiceResult result = await _ordreMissionService.SubmitOrdreMissionWithAvanceVoyage(ordreMissionId);
            if (!result.Success) return Ok(result.Message);
            return Ok(result.Message); 
        }

        //Decider Only
        [HttpPut("Decide")]
        public async Task<IActionResult> DecideOnOrdreMission(DecisionOnRequestPostDTO decision)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _ordreMissionRepository.FindOrdreMissionByIdAsync(decision.RequestId) == null) return NotFound("OrdreMission is not found!");

            ServiceResult result = await _ordreMissionService.DecideOnOrdreMissionWithAvanceVoyage(decision.RequestId, decision.DeciderUserId, decision.DeciderComment, decision.Decision);
            if (!result.Success) return Ok(result.Message);
            return Ok(result.Message);
        }

        //Requester
        [HttpPut("HandleReturn")]
        public async Task<IActionResult> HandleReturnedOrdreMission([FromBody] OrdreMissionPostDTO ordreMission)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            ServiceResult result = await _ordreMissionService.HandleReturnedOrdreMission(ordreMission);

            if (!result.Success) return Ok(result.Message);
            return Ok(result.Message);
        }

    }
}
