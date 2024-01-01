using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Post;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
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
        private readonly ILiquidationService _liquidationService;
        private readonly IAvanceVoyageRepository _avanceVoyageRepository;
        private readonly ITripRepository _tripRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IUserRepository _userRepository;

        public LiquidationController(ILiquidationRepository liquidationRepository,
            ILiquidationService liquidationService,
            IAvanceVoyageRepository avanceVoyageRepository,
            ITripRepository tripRepository,
            IExpenseRepository expenseRepository,
            IUserRepository userRepository)
        {
            _liquidationRepository = liquidationRepository;
            _liquidationService = liquidationService;
            _avanceVoyageRepository = avanceVoyageRepository;
            _tripRepository = tripRepository;
            _expenseRepository = expenseRepository;
            _userRepository = userRepository;
        }

        [Authorize(Roles = "requester, decider")]
        [HttpGet("Create")]
        public async Task<IActionResult> LiquidateAvanceVoyage(AvanceVoyageLiquidationPostDTO avanceVoyageLiquidation)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);
            if (await _avanceVoyageRepository.FindAvanceVoyageByIdAsync(avanceVoyageLiquidation.AvanceVoyageId) == null)
                return BadRequest("AvanceVoyage not found!");

            var tripsDB = await _tripRepository.GetAvanceVoyageTripsByAvIdAsync(avanceVoyageLiquidation.AvanceVoyageId);
            var expensesDB = await _expenseRepository.GetAvanceVoyageExpensesByAvIdAsync(avanceVoyageLiquidation.AvanceVoyageId);

            if (avanceVoyageLiquidation.TripsLiquidations.Count != tripsDB.Count || avanceVoyageLiquidation.ExpensesLiquidations.Count != expensesDB.Count)
                return BadRequest("You must liquidate all expenses and trips!");

            var user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            ServiceResult result = await _liquidationService.LiquidateAvanceVoyage(tripsDB, expensesDB, avanceVoyageLiquidation, user.Id);
            if (!result.Success) return BadRequest($"{result.Message}");

            return Ok(result.Message);
        }


    }
}
