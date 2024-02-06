using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class LiquidationController : ControllerBase
    {
        private readonly ILiquidationRepository _liquidationRepository;
        private readonly IActualRequesterRepository _actualRequesterRepository;
        private readonly ILiquidationService _liquidationService;
        private readonly IAvanceVoyageRepository _avanceVoyageRepository;
        private readonly IAvanceCaisseRepository _avanceCaisseRepository;
        private readonly ITripRepository _tripRepository;
        private readonly IMapper _mapper;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IUserRepository _userRepository;

        public LiquidationController(ILiquidationRepository liquidationRepository,
            IActualRequesterRepository actualRequesterRepository,
            ILiquidationService liquidationService,
            IAvanceVoyageRepository avanceVoyageRepository,
            IAvanceCaisseRepository avanceCaisseRepository,
            ITripRepository tripRepository,
            IMapper mapper,
            IExpenseRepository expenseRepository,
            IUserRepository userRepository)
        {
            _liquidationRepository = liquidationRepository;
            _actualRequesterRepository = actualRequesterRepository;
            _liquidationService = liquidationService;
            _avanceVoyageRepository = avanceVoyageRepository;
            _avanceCaisseRepository = avanceCaisseRepository;
            _tripRepository = tripRepository;
            _mapper = mapper;
            _expenseRepository = expenseRepository;
            _userRepository = userRepository;
        }

        // LIQUIDATE ACTIONS
        [Authorize(Roles = "requester,decider")]
        [HttpPost("Liquidate/AvanceVoyage")]
        public async Task<IActionResult> LiquidateAvanceVoyage(AvanceVoyageLiquidationPostDTO avanceVoyageLiquidation)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);
            if (await _avanceVoyageRepository.FindAvanceVoyageByIdAsync(avanceVoyageLiquidation.AvanceVoyageId) == null)
                return BadRequest("AvanceVoyage not found!");

            var avanceVoyageDB = await _avanceVoyageRepository.GetAvanceVoyageByIdAsync(avanceVoyageLiquidation.AvanceVoyageId);
            if(avanceVoyageDB.LatestStatus != 10)
                return BadRequest("You cannot lequidate this request on this state!");

            var tripsDB = await _tripRepository.GetAvanceVoyageTripsByAvIdAsync(avanceVoyageLiquidation.AvanceVoyageId);
            var expensesDB = await _expenseRepository.GetAvanceVoyageExpensesByAvIdAsync(avanceVoyageLiquidation.AvanceVoyageId);

            if (avanceVoyageLiquidation.TripsLiquidations.Count != tripsDB.Count || avanceVoyageLiquidation.ExpensesLiquidations.Count != expensesDB.Count)
                return BadRequest("You must liquidate all expenses and trips!");

            var user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            ServiceResult result = await _liquidationService.LiquidateAvanceVoyage(avanceVoyageDB, avanceVoyageLiquidation, user.Id);
            if (!result.Success) return BadRequest($"{result.Message}");

            return Ok(result.Message);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpPost("Liquidate/AvanceCaisse")]
        public async Task<IActionResult> LiquidateAvanceCaisse(AvanceCaisseLiquidationPostDTO avanceCaisseLiquidation)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (await _avanceCaisseRepository.FindAvanceCaisseAsync(avanceCaisseLiquidation.AvanceCaisseId) == null)
                return BadRequest("AvanceCaisse not found!");

            var avanceCaisseDB = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync(avanceCaisseLiquidation.AvanceCaisseId);
            if (avanceCaisseDB.LatestStatus != 10)
                return BadRequest("You cannot lequidate this request on this state!");

            var expensesDB = await _expenseRepository.GetAvanceCaisseExpensesByAcIdAsync(avanceCaisseLiquidation.AvanceCaisseId);

            if (avanceCaisseLiquidation.ExpensesLiquidations.Count != expensesDB.Count)
                return BadRequest("You must liquidate all expenses!");

            var user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            ServiceResult result = await _liquidationService.LiquidateAvanceCaisse(avanceCaisseDB,avanceCaisseLiquidation, user.Id);
            if (!result.Success) return BadRequest($"{result.Message}");

            return Ok(result.Message);
        }

        // REQS TO MEANT TO BE LIQUIDATED
        [Authorize(Roles = "requester,decider")]
        [HttpGet("Requests/Pending")]
        public async Task<IActionResult> GetRequestsToLiquidate([FromQuery] string requestType)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
           if(requestType != "AC" && requestType != "AV")
                return BadRequest("Request type is invalid");

            var user = await _userRepository.GetUserByHttpContextAsync(HttpContext);

            var response = await _liquidationRepository.GetRequestsToLiquidatesByTypeAsync(requestType, user.Id);

            return Ok(response);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("Requests/ToLiquidate/AvanceCaisse")]
        public async Task<IActionResult> GetAvanceCaisseToLiquidateDetails([FromQuery] int requestId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var req = await _avanceCaisseRepository.GetAvanceCaisseDetailsForLiquidationAsync(requestId);

            if (req == null) return BadRequest("Request Not Found");
            
            // if the request is on behalf of someone, get the requester data from the DB
            if (req.OnBehalf)
            {
                ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByAvanceCaisseIdAsync(req.Id);
                req.ActualRequester = _mapper.Map<ActualRequesterDTO>(actualRequesterInfo);
                req.ActualRequester.ManagerUserName = await _userRepository.GetUsernameByUserIdAsync(actualRequesterInfo.ManagerUserId);
            }

            return Ok(req);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("Requests/ToLiquidate/AvanceVoyage")]
        public async Task<IActionResult> GetAvanceVoyageToLiquidateDetails([FromQuery] int requestId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var req = await _avanceVoyageRepository.GetAvanceVoyageDetailsForLiquidationAsync(requestId);

            if (req == null) return BadRequest("Request Not Found");

            // if the request is on behalf of someone, get the requester data from the DB
            if (req.OnBehalf)
            {
                ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByAvanceCaisseIdAsync(req.Id);
                req.ActualRequester = _mapper.Map<ActualRequesterDTO>(actualRequesterInfo);
                req.ActualRequester.ManagerUserName = await _userRepository.GetUsernameByUserIdAsync(actualRequesterInfo.ManagerUserId);
            }

            return Ok(req);
        }


        // TABLES
        [Authorize(Roles = "requester,decider")]
        [HttpGet("Requester/Table")]
        public async Task<IActionResult> LiquidationsRequesterTable()
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);

            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");

            List<LiquidationTableDTO> liquidations = await _liquidationRepository.GetLiquidationsTableForRequster(user.Id);

            return Ok(liquidations);
        }

        [Authorize(Roles = "decider")]
        [HttpGet("DecideOnRequests/Table")]
        public async Task<IActionResult> LiquidationsDeciderTable()
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);

            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");

            List<LiquidationTableDTO> liquidations = await _liquidationRepository.GetLiquidationsTableForDecider(user.Id);

            return Ok(liquidations);
        }

    }
}
