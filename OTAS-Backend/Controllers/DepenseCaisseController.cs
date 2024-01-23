using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.DTO.Put;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using OTAS.Repository;
using OTAS.Services;
using System.Net;
using System.Net.Http.Headers;

namespace OTAS.Controllers
{
    [Authorize]
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


        [Authorize(Roles = "requester,decider")]
        [HttpGet("Requests/Table")]
        public async Task<IActionResult> ShowDepenseCaisseRequestsTable()
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);

            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");
            var depenseCaisses = await _depenseCaisseRepository.GetDepensesCaisseByUserIdAsync(user.Id);

            var mappedDepenseCaisses = _mapper.Map<List<DepenseCaisseDTO>>(depenseCaisses);

            return Ok(mappedDepenseCaisses);

        }

        [Authorize(Roles = "requester,decider")]
        [HttpPost("Create")]
        public async Task<IActionResult> AddDepenseCaisse([FromBody] DepenseCaissePostDTO depenseCaisse)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (depenseCaisse.ReceiptsFile == null || depenseCaisse.ReceiptsFile.Length == 0 || depenseCaisse.ReceiptsFile.ToString() == "") 
                return BadRequest("No file uploaded!");

            ServiceResult result = new();

            if (depenseCaisse.Expenses.Count < 1)
            {
                result.Success = false;
                result.Message = "DepenseCaisse must contain at least one expense! You can't request a DepenseCaisse with no expense.";
                return BadRequest(result);
            }
            if (depenseCaisse.Currency != "MAD" && depenseCaisse.Currency != "EUR")
            {
                result.Success = false;
                result.Message = "Invalid currency! Please choose suitable currency from the dropdownmenu!";
                return BadRequest(result);
            }
            if (depenseCaisse.OnBehalf == true && depenseCaisse.ActualRequester == null)
            {
                result.Success = false;
                result.Message = "You must fill actual requester's info in case you are filing this request on behalf of someone";
                return BadRequest(result);
            }

            var user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            result = await _depenseCaisseService.AddDepenseCaisse(depenseCaisse, user.Id);

            if (!result.Success) return BadRequest($"{result.Message}");
            return Ok(result.Message);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("ReceiptsFile/Download")]
        public IActionResult DownloadDepenseCaisseReceiptsFile(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath + "\\Depense-Caisse-Receipts", fileName);
           
                if (System.IO.File.Exists(filePath))
                {
                    HttpResponseMessage response = new(HttpStatusCode.OK);
                    using var fileStream = new FileStream(filePath, FileMode.Open);
                    // Put the file into the response
                    response.Content = new StreamContent(fileStream);
                    response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = fileName
                    };
                    response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    return Ok(response);
                }
                else
                {
                    return NotFound("File is not found! It may have been deleted by mistake");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "requester,decider")]
        [HttpPut("Modify")]
        public async Task<IActionResult> ModifyDepenseCaisse([FromBody] DepenseCaissePutDTO depenseCaisse)
        {
            // REQUEST VALIDATIONS
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _depenseCaisseRepository.FindDepenseCaisseAsync(depenseCaisse.Id) == null) return NotFound("DC is not found");

            bool isActionValid = depenseCaisse.Action.ToLower() == "save" || depenseCaisse.Action.ToLower() == "submit";
            if (!isActionValid) return BadRequest("Action is invalid! If you are seeing this error, you are probably trying to manipulate the system. If not, please report the IT department with the issue.");

            DepenseCaisse depenseCaisse_DB = await _depenseCaisseRepository.GetDepenseCaisseByIdAsync(depenseCaisse.Id);
            if (depenseCaisse.Action.ToLower() == "save" && (depenseCaisse_DB.LatestStatus == 98 || depenseCaisse_DB.LatestStatus == 97)) 
                return BadRequest("You cannot save a returned or a rejected request as a draft!");
            ServiceResult result = new();
            if (depenseCaisse_DB.LatestStatus == 97)
            {
                result.Success = false;
                result.Message = "You can't modify or resubmit a rejected request!";
                return BadRequest(result);
            }
            if (depenseCaisse_DB.LatestStatus != 98 && depenseCaisse_DB.LatestStatus != 99)
            {
                result.Success = false;
                result.Message = "The DP you are trying to modify is not a draft nor returned! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks";
                return BadRequest(result);
            }
            if (depenseCaisse.Action.ToLower() == "save" && depenseCaisse_DB.LatestStatus == 98)
            {
                result.Success = false;
                result.Message = "You can't modify and save a returned request as a draft again. You may want to apply your modifications to you request and resubmit it directly";
                return BadRequest(result);
            }
            if (depenseCaisse.Expenses.Count < 1)
            {
                result.Success = false;
                result.Message = "DepenseCaisse must have at least one expense! You can't request a DP with no expense.";
                return BadRequest(result);
            }
            if (depenseCaisse.OnBehalf == true && depenseCaisse.ActualRequester == null)
            {
                result.Success = false;
                result.Message = "You must fill actual requester's info in case you are filing this request on behalf of someone";
                return BadRequest(result);
            }


            result = await _depenseCaisseService.ModifyDepenseCaisse(depenseCaisse);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpPut("Submit")]
        public async Task<IActionResult> SubmitDepenseCaisse(int Id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _depenseCaisseRepository.FindDepenseCaisseAsync(Id) == null) return NotFound("DepenseCaisse is not found!");
            ServiceResult result = new();
            var testAC = await _depenseCaisseRepository.GetDepenseCaisseByIdAsync(Id);
            if (testAC.LatestStatus == 1)
            {
                result.Success = false;
                result.Message = "You've already submitted this DepenseCaisse!";
                return BadRequest(result);
            }
            result = await _depenseCaisseService.SubmitDepenseCaisse(Id);
            if (!result.Success) return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteDraftedDepenseCaisse(int Id) // Id = depenseCaisseId
        {
            ServiceResult result = new();
            if (await _depenseCaisseRepository.FindDepenseCaisseAsync(Id) == null) return NotFound("DepenseCaisse not found!");
            DepenseCaisse depenseCaisse = await _depenseCaisseRepository.GetDepenseCaisseByIdAsync(Id);

            if (depenseCaisse.LatestStatus != 99)
            {
                result.Success = false;
                result.Message = "The request is not in a draft status. You cannot delete requests in a different status than draft.";
                return BadRequest(result);
            }

            

            result = await _depenseCaisseService.DeleteDraftedDepenseCaisse(depenseCaisse);
            if (!result.Success) return BadRequest(result.Message);
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);

            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");
            var depenseCaisses = await _depenseCaisseRepository.GetDepensesCaisseByUserIdAsync(user.Id);

            var mappedDepenseCaisses = _mapper.Map<List<DepenseCaisseDTO>>(depenseCaisses);

            return Ok(mappedDepenseCaisses);
        }

        [Authorize(Roles = "decider")]
        [HttpGet("DecideOnRequests/Table")]
        public async Task<IActionResult> ShowDepenseCaisseDecideTable()
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");

            List<DepenseCaisseDTO> DCs = await _depenseCaisseRepository.GetDepenseCaissesForDeciderTable(user.Id);

            return Ok(DCs);
        }


        [Authorize(Roles = "decider")]
        [HttpPut("Decide")]
        public async Task<IActionResult> DecideOnDepenseCaisse(DecisionOnRequestPostDTO decision)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            if (await _depenseCaisseRepository.FindDepenseCaisseAsync(decision.RequestId) == null) 
                return NotFound("DepenseCaisse is not found!");
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) 
                return NotFound("Decider is not found");

            bool isDecisionValid = decision.DecisionString.ToLower() != "apSprove" && decision.DecisionString.ToLower() != "return" && decision.DecisionString.ToLower() != "reject";
            if (!isDecisionValid) return BadRequest("Decision is invalid!");

            
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) 
                return BadRequest("User not found!");

            int deciderRole = await _userRepository.GetUserRoleByUserIdAsync(user.Id);
            if (deciderRole != 3) 
                return BadRequest("You are not authorized to decide upon requests!");

            ServiceResult result = await _depenseCaisseService.DecideOnDepenseCaisse(decision, user.Id);
            if (!result.Success) 
                return BadRequest(result.Message);
            return Ok(result.Message);
        }

    }
}
