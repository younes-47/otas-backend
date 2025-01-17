﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
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
using System.Net;
using System.Net.Http.Headers;
using TMA_App.Services;

namespace OTAS.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DepenseCaisseController : ControllerBase
    {
        private readonly IDepenseCaisseService _depenseCaisseService;
        private readonly IActualRequesterRepository _actualRequesterRepository;
        private readonly IDepenseCaisseRepository _depenseCaisseRepository;
        private readonly ILdapAuthenticationService _ldapAuthenticationService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDeciderRepository _deciderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMiscService _miscService;
        private readonly IMapper _mapper;

        public DepenseCaisseController(IDepenseCaisseService depenseCaisseService,
            IActualRequesterRepository actualRequesterRepository,
            IDepenseCaisseRepository depenseCaisseRepository,
            ILdapAuthenticationService ldapAuthenticationService,
            IWebHostEnvironment webHostEnvironment,
            IDeciderRepository deciderRepository,
            IUserRepository userRepository,
            IMiscService miscService,
            IMapper mapper)
        {
            _depenseCaisseService = depenseCaisseService;
            _actualRequesterRepository = actualRequesterRepository;
            _depenseCaisseRepository = depenseCaisseRepository;
            _ldapAuthenticationService = ldapAuthenticationService;
            _webHostEnvironment = webHostEnvironment;
            _deciderRepository = deciderRepository;
            _userRepository = userRepository;
            _miscService = miscService;
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


            bool isPDF = depenseCaisse.ReceiptsFile[0] == 0x25 && 
                       depenseCaisse.ReceiptsFile[1] == 0x50 && 
                       depenseCaisse.ReceiptsFile[2] == 0x44 && 
                       depenseCaisse.ReceiptsFile[3] == 0x46 && 
                       depenseCaisse.ReceiptsFile[4] == 0x2D;
            if (!isPDF)
            {
                result.Success = false;
                result.Message = "File Type is invalid! You must upload a pdf file.";
                return BadRequest(result.Message);
            }

            if (depenseCaisse.Expenses.Count < 1)
            {
                result.Success = false;
                result.Message = "DepenseCaisse must contain at least one expense! You can't request a DepenseCaisse with no expense.";
                return BadRequest(result.Message);
            }
            if (depenseCaisse.Currency != "MAD" && depenseCaisse.Currency != "EUR")
            {
                result.Success = false;
                result.Message = "Invalid currency! Please choose suitable currency from the dropdownmenu!";
                return BadRequest(result.Message);
            }
            if (depenseCaisse.OnBehalf == true && depenseCaisse.ActualRequester == null)
            {
                result.Success = false;
                result.Message = "You must fill actual requester's info in case you are filing this request on behalf of someone";
                return BadRequest(result.Message);
            }

            var user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            result = await _depenseCaisseService.AddDepenseCaisse(depenseCaisse, user.Id);

            if (!result.Success) return BadRequest($"{result.Message}");
            return Ok(result.Id);
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("View")]
        public async Task<IActionResult> ShowDepenseCaisseDetailsPage(int Id)
        {
            if (await _depenseCaisseRepository.FindDepenseCaisseAsync(Id) == null) return NotFound("Depense Caisse is not found");
            DepenseCaisseViewDTO depenseCaisseDetails = await _depenseCaisseRepository.GetDepenseCaisseFullDetailsById(Id);

            // if the request is on behalf of someone, get the requester data from the DB
            if (depenseCaisseDetails.OnBehalf)
            {
                ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByDepenseCaisseIdAsync(depenseCaisseDetails.Id);
                depenseCaisseDetails.RequesterInfo = _mapper.Map<ActualRequesterDTO>(actualRequesterInfo);
                depenseCaisseDetails.RequesterInfo.ManagerUserName = await _userRepository.GetUsernameByUserIdAsync(actualRequesterInfo.ManagerUserId);
            }

            // get the file data
            try
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath + "\\Depense-Caisse-Receipts", depenseCaisseDetails.ReceiptsFileName);

                if (System.IO.File.Exists(filePath))
                {
                    depenseCaisseDetails.ReceiptsFile = System.IO.File.ReadAllBytes(filePath);

                }
                else
                {
                    depenseCaisseDetails.ReceiptsFile = Array.Empty<byte>();
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Something went wrong while trying to retrieve the file" +ex.Message);
            }

            // adding those more detailed status that are not in the DB
            depenseCaisseDetails.StatusHistory = _miscService.IllustrateStatusHistory(depenseCaisseDetails.StatusHistory);

            return Ok(depenseCaisseDetails);
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
                    byte[] base64String = System.IO.File.ReadAllBytes(filePath);

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
        public async Task<IActionResult> DownloadDepenseCaisseDocument(int Id)
        {
            if (await _depenseCaisseRepository.FindDepenseCaisseAsync(Id) == null)
                return NotFound("Request Not Found");

            string docxFileName = await _depenseCaisseService.GenerateDepenseCaisseWordDocument(Id);
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

            Response.Headers.Add($"Content-Disposition", $"attachment; filename=DEPENSE_CAISSE_{Id}_DOCUMENT.pdf");

            return Ok(File(base64String, "application/pdf", $"DEPENSE_CAISSE_{Id}_DOCUMENT.pdf"));

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
            if (depenseCaisse.ReceiptsFile != null)
            {
                bool isPDF = depenseCaisse.ReceiptsFile[0] == 0x25 && 
                                depenseCaisse.ReceiptsFile[1] == 0x50 &&  
                                depenseCaisse.ReceiptsFile[2] == 0x44 &&  
                                depenseCaisse.ReceiptsFile[3] == 0x46 &&  
                                depenseCaisse.ReceiptsFile[4] == 0x2D;
                if (!isPDF)
                {
                    result.Success = false;
                    result.Message = "File Type is invalid! You must upload a pdf file.";
                    return BadRequest(result.Message);
                }
            }
            if (depenseCaisse_DB.LatestStatus == 97)
            {
                result.Success = false;
                result.Message = "You can't modify or resubmit a rejected request!";
                return BadRequest(result.Message);
            }
            if (depenseCaisse_DB.LatestStatus != 98 && depenseCaisse_DB.LatestStatus != 99 && depenseCaisse_DB.LatestStatus != 15)
            {
                result.Success = false;
                result.Message = "The DP you are trying to modify is not a draft nor returned! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks";
                return BadRequest(result.Message);
            }
            if (depenseCaisse.Action.ToLower() == "save" && depenseCaisse_DB.LatestStatus == 98)
            {
                result.Success = false;
                result.Message = "You can't modify and save a returned request as a draft again. You may want to apply your modifications to you request and resubmit it directly";
                return BadRequest(result.Message);
            }
            if (depenseCaisse.Expenses.Count < 1)
            {
                result.Success = false;
                result.Message = "DepenseCaisse must have at least one expense! You can't request a DP with no expense.";
                return BadRequest(result.Message);
            }
            if (depenseCaisse.OnBehalf == true && depenseCaisse.ActualRequester == null)
            {
                result.Success = false;
                result.Message = "You must fill actual requester's info in case you are filing this request on behalf of someone";
                return BadRequest(result.Message);
            }


            result = await _depenseCaisseService.ModifyDepenseCaisse(depenseCaisse);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Id);
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
                return BadRequest(result.Message);
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
                return BadRequest(result.Message);
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

            List<DepenseCaisseDeciderTableDTO> DCs = await _depenseCaisseRepository.GetDepenseCaissesForDeciderTable(user.Id);

            return Ok(DCs);
        }

        [Authorize(Roles = "decider")]
        [HttpGet("DecideOnRequests/View")]
        public async Task<IActionResult> ShowDepenseCaisseDetailsPageForDecider(int Id)
        {
            if (await _depenseCaisseRepository.FindDepenseCaisseAsync(Id) == null)
                return NotFound("DepenseCaisse is not found");

            DepenseCaisseDeciderViewDTO depenseCaisse = await _depenseCaisseRepository.GetDepenseCaisseFullDetailsByIdForDecider(Id);

            // Fill the requester data
            string username = await _userRepository.GetUsernameByUserIdAsync(depenseCaisse.UserId);
            var userInfo = _ldapAuthenticationService.GetUserInformation(username);
            depenseCaisse.Requester = userInfo;


            // if the request is on behalf of someone, get the requester data from the DB
            if (depenseCaisse.OnBehalf)
            {
                ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByDepenseCaisseIdAsync(depenseCaisse.Id);
                depenseCaisse.ActualRequester = _mapper.Map<ActualRequesterDTO>(actualRequesterInfo);
            }

            // get the file data
            try
            {
                string filePath = Path.Combine(_webHostEnvironment.WebRootPath + "\\Depense-Caisse-Receipts", depenseCaisse.ReceiptsFileName);

                if (System.IO.File.Exists(filePath))
                {
                    depenseCaisse.ReceiptsFile = System.IO.File.ReadAllBytes(filePath);

                }
                else
                {
                    depenseCaisse.ReceiptsFile = Array.Empty<byte>();
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Something went wrong while trying to retrieve the file" + ex.Message);
            }

            // adding those more detailed status that are not in the DB
            depenseCaisse.StatusHistory = _miscService.IllustrateStatusHistory(depenseCaisse.StatusHistory);

            return Ok(depenseCaisse);
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


            ServiceResult result = await _depenseCaisseService.DecideOnDepenseCaisse(decision, user.Id);
            if (!result.Success) 
                return BadRequest(result.Message);
            return Ok(result.Message);
        }

    }
}
