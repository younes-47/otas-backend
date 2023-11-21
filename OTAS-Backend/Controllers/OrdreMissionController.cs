﻿using AutoMapper;
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
        private readonly IMapper _mapper;

        public OrdreMissionController(IOrdreMissionService ordreMissionService,
            IOrdreMissionRepository ordreMissionRepository,
            IMapper mapper)
        {
            _ordreMissionService = ordreMissionService;
            _ordreMissionRepository = ordreMissionRepository;
            _mapper = mapper;
        }

        //Requester + decider
        [HttpGet("Requests/Table")]
        public async Task<IActionResult> ShowOrdreMissionRequestsTable(int userId)
        {
            var ordreMissions = await _ordreMissionRepository.GetOrdresMissionByUserIdAsync(userId);
            if(ordreMissions == null || ordreMissions.Count <= 0) return NotFound("User has no \"OrdreMission\"");

            var mappedOrdreMissions = _mapper.Map<List<OrdreMissionDTO>>(ordreMissions);

            return Ok(mappedOrdreMissions);
        }

        // Decider
        [HttpGet("DecideOnRequests/Table")]
        public async Task<IActionResult> ShowOrdreMissionTable(int userId)
        {

        }


        //Requester + decider
        [HttpPost("Create")]
        public async Task<IActionResult> AddOrdreMission([FromBody] OrdreMissionPostDTO ordreMissionRequest)
        {  
            if(!ModelState.IsValid) return BadRequest(ModelState);

            if(ordreMissionRequest.Abroad == false)
            {
                foreach(TripPostDTO trip in ordreMissionRequest.Trips)
                {
                    if(trip.Unit == "EUR") return BadRequest("A trip estimated fee cannot be in EUR if your mission is not abroad!");
                }
                foreach(ExpensePostDTO expense in ordreMissionRequest.Expenses)
                {
                    if (expense.Currency == "EUR") return BadRequest("An expense cannot be in EUR if your mission is not abroad!");
                }
            }

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
        [HttpPut("Modify")]
        public async Task<IActionResult> ModifyOrdreMission([FromBody] OrdreMissionPostDTO ordreMission, [FromQuery] int action)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var testOrdreMission = await _ordreMissionRepository.FindOrdreMissionByIdAsync(ordreMission.Id);
            if(testOrdreMission == null) return NotFound("OrdreMission not found");

            if (ordreMission.Abroad == false)
            {
                foreach (TripPostDTO trip in ordreMission.Trips)
                {
                    if (trip.Unit == "EUR") return BadRequest("A trip estimated fee cannot be in EUR if your mission is not abroad!");
                }
                foreach (ExpensePostDTO expense in ordreMission.Expenses)
                {
                    if (expense.Currency == "EUR") return BadRequest("An expense cannot be in EUR if your mission is not abroad!");
                }
            }

            ServiceResult result = await _ordreMissionService.ModifyOrdreMission(ordreMission, action);

            if (!result.Success) return Ok(result.Message);
            return Ok(result.Message);
        }

    }
}
