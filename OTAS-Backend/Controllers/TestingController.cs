using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Get;
using OTAS.Interfaces.IRepository;


/*This Controller is intended for testing purposes, most of these endpoints don't reflect actual
 use cases in the application, and it won't be in the production state of the app.
This is created to check data integrity, get ids and so on within Postman/Swagger 
instead of manually querrying the data from the DB*/

namespace OTAS.Controllers
{
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
            var OM_List = await _testingRepository.GetAllOrdreMissions();
            if(OM_List.Count <= 0) return NotFound("There is no OrdreMission");
            var mappedOM_List = _mapper.Map<List<OrdreMissionDTO>>(OM_List);
            return Ok(mappedOM_List);
        }

        [HttpGet("AvanceVoyage/All")]
        public async Task<IActionResult> GetAllAvanceVoyages()
        {
            var AV_List = await _testingRepository.GetAllAvanceVoyages();
            if (AV_List.Count <= 0) return NotFound("There is no AvanceVoyage");
            var mappedAV_List = _mapper.Map<List<AvanceVoyageDTO>>(AV_List);
            return Ok(mappedAV_List);
        }

        [HttpGet("Expense/All")]
        public async Task<IActionResult> GetAllExpenses()
        {
            var Expenses_List = await _testingRepository.GetAllExpenses();
            if (Expenses_List.Count <= 0) return NotFound("There is no Expense");
            var mappedExpenses_List = _mapper.Map<List<ExpenseDTO>>(Expenses_List);
            return Ok(mappedExpenses_List);
        }

        [HttpGet("Trip/All")]
        public async Task<IActionResult> GetAllTrips()
        {
            var Trips_List = await _testingRepository.GetAllTrips();
            if (Trips_List.Count <= 0) return NotFound("There is no Trip");
            var mappedTrips_List = _mapper.Map<List<TripDTO>>(Trips_List);
            return Ok(mappedTrips_List);
        }

        [HttpGet("StatusHistory/All")]
        public async Task<IActionResult> GetAllStatusHistories()
        {
            var SH_List = await _testingRepository.GetAllStatusHistories();
            if (SH_List.Count <= 0) return NotFound("There is no StatusHitory");
            var mappedSH_List = _mapper.Map<List<StatusHistoryDTO>>(SH_List);
            return Ok(mappedSH_List);
        }

    }
}
