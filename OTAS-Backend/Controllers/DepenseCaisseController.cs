using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Repository;
using OTAS.Services;

namespace OTAS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepenseCaisseController : ControllerBase
    {
        private readonly IDepenseCaisseService _depenseCaisseService;
        private readonly IDepenseCaisseRepository _depenseCaisseRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public DepenseCaisseController(IDepenseCaisseService depenseCaisseService,
            IDepenseCaisseRepository depenseCaisseRepository,
            IWebHostEnvironment webHostEnvironment,
            IUserRepository userRepository,
            IMapper mapper)
        {
            _depenseCaisseService = depenseCaisseService;
            _depenseCaisseRepository = depenseCaisseRepository;
            _webHostEnvironment = webHostEnvironment;
            _userRepository = userRepository;
            _mapper = mapper;
        }


        //Requester
        [HttpGet("Requests/Table")]
        public async Task<IActionResult> ShowDepenseCaisseRequestsTable(int userId)
        {
            if (await _userRepository.FindUserByUserIdAsync(userId) == null) return BadRequest("User not found!");
            var depenseCaisses = await _depenseCaisseRepository.GetDepensesCaisseByUserIdAsync(userId);
            if (depenseCaisses == null || depenseCaisses.Count <= 0) return NotFound("You haven't placed any \"DepenseCaisse\" yet!");

            var mappedOrdreMissions = _mapper.Map<List<DepenseCaisseDTO>>(depenseCaisses);

            return Ok(mappedOrdreMissions);

        }

        //Requester
        [HttpPost("Create")]
        public async Task<IActionResult> AddDepenseCaisse([FromBody] DepenseCaissePostDTO depenseCaisse)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            string receiptsFilePath;

            if (depenseCaisse.ReceiptsFile == null || depenseCaisse.ReceiptsFile.Length == 0)
            {
                return BadRequest("No file uploaded");
            }
            else
            {
                try
                {
                    // _webHostEnvironment.WebRootPath == wwwroot\ (the default folder to store files)
                    var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Depense-Caisse");

                    if (!Directory.Exists(uploadsFolderPath))
                    {
                        Directory.CreateDirectory(uploadsFolderPath);
                    }
                    var uniqueReceiptsFileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_DC_" + depenseCaisse.UserId + ".pdf";
                    var filePath = Path.Combine(uploadsFolderPath, uniqueReceiptsFileName);
                    await System.IO.File.WriteAllBytesAsync(filePath, depenseCaisse.ReceiptsFile);
                    receiptsFilePath = filePath;
                }
                catch (Exception ex)
                {
                    return BadRequest($"ERROR: {ex.Message} |||||||||| {ex.Source} |||||||||| {ex.InnerException}");
                }
            }
            
            //File uploaded and path assigned now deal with the json

            ServiceResult result = await _depenseCaisseService.AddDepenseCaisse(depenseCaisse, receiptsFilePath);
            if (!result.Success) return BadRequest($"{result.Message}");

            return Ok(result.Message);
        }

        ////Requester
        [HttpPut("Modify")]
        public async Task<IActionResult> ModifyDepenseCaisse([FromBody] DepenseCaissePostDTO depenseCaisse, [FromQuery] int action)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _depenseCaisseRepository.FindDepenseCaisseAsync(depenseCaisse.Id) == null) return NotFound("DC is not found");

            bool isActionValid = action == 99 || action == 1;
            if (!isActionValid) return BadRequest("Action is invalid! If you are seeing this error, you are probably trying to manipulate the system. If not, please report the IT department with the issue.");

            if (action == 99 && (depenseCaisse.LatestStatus == 98 || depenseCaisse.LatestStatus == 97)) return BadRequest("You cannot save a returned or a rejected request as a draft!");

            ServiceResult result = await _depenseCaisseService.ModifyDepenseCaisse(depenseCaisse, action);

            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }

        //Requester
        [HttpPut("Submit")]
        public async Task<IActionResult> Submit(int depenseCaisseId)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _depenseCaisseRepository.FindDepenseCaisseAsync(depenseCaisseId) == null) return NotFound("DepenseCaisse is not found!");

            ServiceResult result = await _depenseCaisseService.SubmitDepenseCaisse(depenseCaisseId);
            if (!result.Success) return BadRequest(result.Message);

            return Ok(result.Message);
        }



        ////Requester
        //[HttpGet("{ordreMissionId}/View")]

        //// Decider
        //[HttpGet("DecideOnRequests/Table")]

        ////Decider
        //[HttpPut("Decide")]

    }
}
