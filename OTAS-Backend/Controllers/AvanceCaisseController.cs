using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using OTAS.Repository;
using OTAS.Services;

namespace OTAS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AvanceCaisseController : ControllerBase
    {
        private readonly IAvanceCaisseRepository _avanceCaisseRepository;
        private readonly IAvanceCaisseService _avanceCaisseService;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public AvanceCaisseController(IAvanceCaisseRepository avanceCaisseRepository,
            IAvanceCaisseService avanceCaisseService,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _avanceCaisseRepository = avanceCaisseRepository;
            _avanceCaisseService = avanceCaisseService;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        [Authorize(Roles = "requester , decider")]
        [HttpGet("Requests/Table")]
        public async Task<IActionResult> ShowAvanceCaisseRequestsTable()
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");
            var mappedACs = await _avanceCaisseRepository.GetAvancesCaisseByUserIdAsync(user.Id);
            return Ok(mappedACs);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpPost("Create")]
        public async Task<IActionResult> AddAvanceCaisse([FromBody] AvanceCaissePostDTO avanceCaisseRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            ServiceResult avResult = await _avanceCaisseService.CreateAvanceCaisseAsync(avanceCaisseRequest, user.Id);

            if (!avResult.Success) return Ok(avResult.Message);

            return Ok(avResult.Message);
        }


        [Authorize(Roles = "requester , decider")]
        [HttpPut("Modify")]
        public async Task<IActionResult> ModifyAvanceCaisse([FromBody] AvanceCaissePostDTO avanceCaisse, [FromQuery] string action)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _avanceCaisseRepository.FindAvanceCaisseAsync(avanceCaisse.Id) == null) return NotFound("AvanceCaisse is not found");

            bool isActionValid = action.ToLower() == "submit" || action.ToLower() == "save";
            if (!isActionValid) return BadRequest("Action is invalid! If you are seeing this error, you are probably trying to manipulate the system. If not, please report the IT department with the issue.");

            var AC = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync(avanceCaisse.Id);
            if (action.ToLower() == "save" && (AC.LatestStatus == 98 || AC.LatestStatus == 97)) return BadRequest("You cannot save a returned or a rejected request as a draft!");

            ServiceResult result = await _avanceCaisseService.ModifyAvanceCaisse(avanceCaisse, action);

            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpPut("Submit")]
        public async Task<IActionResult> SubmitAvanceCaisse(int avanceCaisseId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _avanceCaisseRepository.FindAvanceCaisseAsync(avanceCaisseId) == null) return NotFound("AvanceCaisse is not found!");

            ServiceResult result = await _avanceCaisseService.SubmitAvanceCaisse(avanceCaisseId);
            if (!result.Success) return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpGet("{avanceCaisseId}/View")]
        public async Task<IActionResult> GetAvanceCaisseById(int avanceCaisseId)
        {
            if (await _avanceCaisseRepository.FindAvanceCaisseAsync(avanceCaisseId) == null) return NotFound("AvanceCaisse is not found");
            var avanceCaisseDetails = await _avanceCaisseService.GetAvanceCaisseFullDetailsById(avanceCaisseId);

            return Ok(avanceCaisseDetails);
        }

        [Authorize(Roles = "decider")]
        [HttpGet("DecideOnRequests/Table")]
        public async Task<IActionResult> GetAvancesCaisseByUserId()
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);

            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");
            int deciderRole = await _userRepository.GetUserRoleByUserIdAsync(user.Id);
            if (deciderRole == 1 || deciderRole == 0) 
                return BadRequest("You are not authorized to decide upon requests!");
            List<AvanceCaisseDTO> ACs = await _avanceCaisseService.GetAvanceCaissesForDeciderTable(deciderRole);
            if (ACs.Count <= 0) 
                return NotFound("No AvanceCaisse to decide upon!");

            return Ok(ACs);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpPut("Decide")]
        public async Task<IActionResult> DecideOnAvanceCaisse(DecisionOnRequestPostDTO decision)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _avanceCaisseRepository.FindAvanceCaisseAsync(decision.RequestId) == null) return NotFound("AvanceCaisse is not found!");
            if (await _userRepository.FindUserByUserIdAsync(decision.DeciderUserId) == null) return NotFound("Decider is not found");

            bool isDecisionValid = decision.DecisionString.ToLower() != "approve" && decision.DecisionString.ToLower() != "return" && decision.DecisionString.ToLower() != "reject";
            if (!isDecisionValid) return BadRequest("Decision is invalid!");

            ServiceResult result = await _avanceCaisseService.DecideOnAvanceCaisse(decision);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }
    }
}
