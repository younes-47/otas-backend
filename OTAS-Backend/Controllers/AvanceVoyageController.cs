using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Get;
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


        [HttpGet("{avanceVoyageId}/View")]
        public async Task<IActionResult> ShowAvanceVoyageDetailsPage(int avanceVoyageId)
        {
            if (await _avanceVoyageRepository.FindAvanceVoyageByIdAsync(avanceVoyageId) == null)
                return NotFound("AvanceVoyage is not found");
            var AV = await _avanceVoyageRepository.GetAvanceVoyageByIdAsync(avanceVoyageId);
            var mappedAV = _mapper.Map<AvanceVoyageTableDTO>(AV);
            return Ok(mappedAV);
        }

        //Requester
        [HttpGet("Requests/Table")]
        public async Task<IActionResult> ShowAvanceVoyageRequestsTable(int userId)
        {
            if (await _userRepository.FindUserByUserIdAsync(userId) == null) return BadRequest("User not found!");

            List<AvanceVoyageTableDTO> mappedAVs = await _avanceVoyageRepository.GetAvancesVoyageByUserIdAsync(userId);

            return Ok(mappedAVs);
        }

        //Decider
        [HttpGet("DecideOnRequests/Table")]
        public async Task<IActionResult> ShowAvanceVoyageDecideTable(int userId)
        {
            if (await _userRepository.FindUserByUserIdAsync(userId) == null) 
                return BadRequest("User not found!");

            int deciderRole = await _userRepository.GetUserRoleByUserIdAsync(userId);

            if (deciderRole == 1 || deciderRole == 0) 
                return BadRequest("You are not authorized to decide upon requests!");

            List<AvanceVoyageTableDTO> AVs = await _avanceVoyageRepository.GetAvanceVoyagesForDeciderTable(deciderRole);
            if (AVs.Count <= 0) 
                return NotFound("No AvanceVoyage to decide upon!");

            return Ok(AVs);
        }
        
    }
}
