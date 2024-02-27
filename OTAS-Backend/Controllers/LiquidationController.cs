using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Office.Interop.Word;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.DTO.Put;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using OTAS.Repository;
using OTAS.Services;
using TMA_App.Services;

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
        private readonly ILdapAuthenticationService _ldapAuthenticationService;
        private readonly IMiscService _miscService;
        private readonly IMapper _mapper;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDeciderRepository _deciderRepository;

        public LiquidationController(ILiquidationRepository liquidationRepository,
            IActualRequesterRepository actualRequesterRepository,
            ILiquidationService liquidationService,
            IAvanceVoyageRepository avanceVoyageRepository,
            IAvanceCaisseRepository avanceCaisseRepository,
            ITripRepository tripRepository,
            IWebHostEnvironment webHostEnvironment,
            ILdapAuthenticationService ldapAuthenticationService,
            IMiscService miscService,
            IMapper mapper,
            IExpenseRepository expenseRepository,
            IUserRepository userRepository,
            IDeciderRepository deciderRepository)
        {
            _liquidationRepository = liquidationRepository;
            _actualRequesterRepository = actualRequesterRepository;
            _liquidationService = liquidationService;
            _avanceVoyageRepository = avanceVoyageRepository;
            _avanceCaisseRepository = avanceCaisseRepository;
            _tripRepository = tripRepository;
            _webHostEnvironment = webHostEnvironment;
            _ldapAuthenticationService = ldapAuthenticationService;
            _miscService = miscService;
            _mapper = mapper;
            _expenseRepository = expenseRepository;
            _userRepository = userRepository;
            _deciderRepository = deciderRepository;
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
        public async Task<IActionResult> DeleteDraftedLiquidation(int Id) // Id = liquidationId
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
    
                // if the request is on behalf of someone, get the requester data from the DB
                if (liquidationDetails.OnBehalf)
                {
                    var av = await _avanceVoyageRepository.GetAvanceVoyageByIdAsync(liquidationDetails.RequestId);
                    ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByOrdreMissionIdAsync(av.OrdreMissionId);
                    liquidationDetails.RequestDetails.ActualRequester = _mapper.Map<ActualRequesterDTO>(actualRequesterInfo);
                    liquidationDetails.RequestDetails.ActualRequester.ManagerUserName = await _userRepository.GetUsernameByUserIdAsync(actualRequesterInfo.ManagerUserId);
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
                    liquidationDetails.RequestDetails.ActualRequester = _mapper.Map<ActualRequesterDTO>(actualRequesterInfo);
                    liquidationDetails.RequestDetails.ActualRequester.ManagerUserName = await _userRepository.GetUsernameByUserIdAsync(actualRequesterInfo.ManagerUserId);
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

            // adding more detailed status that are not in the DB
            liquidationDetails.StatusHistory = _miscService.IllustrateStatusHistory(liquidationDetails.StatusHistory);

            return Ok(liquidationDetails);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("ReceiptsFile/Download")]
        public IActionResult DownloadLiquidationReceiptsFile(string fileName)
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

        [Authorize(Roles = "requester,decider")]
        [HttpGet("Document/Download")]
        public async Task<IActionResult> DownloadLiquidationDocument(int Id)
        {
            Liquidation? liquidation = await _liquidationRepository.FindLiquidationAsync(Id);
            if (liquidation == null)
                return NotFound("Request Not Found");

            string docxFileName;
            if (liquidation.AvanceCaisseId != null)
            {
                docxFileName = await _liquidationService.GenerateAvanceCaisseLiquidationWordDocument(Id);
            }
            else
            {
                docxFileName = await _liquidationService.GenerateAvanceVoyageLiquidationWordDocument(Id);
            }
            var tempDir = Path.Combine(_webHostEnvironment.WebRootPath, "Temp-Files");
            var docxFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "Temp-Files", docxFileName + ".docx");
            Guid tempPdfName = Guid.NewGuid();
            var pdfFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "Temp-Files", tempPdfName.ToString() + ".pdf");

            Application wordApplication = new Application();
            Microsoft.Office.Interop.Word.Document wordDocument = wordApplication.Documents.Open(docxFilePath);
            wordDocument.ExportAsFixedFormat(pdfFilePath, WdExportFormat.wdExportFormatPDF);
            wordDocument.Close(false);
            wordApplication.Quit();

            byte[] base64String = System.IO.File.ReadAllBytes(pdfFilePath);


            /* Delete temp files */
            System.IO.File.Delete(docxFilePath);
            System.IO.File.Delete(pdfFilePath);

            Response.Headers.Add($"Content-Disposition", $"attachment; filename=LIQUIDATION_{Id}_DOCUMENT.pdf");

            return Ok(File(base64String, "application/pdf", $"LIQUIDATION_{Id}_DOCUMENT.pdf"));

        }


        [Authorize(Roles = "requester,decider")]
        [HttpPut("Modify")]
        public async Task<IActionResult> ModifyLiquidation([FromBody] LiquidationPutDTO liquidation)
        {
            // REQUEST VALIDATIONS
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _liquidationRepository.FindLiquidationAsync(liquidation.Id) == null) return NotFound("Request is not found");

            bool isActionValid = liquidation.Action.ToLower() == "save" || liquidation.Action.ToLower() == "submit";
            if (!isActionValid) return BadRequest("Action is invalid! If you are seeing this error, you are probably trying to manipulate the system. If not, please report the IT department with the issue.");

            Liquidation liquidation_DB = await _liquidationRepository.GetLiquidationByIdAsync(liquidation.Id);

            if (liquidation.Action.ToLower() == "save" && (liquidation_DB.LatestStatus == 98 || liquidation_DB.LatestStatus == 97))
                return BadRequest("You cannot save a returned or a rejected request as a draft!");

            ServiceResult result = new();

            if (liquidation_DB.LatestStatus == 97)
            {
                result.Success = false;
                result.Message = "You can't modify or resubmit a rejected request!";
                return BadRequest(result);
            }
            if (liquidation_DB.LatestStatus != 98 && liquidation_DB.LatestStatus != 99 && liquidation_DB.LatestStatus != 15)
            {
                result.Success = false;
                result.Message = "The DP you are trying to modify is not a draft nor returned! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks";
                return BadRequest(result);
            }
            if (liquidation.Action.ToLower() == "save" && liquidation_DB.LatestStatus == 98)
            {
                result.Success = false;
                result.Message = "You can't modify and save a returned request as a draft again. You may want to apply your modifications to you request and resubmit it directly";
                return BadRequest(result);
            }

            if(liquidation.RequestType == "AV")
            {
                var db_expenses = await _expenseRepository.GetAvanceVoyageExpensesByAvIdAsync(liquidation.RequestId);
                List<Expense> required_to_liquidate_expenses = new();
                foreach(var expense in db_expenses)
                {
                    if(expense.EstimatedFee > 0)
                    {
                        required_to_liquidate_expenses.Add(expense);
                    }
                }
                if (liquidation.ExpensesLiquidations.Count != required_to_liquidate_expenses.Count)
                    return BadRequest("Not All expenses are liquidated! You must liquidate all of the expenses you previously mentioned");
                foreach(var expense in required_to_liquidate_expenses)
                {
                    bool isExistant = liquidation.ExpensesLiquidations.Any(ex => ex.ExpenseId == expense.Id);
                    if (!isExistant)
                        return BadRequest("One of the expenses related to the request is not liquidated!");
                }

                var db_trips = await _tripRepository.GetAvanceVoyageTripsByAvIdAsync(liquidation.RequestId);
                List<Trip> required_to_liquidate_trips = new();
                foreach (var trip in db_trips)
                {
                    if (trip.EstimatedFee > 0)
                    {
                        required_to_liquidate_trips.Add(trip);
                    }
                }
                if (liquidation.TripsLiquidations.Count != required_to_liquidate_trips.Count)
                    return BadRequest("Not All trips are liquidated! You must liquidate all of the trips you previously mentioned");
                foreach (var trip in required_to_liquidate_trips)
                {
                    bool isExistant = liquidation.TripsLiquidations.Any(ex => ex.TripId == trip.Id);
                    if (!isExistant)
                        return BadRequest("One of the trips related to the request is not liquidated!");
                }
            }
            else
            {
                var db_expenses = await _expenseRepository.GetAvanceCaisseExpensesByAcIdAsync(liquidation.RequestId);
                List<Expense> required_to_liquidate_expenses = new();
                foreach (var expense in db_expenses)
                {
                    if (expense.EstimatedFee > 0)
                    {
                        required_to_liquidate_expenses.Add(expense);
                    }
                }
                if (liquidation.ExpensesLiquidations.Count != required_to_liquidate_expenses.Count)
                    return BadRequest("Not All expenses are liquidated! You must liquidate all of the expenses you previously mentioned");
                foreach (var expense in required_to_liquidate_expenses)
                {
                    bool isExistant = liquidation.ExpensesLiquidations.Any(ex => ex.ExpenseId == expense.Id);
                    if (!isExistant)
                        return BadRequest("One of the expenses related to the request is not liquidated!");
                }
            }


            result = await _liquidationService.ModifyLiquidation(liquidation, liquidation_DB);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Id);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpPut("Submit")]
        public async Task<IActionResult> SubmitLiquidation(int Id)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _liquidationRepository.FindLiquidationAsync(Id) == null) return NotFound("Liquidation is not found!");
            ServiceResult result = new();
            var testAC = await _liquidationRepository.GetLiquidationByIdAsync(Id);
            if (testAC.LatestStatus == 1)
            {
                result.Success = false;
                result.Message = "You've already submitted this Liquidation!";
                return BadRequest(result);
            }
            result = await _liquidationService.SubmitLiquidation(Id);
            if (!result.Success) return BadRequest(result.Message);

            return Ok(result.Message);
        }

        [Authorize(Roles = "decider")]
        [HttpPut("Decide")]
        public async Task<IActionResult> DecideOnLiquidation(DecisionOnRequestPostDTO decision)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _liquidationRepository.FindLiquidationAsync(decision.RequestId) == null)
                return NotFound("Liquidation is not found!");

            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null)
                return NotFound("Decider is not found");

            bool isDecisionValid = decision.DecisionString.ToLower() == "approve" || decision.DecisionString.ToLower() == "return" || decision.DecisionString.ToLower() == "reject";
            if (!isDecisionValid) return BadRequest("Decision is invalid!");


            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null)
                return BadRequest("User not found!");

            int deciderRole = await _userRepository.GetUserRoleByUserIdAsync(user.Id);
            if (deciderRole != 3)
                return BadRequest("You are not authorized to decide upon requests!");

            List<string> levels = await _deciderRepository.GetDeciderLevelsByUserId(user.Id);
            if (levels.Contains("TR") && decision.DecisionString.ToLower() == "return" 
                && decision.ReturnedToRequesterByTR == false 
                && decision.ReturnedToFMByTR == false)
                return BadRequest("If you are returning a request, you should specify to whom!");


            ServiceResult result = await _liquidationService.DecideOnLiquidation(decision, user.Id);
            if (!result.Success)
                return BadRequest(result.Message);
            return Ok(result.Message);
        }

        [Authorize(Roles = "decider")]
        [HttpGet("DecideOnRequests/View")]
        public async Task<IActionResult> ShowLiquidationDetailsPageForDecider(int Id)
        {
            if (await _liquidationRepository.FindLiquidationAsync(Id) == null)
                return NotFound("Liquidation is not found");

            LiquidationDeciderViewDTO liquidation = await _liquidationRepository.GetLiquidationFullDetailsByIdForDecider(Id);

            // Fill the requester data
            string username = await _userRepository.GetUsernameByUserIdAsync(liquidation.UserId);
            var userInfo = _ldapAuthenticationService.GetUserInformation(username);
            liquidation.Requester = userInfo;

            // fill request details
            if(liquidation.RequestType == "AV")
            {
                liquidation.RequestDetails = await _avanceVoyageRepository.GetAvanceVoyageDetailsForLiquidationAsync(liquidation.RequestId);

                // if the request is on behalf of someone, get the requester data from the DB
                if (liquidation.OnBehalf)
                {
                    var av = await _avanceVoyageRepository.GetAvanceVoyageByIdAsync(liquidation.RequestId);
                    ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByOrdreMissionIdAsync(av.OrdreMissionId);
                    liquidation.RequestDetails.ActualRequester = _mapper.Map<ActualRequesterDTO>(actualRequesterInfo);
                    liquidation.RequestDetails.ActualRequester.ManagerUserName = await _userRepository.GetUsernameByUserIdAsync(actualRequesterInfo.ManagerUserId);
                }
                try
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath + "\\liquidation-Avance-Voyage-Receipts", liquidation.ReceiptsFileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        liquidation.ReceiptsFile = System.IO.File.ReadAllBytes(filePath);
                    }
                    else
                    {
                        liquidation.ReceiptsFile = Array.Empty<byte>();
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest("Something went wrong while trying to retrieve the file" + ex.Message);
                }
            }
            else
            {
                liquidation.RequestDetails = await _avanceCaisseRepository.GetAvanceCaisseDetailsForLiquidationAsync(liquidation.RequestId);

                // if the request is on behalf of someone, get the requester data from the DB
                if (liquidation.OnBehalf)
                {
                    ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByAvanceCaisseIdAsync(liquidation.RequestId);
                    liquidation.RequestDetails.ActualRequester = _mapper.Map<ActualRequesterDTO>(actualRequesterInfo);
                    liquidation.RequestDetails.ActualRequester.ManagerUserName = await _userRepository.GetUsernameByUserIdAsync(actualRequesterInfo.ManagerUserId);
                }
                try
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath + "\\liquidation-Avance-Caisse-Receipts", liquidation.ReceiptsFileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        liquidation.ReceiptsFile = System.IO.File.ReadAllBytes(filePath);
                    }
                    else
                    {
                        liquidation.ReceiptsFile = Array.Empty<byte>();
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest("Something went wrong while trying to retrieve the file" + ex.Message);
                }
            }


            // adding those more detailed status that are not in the DB
            liquidation.StatusHistory = _miscService.IllustrateStatusHistory(liquidation.StatusHistory);

            return Ok(liquidation);
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

            List<LiquidationDeciderTableDTO> liquidations = await _liquidationRepository.GetLiquidationsTableForDecider(user.Id);

            return Ok(liquidations);
        }

    }
}
