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
    public class AvanceVoyageController : ControllerBase
    {
        private readonly IAvanceVoyageRepository _avanceVoyageRepository;
        private readonly IAvanceVoyageService _avanceVoyageService;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;

        public AvanceVoyageController(IAvanceVoyageRepository avanceVoyageRepository,
            IAvanceVoyageService avanceVoyageService,
            IMapper mapper,
            IUserRepository userRepository)
        {
            _avanceVoyageRepository = avanceVoyageRepository;
            _avanceVoyageService = avanceVoyageService;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        [Authorize(Roles = "requester , decider")]
        [HttpGet("View")]
        public async Task<IActionResult> ShowAvanceVoyageDetailsPage(int Id)
        {
            if (await _avanceVoyageRepository.FindAvanceVoyageByIdAsync(Id) == null)
                return NotFound("AvanceVoyage is not found");
            var AV = await _avanceVoyageRepository.GetAvanceVoyageByIdAsync(Id);
            var mappedAV = _mapper.Map<AvanceVoyageTableDTO>(AV);
            return Ok(mappedAV);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpGet("Requests/Table")]
        public async Task<IActionResult> ShowAvanceVoyageRequestsTable()
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);

            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");

            List<AvanceVoyageTableDTO> mappedAVs = await _avanceVoyageRepository.GetAvancesVoyageByUserIdAsync(user.Id);

            return Ok(mappedAVs);
        }

        [Authorize(Roles = "decider")]
        [HttpGet("DecideOnRequests/Table")]
        public async Task<IActionResult> ShowAvanceVoyageDecideTable()
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");

            int deciderRole = await _userRepository.GetUserRoleByUserIdAsync(user.Id);
            if (deciderRole == 1 || deciderRole == 0) return BadRequest("You are not authorized to decide upon requests!");

            List<AvanceVoyageTableDTO> AVs = await _avanceVoyageRepository.GetAvanceVoyagesForDeciderTable(user.Id);

            return Ok(AVs);
        }

        [Authorize(Roles = "decider")]
        [HttpPut("Decide")]
        public async Task<IActionResult> DecideOnAvanceVoyage(DecisionOnRequestPostDTO decision)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _avanceVoyageRepository.FindAvanceVoyageByIdAsync(decision.RequestId) == null) return NotFound("AvanceVoyage is not found!");

            ServiceResult result = await _avanceVoyageService.DecideOnAvanceVoyage(decision);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }


    }
}
