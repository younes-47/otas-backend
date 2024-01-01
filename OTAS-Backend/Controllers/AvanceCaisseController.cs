using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.DTO.Put;
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
    public class AvanceCaisseController : ControllerBase
    {
        private readonly IAvanceCaisseRepository _avanceCaisseRepository;
        private readonly IAvanceCaisseService _avanceCaisseService;
        private readonly IUserRepository _userRepository;
        private readonly IDeciderRepository _deciderRepository;
        private readonly IMapper _mapper;

        public AvanceCaisseController(IAvanceCaisseRepository avanceCaisseRepository,
            IAvanceCaisseService avanceCaisseService,
            IUserRepository userRepository,
            IDeciderRepository deciderRepository,
            IMapper mapper)
        {
            _avanceCaisseRepository = avanceCaisseRepository;
            _avanceCaisseService = avanceCaisseService;
            _userRepository = userRepository;
            _deciderRepository = deciderRepository;
            _mapper = mapper;
        }

        /*
         
        REQUESTER
         
         */

        [Authorize(Roles = "requester , decider")]
        [HttpGet("Requests/Table")]
        public async Task<IActionResult> ShowAvanceCaisseRequestsTable()
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");
            var mappedACs = await _avanceCaisseRepository.GetAvancesCaisseByUserIdAsync(user.Id);
            return Ok(mappedACs);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpPost("Create")]
        public async Task<IActionResult> AddAvanceCaisse([FromBody] AvanceCaissePostDTO avanceCaisse)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            ServiceResult result = new();
            if (avanceCaisse.Expenses.Count < 1)
            {
                result.Success = false;
                result.Message = "AvanceCaisse must have at least one expense! You can't request an AvanceCaisse with no expense.";
                return BadRequest(result);
            }
            if (avanceCaisse.Currency != "MAD" && avanceCaisse.Currency != "EUR")
            {
                result.Success = false;
                result.Message = "Invalid currency! Please choose suitable currency from the dropdownmenu!";
                return BadRequest(result);
            }
            if (avanceCaisse.OnBehalf == true && avanceCaisse.ActualRequester == null)
            {
                result.Success = false;
                result.Message = "You must fill actual requester's info in case you are filing this request on behalf of someone";
                return BadRequest(result);
            }
            var user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            result = await _avanceCaisseService.CreateAvanceCaisseAsync(avanceCaisse, user.Id);
            if (!result.Success) return Ok(result.Message);
            return Ok(result.Message);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpPut("Modify")]
        public async Task<IActionResult> ModifyAvanceCaisse([FromBody] AvanceCaissePutDTO avanceCaisse)
        {
            // VALIDATIONS
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _avanceCaisseRepository.FindAvanceCaisseAsync(avanceCaisse.Id) == null) 
                return NotFound("AvanceCaisse is not found");

            bool isActionValid = avanceCaisse.Action.ToLower() == "submit" || avanceCaisse.Action.ToLower() == "save";
            if (!isActionValid) 
                return BadRequest("Action is invalid! If you are seeing this error, you are probably trying to manipulate the system. If not, please report the IT department with the issue.");

            var avanceCaisse_DB = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync(avanceCaisse.Id);

            if (avanceCaisse.Action.ToLower() == "save" && avanceCaisse_DB.LatestStatus == 98) 
                return BadRequest("You cannot modify and save a returned request as a draft!");
            if (avanceCaisse_DB.LatestStatus != 98 && avanceCaisse_DB.LatestStatus != 99) 
                return BadRequest("The AvanceCaisse is not modifiable in this state! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system.");
            if (avanceCaisse.Expenses.Count < 1) 
                return BadRequest("AvanceCaisse must have at least one expense! You can't request a Cash advance with no expense.");
            if (avanceCaisse.OnBehalf == true && avanceCaisse.ActualRequester == null)
                return BadRequest("You must fill actual requester's info in case you are filing this request on behalf of someone");


            ServiceResult result = await _avanceCaisseService.ModifyAvanceCaisse(avanceCaisse);

            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpPut("Submit")]
        public async Task<IActionResult> SubmitAvanceCaisse(int Id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _avanceCaisseRepository.FindAvanceCaisseAsync(Id) == null) return NotFound("AvanceCaisse is not found!");
       
            AvanceCaisse avanceCaisse_DB = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync(Id);
            if (avanceCaisse_DB.LatestStatus != 99)
                return BadRequest("Action denied! The request is not in a suitable state where you can submit it.");
            if (avanceCaisse_DB.LatestStatus == 1)
                return BadRequest("You've already submitted this request!");

            ServiceResult result = await _avanceCaisseService.SubmitAvanceCaisse(avanceCaisse_DB);
            if (!result.Success) return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpGet("View")]
        public async Task<IActionResult> GetAvanceCaisseById(int Id)
        {
            if (await _avanceCaisseRepository.FindAvanceCaisseAsync(Id) == null) return NotFound("AvanceCaisse is not found");
            var avanceCaisseDetails = await _avanceCaisseService.GetAvanceCaisseFullDetailsById(Id);

            return Ok(avanceCaisseDetails);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteDraftedAvanceCaisse(int Id) // Id = avanceCaisseId
        {
            ServiceResult result = new();
            if (await _avanceCaisseRepository.FindAvanceCaisseAsync(Id) == null) return NotFound("AvanceCaisse not found!");
            AvanceCaisse avanceCaisse = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync(Id);

            if (avanceCaisse.LatestStatus != 99)
            {
                result.Success = false;
                result.Message = "The request is not in a draft. You cannot delete requests in a different status than draft.";
                return BadRequest(result);
            }

            result = await _avanceCaisseService.DeleteDraftedAvanceCaisse(avanceCaisse);
            return Ok(result.Message);
        }

        /*
         
        DECIDER
         
         */

        [Authorize(Roles = "decider")]
        [HttpGet("DecideOnRequests/Table")]
        public async Task<IActionResult> ShowAvanceCaisseDecideTable()
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");

            List<AvanceCaisseDTO> ACs = await _avanceCaisseRepository.GetAvanceCaissesForDeciderTable(user.Id);

            return Ok(ACs);
        }

        [Authorize(Roles = "decider")]
        [HttpPut("Decide")]
        public async Task<IActionResult> DecideOnAvanceCaisse(DecisionOnRequestPostDTO decision)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);
            if (await _avanceCaisseRepository.FindAvanceCaisseAsync(decision.RequestId) == null) 
                return NotFound("AvanceCaisse is not found!");
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);

            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) 
                return NotFound("Decider is not found");
            bool isDecisionValid = decision.DecisionString.ToLower() != "approve" && decision.DecisionString.ToLower() != "return" && decision.DecisionString.ToLower() != "reject";
            if (!isDecisionValid) 
                return BadRequest("Decision is invalid!");

            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) 
                return BadRequest("User not found!");

            if (await _userRepository.GetUserRoleByUserIdAsync(user.Id) != 3) 
                return BadRequest("You are not authorized to decide upon requests!");

            ServiceResult result = await _avanceCaisseService.DecideOnAvanceCaisse(decision, user.Id);
            if (!result.Success) 
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [Authorize(Roles = "decider")]
        [HttpPut("Decide/Funds/Choose")]
        public async Task<IActionResult> MarkFundsAsPrepared(MarkFundsAsPreparedPutDTO action) // Only TR
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if(await _userRepository.FindUserByUserIdAsync(user.Id) == null)
                return BadRequest("User not found!");

            if (await _userRepository.GetUserRoleByUserIdAsync(user.Id) != 3)
                return BadRequest("You are not authorized to decide upon requests!");

            if(await _deciderRepository.GetDeciderLevelByUserId(user.Id) != "TR")
                return BadRequest("You are not authorized to do this action!");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //if (advanceOption.ToLower() != "" && advanceOption.ToLower() != "")
            //    return BadRequest("Invalid Advance choice!");

            if (await _avanceCaisseRepository.FindAvanceCaisseAsync(action.RequestId) == null)
                return BadRequest("Request is not found");

            ServiceResult result = await _avanceCaisseService.MarkFundsAsPrepared(action.RequestId, action.AdvanceOption, user.Id);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);

        }

        [Authorize(Roles = "decider")]
        [HttpPut("Decide/Funds/Confirm")]
        public async Task<IActionResult> ConfirmFundsDelivery(ConfirmFundsDeliveryPutDTO action) // Only TR
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null)
                return BadRequest("User not found!");

            if (await _userRepository.GetUserRoleByUserIdAsync(user.Id) != 3)
                return BadRequest("You are not authorized to decide upon requests!");

            if (await _deciderRepository.GetDeciderLevelByUserId(user.Id) != "TR")
                return BadRequest("You are not authorized to do this action!");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _avanceCaisseRepository.FindAvanceCaisseAsync(action.RequestId) == null)
                return BadRequest("Request is not found");

            ServiceResult result = await _avanceCaisseService.ConfirmFundsDelivery(action.RequestId, action.ConfirmationNumber);
            if (!result.Success)
                return Forbid(result.Message);

            return Ok(result.Message);
        }


    }
}
