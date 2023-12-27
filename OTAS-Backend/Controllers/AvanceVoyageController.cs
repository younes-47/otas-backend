using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Get;
using OTAS.Interfaces.IRepository;
using OTAS.Models;
using OTAS.Repository;

namespace OTAS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AvanceVoyageController : ControllerBase
    {
        private readonly IAvanceVoyageRepository _avanceVoyageRepository;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;

        public AvanceVoyageController(IAvanceVoyageRepository avanceVoyageRepository,
            IMapper mapper,
            IUserRepository userRepository)
        {
            _avanceVoyageRepository = avanceVoyageRepository;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        [Authorize(Roles = "requester , decider")]
        [HttpGet("{avanceVoyageId}/View")]
        public async Task<IActionResult> ShowAvanceVoyageDetailsPage(int avanceVoyageId)
        {
            if (await _avanceVoyageRepository.FindAvanceVoyageByIdAsync(avanceVoyageId) == null)
                return NotFound("AvanceVoyage is not found");
            var AV = await _avanceVoyageRepository.GetAvanceVoyageByIdAsync(avanceVoyageId);
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

            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) 
                return BadRequest("User not found!");

            int deciderRole = await _userRepository.GetUserRoleByUserIdAsync(user.Id);

            if (deciderRole == 1 || deciderRole == 0) 
                return BadRequest("You are not authorized to decide upon requests!");

            List<AvanceVoyageTableDTO> AVs = await _avanceVoyageRepository.GetAvanceVoyagesForDeciderTable(deciderRole);
            if (AVs.Count <= 0) 
                return NotFound("No AvanceVoyage to decide upon!");

            return Ok(AVs);
        }
        
    }
}
