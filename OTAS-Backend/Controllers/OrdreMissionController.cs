﻿using AutoMapper;
using Microsoft.AspNetCore.Mvc;
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
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public OrdreMissionController(IOrdreMissionService ordreMissionService,
            IOrdreMissionRepository ordreMissionRepository,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _ordreMissionService = ordreMissionService;
            _ordreMissionRepository = ordreMissionRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        //Requester
        [HttpGet("Requests/Table")]
        public async Task<IActionResult> ShowOrdreMissionRequestsTable(int userId)
        {
            if (await _userRepository.FindUserByUserIdAsync(userId) == null) return BadRequest("User not found!");
            var ordreMissions = await _ordreMissionRepository.GetOrdresMissionByUserIdAsync(userId);
            //if (ordreMissions == null || ordreMissions.Count <= 0) return NotFound("You haven't placed any \"OrdreMission\" yet!");

            //var mappedOrdreMissions = _mapper.Map<List<OrdreMissionDTO>>(ordreMissions);

            return Ok(ordreMissions);
        }

        //Requester
        [HttpPost("Create")]
        public async Task<IActionResult> AddOrdreMission([FromBody] OrdreMissionPostDTO ordreMissionRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (ordreMissionRequest.Abroad == false)
            {
                foreach (TripPostDTO trip in ordreMissionRequest.Trips)
                {
                    if (trip.Unit == "EUR") return BadRequest("A trip estimated fee cannot be in EUR if your mission is not abroad!");
                }
                foreach (ExpensePostDTO expense in ordreMissionRequest.Expenses)
                {
                    if (expense.Currency == "EUR") return BadRequest("An expense cannot be in EUR if your mission is not abroad!");
                }
            }

            ServiceResult omResult = await _ordreMissionService.CreateOrdreMissionWithAvanceVoyageAsDraft(ordreMissionRequest);

            if (!omResult.Success) return BadRequest(omResult.Message);

            return Ok(omResult.Message);
        }

        //Requester
        [HttpPut("Modify")]
        public async Task<IActionResult> ModifyOrdreMission([FromBody] OrdreMissionPostDTO ordreMission, [FromQuery] int action)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _ordreMissionRepository.FindOrdreMissionByIdAsync(ordreMission.Id) == null) return NotFound("OrdreMission not found");

            bool isActionValid = action == 99 || action == 1;
            if (!isActionValid) return BadRequest("Action is invalid! If you are seeing this error, you are probably trying to manipulate the system. If not, please report the IT department with the issue.");

            if (action == 99 && (ordreMission.LatestStatus == 98 || ordreMission.LatestStatus == 97)) return BadRequest("You cannot save a returned or a rejected request as a draft!");

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

            ServiceResult result = await _ordreMissionService.ModifyOrdreMissionWithAvanceVoyage(ordreMission, action);

            if (!result.Success) return BadRequest(result.Message);

            return Ok(result.Message);
        }

        //Requester
        [HttpPut("Submit")]
        public async Task<IActionResult> SubmitOrdreMission(int ordreMissionId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _ordreMissionRepository.FindOrdreMissionByIdAsync(ordreMissionId) == null) return NotFound("OrdreMission is not found!");

            ServiceResult result = await _ordreMissionService.SubmitOrdreMissionWithAvanceVoyage(ordreMissionId);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }


        [HttpGet("{ordreMissionId}/View")]
        public async Task<IActionResult> ShowOrdreMissionDetailsPage(int ordreMissionId)
        {
            if(await _ordreMissionRepository.FindOrdreMissionByIdAsync(ordreMissionId) == null) return NotFound("OrdreMission not found!");
            var ordreMission = await _ordreMissionRepository.GetOrdreMissionFullDetailsById(ordreMissionId);
            return Ok(ordreMission);
        }


        // Decider
        [HttpGet("DecideOnRequests/Table")]
        public async Task<IActionResult> ShowOrdreMissionDecideTable(int userId)
        {
            if (await _userRepository.FindUserByUserIdAsync(userId) == null) return BadRequest("User not found!");
            List<OrdreMissionDTO> ordreMissions = await _ordreMissionService.GetOrdreMissionsForDeciderTable(userId);
            if (ordreMissions.Count <= 0) return NotFound("User has no \"OrdreMission\" to decide upon!");

            return Ok(ordreMissions);
        }


        //Decider
        [HttpPut("Decide")]
        public async Task<IActionResult> DecideOnOrdreMission(DecisionOnRequestPostDTO decision)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _ordreMissionRepository.FindOrdreMissionByIdAsync(decision.RequestId) == null) return NotFound("OrdreMission is not found!");

            ServiceResult result = await _ordreMissionService.DecideOnOrdreMissionWithAvanceVoyage(decision.RequestId, decision.DeciderUserId, decision.DeciderComment, decision.Decision);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }


    }
}
