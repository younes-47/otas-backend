using AutoMapper;
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

        // Requester
        [HttpGet("Requests/Table")]
        public async Task<IActionResult> ShowAvanceCaisseRequestsTable(int userId)
        {
            if (await _userRepository.FindUserByUserIdAsync(userId) == null) return BadRequest("User not found!");
            var mappedACs = await _avanceCaisseRepository.GetAvancesCaisseByUserIdAsync(userId);
            return Ok(mappedACs);
        }

        //Requester
        [HttpPost("Create")]
        public async Task<IActionResult> AddAvanceCaisse([FromBody] AvanceCaissePostDTO avanceCaisseRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            ServiceResult avResult = await _avanceCaisseService.CreateAvanceCaisseAsync(avanceCaisseRequest);

            if (!avResult.Success) return Ok(avResult.Message);

            return Ok(avResult.Message);
        }


        //Requester
        [HttpPut("Modify")]
        public async Task<IActionResult> ModifyAvanceCaisse([FromBody] AvanceCaissePostDTO avanceCaisse, [FromQuery] int action)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _avanceCaisseRepository.FindAvanceCaisseAsync(avanceCaisse.Id) == null) return NotFound("AvanceCaisse is not found");

            bool isActionValid = action == 99 || action == 1;
            if (!isActionValid) return BadRequest("Action is invalid! If you are seeing this error, you are probably trying to manipulate the system. If not, please report the IT department with the issue.");

            if (action == 99 && (avanceCaisse.LatestStatus == 98 || avanceCaisse.LatestStatus == 97)) return BadRequest("You cannot save a returned or a rejected request as a draft!");

            ServiceResult result = await _avanceCaisseService.ModifyAvanceCaisse(avanceCaisse, action);

            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }

        //Requester
        [HttpPut("Submit")]
        public async Task<IActionResult> SubmitAvanceCaisse(int avanceCaisseId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _avanceCaisseRepository.FindAvanceCaisseAsync(avanceCaisseId) == null) return NotFound("AvanceCaisse is not found!");

            ServiceResult result = await _avanceCaisseService.SubmitAvanceCaisse(avanceCaisseId);
            if (!result.Success) return BadRequest(result.Message);

            return Ok(result.Message);
        }

        // Requester
        [HttpGet("{avanceCaisseId}/View")]
        public async Task<IActionResult> GetAvanceCaisseById(int avanceCaisseId)
        {
            if (await _avanceCaisseRepository.FindAvanceCaisseAsync(avanceCaisseId) == null) return NotFound("AvanceCaisse is not found");
            var avanceCaisseDetails = await _avanceCaisseService.GetAvanceCaisseFullDetailsById(avanceCaisseId);

            return Ok(avanceCaisseDetails);
        }

        //Decider
        [HttpGet("DecideOnRequests/Table")]
        public async Task<IActionResult> GetAvancesCaisseByUserId(int userId)
        {
            if (await _userRepository.FindUserByUserIdAsync(userId) == null) return BadRequest("User not found!");
            int deciderRole = await _userRepository.GetUserRoleByUserIdAsync(userId);
            if (deciderRole == 1 || deciderRole == 0) 
                return BadRequest("You are not authorized to decide upon requests!");
            List<AvanceCaisseDTO> ACs = await _avanceCaisseService.GetAvanceCaissesForDeciderTable(deciderRole);
            if (ACs.Count <= 0) 
                return NotFound("No AvanceCaisse to decide upon!");

            return Ok(ACs);
        }

        //Decider
        [HttpPut("Decide")]
        public async Task<IActionResult> DecideOnAvanceCaisse(DecisionOnRequestPostDTO decision)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _avanceCaisseRepository.FindAvanceCaisseAsync(decision.RequestId) == null) return NotFound("AvanceCaisse is not found!");
            if (await _userRepository.FindUserByUserIdAsync(decision.DeciderUserId) == null) return NotFound("Decider is not found");

            bool isDecisionValid = decision.Decision > 1 && decision.Decision <= 14 || decision.Decision == 98 || decision.Decision == 97;
            if (!isDecisionValid) return BadRequest("Decision is invalid!");

            ServiceResult result = await _avanceCaisseService.DecideOnAvanceCaisse(decision.RequestId, decision.AdvanceOption, decision.DeciderUserId, decision.DeciderComment, decision.Decision);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }
    }
}
