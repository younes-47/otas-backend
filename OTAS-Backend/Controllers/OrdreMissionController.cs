using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.DTO.Put;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Controllers
{
    [Authorize]
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

        [Authorize(Roles = "requester , decider")]
        [HttpGet("Requests/Table")]
        public async Task<IActionResult> ShowOrdreMissionRequestsTable()
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");
            var ordreMissions = await _ordreMissionRepository.GetOrdresMissionByUserIdAsync(user.Id);
            //if (ordreMissions == null || ordreMissions.Count <= 0) return NotFound("You haven't placed any \"OrdreMission\" yet!");

            //var mappedOrdreMissions = _mapper.Map<List<OrdreMissionDTO>>(ordreMissions);

            return Ok(ordreMissions);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpPost("Create")]
        public async Task<IActionResult> AddOrdreMission([FromBody] OrdreMissionPostDTO ordreMissionRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (ordreMissionRequest.Trips.Count < 2)
               return BadRequest("OrdreMission must have at least two trips!");
            if (ordreMissionRequest.OnBehalf == true && ordreMissionRequest.ActualRequester == null)
                return BadRequest("You must fill actual requester's info in case you are filling this request on behalf of someone");

            var sortedTrips = ordreMissionRequest.Trips.OrderBy(trip => trip.DepartureDate).ToList();
            for (int i = 0; i < sortedTrips.Count; i++)
            {
                if (ordreMissionRequest.Abroad == false && sortedTrips[i].Unit == "EUR")
                    return BadRequest("A trip estimated fee cannot be in EUR if your mission is not abroad!");
                if (sortedTrips[i].DepartureDate > sortedTrips[i].ArrivalDate)
                    return BadRequest("One of the trips has a departure date bigger than its arrival date!");

                if (sortedTrips[i].DepartureDate > sortedTrips[i].ArrivalDate)
                    return BadRequest("One of the trips has a departure date bigger than its arrival date!");

                if (i == sortedTrips.Count - 1) break; // prevent out of range index exception
                if (sortedTrips[i].ArrivalDate > sortedTrips[i + 1].DepartureDate)
                    return BadRequest("Trips dates don't make sense! You can't start another trip before you arrive from the previous one.");
            }
            if (ordreMissionRequest.Abroad == false)
            {
                foreach (ExpensePostDTO expense in ordreMissionRequest.Expenses)
                {
                    if (expense.Currency == "EUR") return BadRequest("An expense cannot be in EUR if your mission is not abroad!");
                }
            }

            var user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            ServiceResult omResult = await _ordreMissionService.CreateOrdreMissionWithAvanceVoyageAsDraft(ordreMissionRequest, user.Id);

            if (!omResult.Success) return BadRequest(omResult.Message);

            return Ok(omResult.Message);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpPut("Modify")]
        public async Task<IActionResult> ModifyOrdreMission([FromBody] OrdreMissionPutDTO ordreMission)
        {
            // VALIDATIONS

            if (!ModelState.IsValid) 
                return BadRequest(ModelState);
            if (await _ordreMissionRepository.FindOrdreMissionByIdAsync(ordreMission.Id) == null) 
                return NotFound("OrdreMission not found");

            bool isActionValid = ordreMission.Action.ToLower() == "save" || ordreMission.Action.ToLower() == "submit";
            if (!isActionValid) 
                return BadRequest("Action is invalid! If you are seeing this error, you are probably trying to manipulate the system. If not, please report the IT department with the issue.");

            var ordreMission_DB = await _ordreMissionRepository.GetOrdreMissionByIdAsync(ordreMission.Id);   
            if (ordreMission.Action.ToLower() != "save" && ordreMission_DB.LatestStatus == 98) 
                return BadRequest("You can't modify and save a returned request as a draft again. You may want to apply your modifications to you request and resubmit it directly");
            if (ordreMission_DB.LatestStatus != 98 && ordreMission_DB.LatestStatus != 99)
                return BadRequest("The OrdreMission you are trying to modify is not a draft nor returned! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks");
            if (ordreMission.Trips.Count < 2)           
                return BadRequest( "OrdreMission must have at least 2 trips!");
            if (ordreMission.OnBehalf == true && ordreMission.ActualRequester == null)
                return BadRequest("You must fill actual requester's info in case you are filling this request on behalf of someone");

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

            ServiceResult result = await _ordreMissionService.ModifyOrdreMissionWithAvanceVoyage(ordreMission);

            if (!result.Success) return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpPut("Submit")]
        public async Task<IActionResult> SubmitOrdreMission(int Id) // Id = ordreMissionId
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _ordreMissionRepository.FindOrdreMissionByIdAsync(Id) == null) 
                return NotFound("OrdreMission is not found!");

            var ordreMission_DB = await _ordreMissionRepository.GetOrdreMissionByIdAsync(Id);
            if (ordreMission_DB.LatestStatus != 99)
                return BadRequest("Action denied! You cannot submit the request in this state");

            ServiceResult result = await _ordreMissionService.SubmitOrdreMissionWithAvanceVoyage(Id);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpGet("View")]
        public async Task<IActionResult> ShowOrdreMissionDetailsPage(int Id) // Id = ordreMissionId
        {
            if(await _ordreMissionRepository.FindOrdreMissionByIdAsync(Id) == null) return NotFound("OrdreMission not found!");
            var ordreMission = await _ordreMissionRepository.GetOrdreMissionFullDetailsById(Id);
            return Ok(ordreMission);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteDraftedOrdreMission(int Id) // Id = ordreMissionId
        {
           
            if (await _ordreMissionRepository.FindOrdreMissionByIdAsync(Id) == null) return NotFound("OrdreMission not found!");
            OrdreMission ordreMission = await _ordreMissionRepository.GetOrdreMissionByIdAsync(Id);

            if (ordreMission.LatestStatus != 99)
                return BadRequest("The request is not in a draft. You cannot delete requests in a different status than draft.");

            ServiceResult result = await _ordreMissionService.DeleteDraftedOrdreMissionWithAvanceVoyages(ordreMission);
            return Ok(result.Message);
        }

        [Authorize(Roles ="decider")]
        [HttpGet("DecideOnRequests/Table")]
        public async Task<IActionResult> ShowOrdreMissionDecideTable()
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");

            List<OrdreMissionDTO> ordreMissions = await _ordreMissionRepository.GetOrdreMissionsForDecider(user.Id);
            return Ok(ordreMissions);
        }

        [Authorize(Roles = "decider")]
        [HttpPut("Decide")]
        public async Task<IActionResult> DecideOnOrdreMission(DecisionOnRequestPostDTO decision)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _ordreMissionRepository.FindOrdreMissionByIdAsync(decision.RequestId) == null) return NotFound("OrdreMission is not found!");

            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");

            int deciderRole = await _userRepository.GetUserRoleByUserIdAsync(user.Id);
            if (deciderRole != 3) return BadRequest("You are not authorized to decide upon requests!");

            ServiceResult result = await _ordreMissionService.DecideOnOrdreMission(decision,user.Id);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }


    }
}
