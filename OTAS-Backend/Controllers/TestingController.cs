using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Get;
using OTAS.Interfaces.IRepository;
using OTAS.Models;
using OTAS.Repository;
using OTAS.Services;


/*This Controller is intended for testing purposes, most of these endpoints don't reflect actual
 use cases in the application, and it won't be in the production state of the app.
This is created to check data integrity, get ids and so on 
instead of manually querrying the data from the DB*/

namespace OTAS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TestingController : ControllerBase
    {
        private readonly ITestingRepository _testingRepository;
        private readonly IMapper _mapper;

        public TestingController(ITestingRepository testingRepository, IMapper mapper)
        {
            _testingRepository = testingRepository;
            _mapper = mapper;
        }

        [HttpGet("OrdreMission/All")]
        public async Task<IActionResult> GetAllOrdreMissions()
        {
            var OM_List = await _testingRepository.GetAllOrdreMissionsAsync();
            if(OM_List.Count <= 0) return NotFound("There is no OrdreMission");
            var mappedOM_List = _mapper.Map<List<OrdreMissionDTO>>(OM_List);
            return Ok(mappedOM_List);
        }

        [HttpGet("AvanceVoyage/All")]
        public async Task<IActionResult> GetAllAvanceVoyages()
        {
            var AV_List = await _testingRepository.GetAllAvanceVoyagesAsync();
            if (AV_List.Count <= 0) return NotFound("There is no AvanceVoyage");
            var mappedAV_List = _mapper.Map<List<AvanceVoyageViewDTO>>(AV_List);
            return Ok(mappedAV_List);
        }

        [HttpGet("AvanceCaisse/All")]
        public async Task<IActionResult> GetAllAvanceCaisses()
        {
            var AC_List = await _testingRepository.GetAllAvanceCaissesAsync();
            if (AC_List.Count <= 0) return NotFound("There is no AvanceCaisse");
            var mappedAC_List = _mapper.Map<List<AvanceCaisseDTO>>(AC_List);
            return Ok(mappedAC_List);
        }

        [HttpGet("Expense/All")]
        public async Task<IActionResult> GetAllExpenses()
        {
            var Expenses_List = await _testingRepository.GetAllExpensesAsync();
            if (Expenses_List.Count <= 0) return NotFound("There is no Expense");
            var mappedExpenses_List = _mapper.Map<List<ExpenseDTO>>(Expenses_List);
            return Ok(mappedExpenses_List);
        }

        [HttpGet("Trip/All")]
        public async Task<IActionResult> GetAllTrips()
        {
            var Trips_List = await _testingRepository.GetAllTripsAsync();
            if (Trips_List.Count <= 0) return NotFound("There is no Trip");
            var mappedTrips_List = _mapper.Map<List<TripDTO>>(Trips_List);
            return Ok(mappedTrips_List);
        }

        [HttpGet("StatusHistory/All")]
        public async Task<IActionResult> GetAllStatusHistories()
        {
            var SH_List = await _testingRepository.GetAllStatusHistoriesAsync();
            if (SH_List.Count <= 0) return NotFound("There is no StatusHitory");
            var mappedSH_List = _mapper.Map<List<StatusHistoryDTO>>(SH_List);
            return Ok(mappedSH_List);
        }

        [HttpGet("User/All")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _testingRepository.GetAllUsersAsync();
            List<UserDTO> mappedUsers = _mapper.Map<List<UserDTO>>(users);

            if (users.Count <= 0) return NotFound("No User found");

            return Ok(mappedUsers);
        }

        [HttpPost("User/Add")]
        public async Task<IActionResult> AddUserAsync([FromBody] UserDTO user)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var mappedUser = _mapper.Map<User>(user);
            ServiceResult result = await _testingRepository.AddUserAsync(mappedUser);

            return Ok(result.Message);
        }

    }
}
