using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.DTO.Put;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using OTAS.Repository;
using OTAS.Services;
using TMA_App.Services;

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
        private readonly ILdapAuthenticationService _ldapAuthenticationService;
        private readonly IActualRequesterRepository _actualRequesterRepository;
        private readonly IMiscService _miscService;
        private readonly IDeciderRepository _deciderRepository;
        private readonly IMapper _mapper;

        public OrdreMissionController(IOrdreMissionService ordreMissionService,
            IOrdreMissionRepository ordreMissionRepository,
            IUserRepository userRepository,
            ILdapAuthenticationService ldapAuthenticationService,
            IActualRequesterRepository actualRequesterRepository,
            IMiscService miscService,
            IDeciderRepository deciderRepository,
            IMapper mapper)
        {
            _ordreMissionService = ordreMissionService;
            _ordreMissionRepository = ordreMissionRepository;
            _userRepository = userRepository;
            _ldapAuthenticationService = ldapAuthenticationService;
            _actualRequesterRepository = actualRequesterRepository;
            _miscService = miscService;
            _deciderRepository = deciderRepository;
            _mapper = mapper;
        }

        [Authorize(Roles = "requester,decider")]
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

        [Authorize(Roles = "requester,decider")]
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

            return Ok(omResult.Id);
        }

        [Authorize(Roles = "requester,decider")]
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
            if (ordreMission.Action.ToLower() == "save" && ordreMission_DB.LatestStatus == 98) 
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

            return Ok(result.Id);
        }

        [Authorize(Roles = "requester,decider")]
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

        [Authorize(Roles = "requester,decider")]
        [HttpGet("View")]
        public async Task<IActionResult> ShowOrdreMissionDetailsPage(int Id) // Id = ordreMissionId
        {
            if(await _ordreMissionRepository.FindOrdreMissionByIdAsync(Id) == null) return NotFound("OrdreMission not found!");
            OrdreMissionViewDTO ordreMission = await _ordreMissionRepository.GetOrdreMissionFullDetailsById(Id);

            // if the request is on behalf of someone, get the requester data from the DB
            if (ordreMission.OnBehalf)
            {
                ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByOrdreMissionIdAsync(ordreMission.Id);
                ordreMission.RequesterInfo = _mapper.Map<ActualRequesterDTO>(actualRequesterInfo);
                ordreMission.RequesterInfo.ManagerUserName = await _userRepository.GetUsernameByUserIdAsync(actualRequesterInfo.ManagerUserId);
            }

            // adding those more detailed status that are not in the DB
            ordreMission.StatusHistory = _miscService.IllustrateStatusHistory(ordreMission.StatusHistory);

            return Ok(ordreMission);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteDraftedOrdreMission(int Id) // Id = ordreMissionId
        {
           
            if (await _ordreMissionRepository.FindOrdreMissionByIdAsync(Id) == null) return NotFound("OrdreMission not found!");
            OrdreMission ordreMission = await _ordreMissionRepository.GetOrdreMissionByIdAsync(Id);

            if (ordreMission.LatestStatus != 99)
                return BadRequest("The request is not in a draft. You cannot delete requests in a different status than draft.");

            ServiceResult result = await _ordreMissionService.DeleteDraftedOrdreMissionWithAvanceVoyages(ordreMission);
            if(!result.Success) return BadRequest(result.Message);

            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            var newOrdreMission = await _ordreMissionRepository.GetOrdresMissionByUserIdAsync(user.Id);

            return Ok(newOrdreMission);
        }

        [Authorize(Roles ="decider")]
        [HttpGet("DecideOnRequests/Table")]
        public async Task<IActionResult> ShowOrdreMissionDecideTable()
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");

            List<OrdreMissionDeciderTableDTO> ordreMissions = await _ordreMissionRepository.GetOrdreMissionsForDecider(user.Id);
            return Ok(ordreMissions);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("DecideOnRequests/View")]
        public async Task<IActionResult> ShowOrdreMissionDetailsPageForDecider(int Id) // Id = ordreMissionId
        {
            if (await _ordreMissionRepository.FindOrdreMissionByIdAsync(Id) == null) return NotFound("OrdreMission not found!");
            OrdreMissionDeciderViewDTO ordreMission = await _ordreMissionRepository.GetOrdreMissionFullDetailsByIdForDecider(Id);

            // Fill the requester data
            string username = await _userRepository.GetUsernameByUserIdAsync(ordreMission.UserId);
            var userInfo = _ldapAuthenticationService.GetUserInformation(username);
            ordreMission.Requester = userInfo;

            // if the request is on behalf of someone, get the requester data from the DB
            if (ordreMission.OnBehalf)
            {
                ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByOrdreMissionIdAsync(ordreMission.Id);
                ordreMission.ActualRequester = _mapper.Map<ActualRequesterDTO>(actualRequesterInfo);
                ordreMission.ActualRequester.ManagerUserName = await _userRepository.GetUsernameByUserIdAsync(actualRequesterInfo.ManagerUserId);
            }

            // adding those more detailed status that are not in the DB
            ordreMission.StatusHistory = _miscService.IllustrateStatusHistory(ordreMission.StatusHistory);

            return Ok(ordreMission);
        }

        [Authorize(Roles = "decider")]
        [HttpPut("Decide")]
        public async Task<IActionResult> DecideOnOrdreMission(DecisionOnRequestPostDTO decision)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _ordreMissionRepository.FindOrdreMissionByIdAsync(decision.RequestId) == null) return NotFound("OrdreMission is not found!");

            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");

            bool isDecisionValid = new[] { "approve", "return", "reject", "approveall" }.Contains(decision.DecisionString.ToLower());
            if (!isDecisionValid)
                return BadRequest("Decision is invalid!");


            int deciderRole = await _userRepository.GetUserRoleByUserIdAsync(user.Id);
            if (deciderRole != 3) return BadRequest("You are not authorized to decide upon requests!");

            List<string> levels = await _deciderRepository.GetDeciderLevelsByUserId(user.Id);
            if (levels.Contains("TR"))
                return BadRequest("You are not authorized to decide upon Mission Orders");

            if (decision.DecisionString.ToLower() == "approveall")
            {
                ServiceResult resultApproveAll = await _ordreMissionService.ApproveOrdreMissionWithAvanceVoyage(decision.RequestId, user.Id);
                if (!resultApproveAll.Success) return BadRequest(resultApproveAll.Message);
                return Ok(resultApproveAll.Message);
            }
            ServiceResult result = await _ordreMissionService.DecideOnOrdreMission(decision, user.Id);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }


    }
}
