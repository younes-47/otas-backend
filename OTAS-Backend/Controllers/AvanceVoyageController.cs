﻿using AutoMapper;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.PdfCleanup.Autosweep;
using iText.PdfCleanup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
    public class AvanceVoyageController : ControllerBase
    {
        private readonly IAvanceVoyageRepository _avanceVoyageRepository;
        private readonly IAvanceVoyageService _avanceVoyageService;
        private readonly IDeciderRepository _deciderRepository;
        private readonly IActualRequesterRepository _actualRequesterRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ILdapAuthenticationService _ldapAuthenticationService;
        private readonly IMiscService _miscService;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;

        public AvanceVoyageController(IAvanceVoyageRepository avanceVoyageRepository,
            IAvanceVoyageService avanceVoyageService,
            IDeciderRepository deciderRepository,
            IActualRequesterRepository actualRequesterRepository,
            IWebHostEnvironment webHostEnvironment,
            ILdapAuthenticationService ldapAuthenticationService,
            IMiscService miscService,
            IMapper mapper,
            IUserRepository userRepository)
        {
            _avanceVoyageRepository = avanceVoyageRepository;
            _avanceVoyageService = avanceVoyageService;
            _deciderRepository = deciderRepository;
            _actualRequesterRepository = actualRequesterRepository;
            _webHostEnvironment = webHostEnvironment;
            _ldapAuthenticationService = ldapAuthenticationService;
            _miscService = miscService;
            _mapper = mapper;
            _userRepository = userRepository;
        }

        [Authorize(Roles = "requester,decider")]
        [HttpGet("View")]
        public async Task<IActionResult> ShowAvanceVoyageDetailsPage(int Id)
        {
            if (await _avanceVoyageRepository.FindAvanceVoyageByIdAsync(Id) == null)
                return NotFound("AvanceVoyage is not found");

            AvanceVoyageViewDTO avanceVoyage = await _avanceVoyageRepository.GetAvancesVoyageViewDetailsByIdAsync(Id);
           

            // if the request is on behalf of someone, get the requester data from the DB
            if (avanceVoyage.OnBehalf)
            {
                ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByOrdreMissionIdAsync(avanceVoyage.OrdreMissionId);
                avanceVoyage.RequesterInfo = _mapper.Map<ActualRequesterDTO>(actualRequesterInfo);
                
            }


            // adding those more detailed status that are not in the DB
            avanceVoyage.StatusHistory = _miscService.IllustrateStatusHistory(avanceVoyage.StatusHistory);

            return Ok(avanceVoyage);
        }


        [Authorize(Roles = "requester,decider")]
        [HttpGet("Requests/Table")]
        public async Task<IActionResult> ShowAvanceVoyageRequestsTable()
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);

            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");

            List<AvanceVoyageTableDTO> mappedAVs = await _avanceVoyageRepository.GetAvancesVoyageByUserIdAsync(user.Id);

            return Ok(mappedAVs);
        }

        //[Authorize(Roles = "requester")]
        //[HttpGet("Document/Download")]
        //public async Task<IActionResult> DownloadAvanceVoyageDocument(int Id)
        //{
        //    if (await _avanceVoyageRepository.FindAvanceVoyageByIdAsync(Id) == null)
        //        return NotFound("Request Not Found");

        //    string watermarkedFileName = await _avanceVoyageService.GenerateWaterMarkedAvanceVoyageDocument(Id);
        //    var tempDir = Path.Combine(_webHostEnvironment.WebRootPath, "Temp-Files");
        //    var watermarkedFilePath = Path.Combine(_webHostEnvironment.WebRootPath, "Temp-Files", watermarkedFileName + ".pdf");

        //    // Remove Watermark
        //    CompositeCleanupStrategy strategy = new();
        //    strategy.Add(new RegexBasedCleanupStrategy("Evaluation Only. Created with Aspose.PDF. Copyright 2002-2023 Aspose Pty Ltd.")
        //                    .SetRedactionColor(ColorConstants.WHITE));

        //    Guid tempName = Guid.NewGuid();
        //    var cleanedDocPath = Path.Combine(tempDir, tempName.ToString() + ".pdf");

        //    PdfDocument cleanedDoc = new PdfDocument(new PdfReader(watermarkedFilePath), new PdfWriter(cleanedDocPath).SetCompressionLevel(0));


        //    // Do the magic
        //    PdfCleaner.AutoSweepCleanUp(cleanedDoc, strategy);

        //    cleanedDoc.Close();

        //    byte[] base64String = System.IO.File.ReadAllBytes(cleanedDocPath);

        //    /* Delete temp files */
        //    System.IO.File.Delete(watermarkedFilePath);
        //    System.IO.File.Delete(cleanedDocPath);



        //    Response.Headers.Add($"Content-Disposition", $"attachment; filename=AVANCE_VOYAGE_{Id}_DOCUMENT.pdf");

        //    return Ok(File(base64String, "application/pdf", $"AVANCE_VOYAGE_{Id}_DOCUMENT.pdf"));

        //}



        [Authorize(Roles = "decider")]
        [HttpGet("DecideOnRequests/Table")]
        public async Task<IActionResult> ShowAvanceVoyageDecideTable()
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");

            List<AvanceVoyageDeciderTableDTO> AVs = await _avanceVoyageRepository.GetAvanceVoyagesForDeciderTable(user.Id);

            return Ok(AVs);
        }
        [Authorize(Roles = "decider")]
        [HttpGet("DecideOnRequests/View")]
        public async Task<IActionResult> ShowAvanceVoyageDetailsPageForDecider(int Id)
        {
            if (await _avanceVoyageRepository.FindAvanceVoyageByIdAsync(Id) == null)
                return NotFound("AvanceVoyage is not found");

            AvanceVoyageDeciderViewDTO avanceVoyage = await _avanceVoyageRepository.GetAvanceVoyageFullDetailsByIdForDecider(Id);

            // Fill the requester data
            string username = await _userRepository.GetUsernameByUserIdAsync(avanceVoyage.UserId);
            var userInfo = _ldapAuthenticationService.GetUserInformation(username);
            avanceVoyage.Requester = userInfo;


            // if the request is on behalf of someone, get the requester data from the DB
            if (avanceVoyage.OnBehalf)
            {
                ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByOrdreMissionIdAsync(avanceVoyage.OrdreMissionId);
                avanceVoyage.ActualRequester = _mapper.Map<ActualRequesterDTO>(actualRequesterInfo);
            }

            // adding those more detailed status that are not in the DB
            avanceVoyage.StatusHistory = _miscService.IllustrateStatusHistory(avanceVoyage.StatusHistory);
            return Ok(avanceVoyage);
        }



        [Authorize(Roles = "decider")]
        [HttpPut("Decide")]
        public async Task<IActionResult> DecideOnAvanceVoyage(DecisionOnRequestPostDTO decision)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _avanceVoyageRepository.FindAvanceVoyageByIdAsync(decision.RequestId) == null) return NotFound("AvanceVoyage is not found!");

            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");

            bool isDecisionValid = decision.DecisionString.ToLower() == "approve" || decision.DecisionString.ToLower() == "return" || decision.DecisionString.ToLower() == "reject";
            if (!isDecisionValid)
                return BadRequest("Decision is invalid!");


            int deciderRole = await _userRepository.GetUserRoleByUserIdAsync(user.Id);
            if (deciderRole != 3) return BadRequest("You are not authorized to decide upon requests!");

            List<string> levels = await _deciderRepository.GetDeciderLevelsByUserId(user.Id);
            if (levels.Contains("TR") && decision.DecisionString.ToLower() == "return")
                return BadRequest("You are not authorized to return requests");


            ServiceResult result = await _avanceVoyageService.DecideOnAvanceVoyage(decision, user.Id);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }

        [Authorize(Roles = "decider")]
        [HttpPut("Decide/Funds/MarkAsPrepared")]
        public async Task<IActionResult> MarkFundsAsPrepared(MarkFundsAsPreparedPutDTO action) // Only TR
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null)
                return BadRequest("User not found!");

            if (await _userRepository.GetUserRoleByUserIdAsync(user.Id) != 3)
                return BadRequest("You are not authorized to decide upon requests!");

            List<string> levels = await _deciderRepository.GetDeciderLevelsByUserId(user.Id);
            if (levels.Contains("TR") == false)
                return BadRequest("You are not authorized to perform this action");


            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (action.AdvanceOption.ToUpper() != "CASH" && action.AdvanceOption.ToUpper() != "PROVISION")
                return BadRequest("Invalid Advance choice!");

            if (await _avanceVoyageRepository.FindAvanceVoyageByIdAsync(action.RequestId) == null)
                return BadRequest("Request is not found");

            ServiceResult result = await _avanceVoyageService.MarkFundsAsPrepared(action.RequestId, action.AdvanceOption, user.Id);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);

        }

        [Authorize(Roles = "decider")]
        [HttpPut("Decide/Funds/ConfirmDelivery")]
        public async Task<IActionResult> ConfirmFundsDelivery(ConfirmFundsDeliveryPutDTO action) // Only TR
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null)
                return BadRequest("User not found!");

            if (await _userRepository.GetUserRoleByUserIdAsync(user.Id) != 3)
                return BadRequest("You are not authorized to decide upon requests!");

            List<string> levels = await _deciderRepository.GetDeciderLevelsByUserId(user.Id);
            if (levels.Contains("TR") == false)
                return BadRequest("You are not authorized to perform this action");


            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            AvanceVoyage? av = await _avanceVoyageRepository.FindAvanceVoyageByIdAsync(action.RequestId);
            if (av == null)
                return BadRequest("Request is not found");

            if (av.LatestStatus == 10)
                return BadRequest("Funds has already been confirmed as delivered!");

            if (av.LatestStatus != 9)
                return BadRequest("Cannot confirm delivery of funds at this state");

            ServiceResult result = await _avanceVoyageService.ConfirmFundsDelivery(action.RequestId, action.ConfirmationNumber);
            if (!result.Success)
                return Forbid(result.Message);

            return Ok(result.Message);
        }

    }
}
