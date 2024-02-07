using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IUserRepository _userRepository;

        public LiquidationController(ILiquidationRepository liquidationRepository,
            IActualRequesterRepository actualRequesterRepository,
            ILiquidationService liquidationService,
            IAvanceVoyageRepository avanceVoyageRepository,
            IAvanceCaisseRepository avanceCaisseRepository,
            ITripRepository tripRepository,
            IWebHostEnvironment webHostEnvironment,
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
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
            _expenseRepository = expenseRepository;
            _userRepository = userRepository;
        }

        // LIQUIDATE ACTIONS
        [Authorize(Roles = "requester,decider")]
        [HttpPost("Create")]
        public async Task<IActionResult> CreateLiquidation(LiquidationPostDTO liquidation)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            ServiceResult result;

            if (liquidation.RequestType == "AV")
            {
                if (await _avanceVoyageRepository.FindAvanceVoyageByIdAsync(liquidation.RequestId) == null)
                    return BadRequest("AvanceVoyage not found!");
                var avanceVoyageDB = await _avanceVoyageRepository.GetAvanceVoyageByIdAsync(liquidation.RequestId);
                if (avanceVoyageDB.LatestStatus != 10)
                    return BadRequest("You cannot lequidate this request on this state!");

                var tripsDB = await _tripRepository.GetAvanceVoyageTripsByAvIdAsync(liquidation.RequestId); 
                var expensesDB = await _expenseRepository.GetAvanceVoyageExpensesByAvIdAsync(liquidation.RequestId);

                if (liquidation.TripsLiquidations.Count != tripsDB.Count || liquidation.ExpensesLiquidations.Count != expensesDB.Count)
                    return BadRequest("You must liquidate all expenses and trips!");

                result = await _liquidationService.LiquidateAvanceVoyage(avanceVoyageDB, liquidation, user.Id);
                if (!result.Success) return BadRequest($"{result.Message}");
            }
            else
            {
                if (await _avanceCaisseRepository.FindAvanceCaisseAsync(liquidation.RequestId) == null)
                    return BadRequest("AvanceVoyage not found!");

                var avanceCaisseDB = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync(liquidation.RequestId);
                if (avanceCaisseDB.LatestStatus != 10)
                    return BadRequest("You cannot lequidate this request on this state!");

                var expensesDB = await _expenseRepository.GetAvanceCaisseExpensesByAcIdAsync(liquidation.RequestId);

                if (liquidation.ExpensesLiquidations.Count != expensesDB.Count)
                    return BadRequest("You must liquidate all expenses!");

                result = await _liquidationService.LiquidateAvanceCaisse(avanceCaisseDB, liquidation, user.Id);
                if (!result.Success) return BadRequest($"{result.Message}");

            }

            return Ok(result.Id);
        }

        [Authorize(Roles = "requester , decider")]
        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteDraftedLiquidation(int Id) // Id = depenseCaisseId
        {
            ServiceResult result = new();
            
            Liquidation? liquidation = await _liquidationRepository.FindLiquidationAsync(Id);
            if (liquidation == null) return NotFound("Liquidation not found!");

            if (liquidation.LatestStatus != 99)
            {
                result.Success = false;
                result.Message = "The request is not in a draft status. You cannot delete requests in a different status than draft.";
                return BadRequest(result);
            }

            result = await _liquidationService.DeleteDraftedLiquidation(liquidation);
            if (!result.Success) return BadRequest(result.Message);
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);

            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");

            var liquidations = await _liquidationRepository.GetLiquidationsTableForRequster(user.Id);
            return Ok(liquidations);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("View")]
        public async Task<IActionResult> ShowLiquidationDetailsPage(int Id)
        {
            if (await _liquidationRepository.FindLiquidationAsync(Id) == null) return NotFound("Liquidation is not found");
            LiquidationViewDTO liquidationDetails = await _liquidationRepository.GetLiquidationFullDetailsByIdForRequester(Id);

            if(liquidationDetails.RequestType == "AV")
            {
                liquidationDetails.RequestDetails =
                    await _avanceVoyageRepository.GetAvanceVoyageDetailsForLiquidationAsync(liquidationDetails.RequestId);


                var av = await _avanceVoyageRepository.GetAvanceVoyageByIdAsync(liquidationDetails.RequestId);

                // if the request is on behalf of someone, get the requester data from the DB
                if (liquidationDetails.OnBehalf)
                {
                    ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByOrdreMissionIdAsync(av.OrdreMissionId);
                    liquidationDetails.RequesterInfo = _mapper.Map<ActualRequesterDTO>(actualRequesterInfo);
                    liquidationDetails.RequesterInfo.ManagerUserName = await _userRepository.GetUsernameByUserIdAsync(actualRequesterInfo.ManagerUserId);
                }
                try
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath + "\\liquidation-Avance-Voyage-Receipts", liquidationDetails.ReceiptsFileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        liquidationDetails.ReceiptsFile = System.IO.File.ReadAllBytes(filePath);
                    }
                    else
                    {
                        liquidationDetails.ReceiptsFile = Array.Empty<byte>();
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest("Something went wrong while trying to retrieve the file" + ex.Message);
                }
            }
            else
            {
                liquidationDetails.RequestDetails =
                    await _avanceCaisseRepository.GetAvanceCaisseDetailsForLiquidationAsync(liquidationDetails.RequestId);

                // if the request is on behalf of someone, get the requester data from the DB
                if (liquidationDetails.OnBehalf)
                {
                    ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByAvanceCaisseIdAsync(liquidationDetails.RequestId);
                    liquidationDetails.RequesterInfo = _mapper.Map<ActualRequesterDTO>(actualRequesterInfo);
                    liquidationDetails.RequesterInfo.ManagerUserName = await _userRepository.GetUsernameByUserIdAsync(actualRequesterInfo.ManagerUserId);
                }

                try
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath + "\\liquidation-Avance-Caisse-Receipts", liquidationDetails.ReceiptsFileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        liquidationDetails.ReceiptsFile = System.IO.File.ReadAllBytes(filePath);

                    }
                    else
                    {
                        liquidationDetails.ReceiptsFile = Array.Empty<byte>();
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest("Something went wrong while trying to retrieve the file" + ex.Message);
                }
            }

            

            // adding those more detailed status that are not in the DB
            for (int i = liquidationDetails.StatusHistory.Count - 1; i >= 0; i--)
            {
                StatusHistoryDTO statusHistory = liquidationDetails.StatusHistory[i];
                StatusHistoryDTO explicitStatusHistory = new();
                switch (statusHistory.Status)
                {
                    case "Pending Manager's Approval":
                        explicitStatusHistory.Status = "Submitted";
                        explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                        break;
                    case "Pending HR's Approval":
                        explicitStatusHistory.Status = "Approved";
                        explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                        explicitStatusHistory.DeciderFirstName = statusHistory.DeciderFirstName;
                        explicitStatusHistory.DeciderLastName = statusHistory.DeciderLastName;
                        break;
                    case "Pending Finance Department's Approval":
                        explicitStatusHistory.Status = "Approved";
                        explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                        explicitStatusHistory.DeciderFirstName = statusHistory.DeciderFirstName;
                        explicitStatusHistory.DeciderLastName = statusHistory.DeciderLastName;
                        break;
                    case "Pending General Director's Approval":
                        explicitStatusHistory.Status = "Approved";
                        explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                        explicitStatusHistory.DeciderFirstName = statusHistory.DeciderFirstName;
                        explicitStatusHistory.DeciderLastName = statusHistory.DeciderLastName;
                        break;
                    case "Pending Vice President's Approval":
                        explicitStatusHistory.Status = "Approved";
                        explicitStatusHistory.CreateDate = statusHistory.CreateDate;
                        explicitStatusHistory.DeciderFirstName = statusHistory.DeciderFirstName;
                        explicitStatusHistory.DeciderLastName = statusHistory.DeciderLastName;
                        break;
                    default:
                        continue;
                }
                liquidationDetails.StatusHistory.Insert(i, explicitStatusHistory);
            }

            return Ok(liquidationDetails);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("ReceiptsFile/Download")]
        public IActionResult DownloadDepenseCaisseReceiptsFile(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath + "\\Liquidation-Avance-Voyage-Receipts", fileName);
                string filePath2 = Path.Combine(_webHostEnvironment.WebRootPath + "\\Liquidation-Avance-Caisse-Receipts", fileName);

                if (System.IO.File.Exists(filePath))
                {
                    byte[] base64String = System.IO.File.ReadAllBytes(filePath);

                    Response.Headers.Add("Content-Disposition", $"attachment; filename={fileName}");

                    return Ok(File(base64String, "application/pdf", fileName));
                }
                else if (System.IO.File.Exists(filePath2))
                {
                    byte[] base64String = System.IO.File.ReadAllBytes(filePath2);

                    Response.Headers.Add("Content-Disposition", $"attachment; filename={fileName}");

                    return Ok(File(base64String, "application/pdf", fileName));
                }
                else
                {
                    return NotFound("File is not found! It may have been moved or deleted");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }



        // REQS THAT MEANT TO BE LIQUIDATED
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
