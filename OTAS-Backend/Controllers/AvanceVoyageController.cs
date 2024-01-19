﻿using AutoMapper;
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
        private readonly ILdapAuthenticationService _ldapAuthenticationService;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;

        public AvanceVoyageController(IAvanceVoyageRepository avanceVoyageRepository,
            IAvanceVoyageService avanceVoyageService,
            IDeciderRepository deciderRepository,
            IActualRequesterRepository actualRequesterRepository,
            ILdapAuthenticationService ldapAuthenticationService,
            IMapper mapper,
            IUserRepository userRepository)
        {
            _avanceVoyageRepository = avanceVoyageRepository;
            _avanceVoyageService = avanceVoyageService;
            _deciderRepository = deciderRepository;
            _actualRequesterRepository = actualRequesterRepository;
            _ldapAuthenticationService = ldapAuthenticationService;
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
            // otherwise get the requester data from AD
            // it wll be handled from frontend since user data is stored in a state so no need to interact with AD everytime
            //else
            //{
            //    var userInfo = _ldapAuthenticationService.GetUserInfo(user.Username);
            //    avanceVoyage.RequesterInfo.FirstName = userInfo[0];
            //    avanceVoyage.RequesterInfo.LastName = userInfo[1];
            //    avanceVoyage.RequesterInfo.ManagerUserName = userInfo[2];
            //    avanceVoyage.RequesterInfo.RegistrationNumber = userInfo[3];
            //    avanceVoyage.RequesterInfo.JobTitle = userInfo[4];
            //    avanceVoyage.RequesterInfo.Department = userInfo[5];
            //}

            // adding those more detailed status that are not in the DB
            for (int i = avanceVoyage.StatusHistory.Count - 1; i >= 0; i--)
            {
                StatusHistoryDTO statusHistory = avanceVoyage.StatusHistory[i];
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
                avanceVoyage.StatusHistory.Insert(i, explicitStatusHistory);
            }
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

        [Authorize(Roles = "decider")]
        [HttpGet("DecideOnRequests/Table")]
        public async Task<IActionResult> ShowAvanceVoyageDecideTable()
        {
            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");

            List<AvanceVoyageTableDTO> AVs = await _avanceVoyageRepository.GetAvanceVoyagesForDeciderTable(user.Id);

            return Ok(AVs);
        }

        [Authorize(Roles = "decider")]
        [HttpPut("Decide")]
        public async Task<IActionResult> DecideOnAvanceVoyage(DecisionOnRequestPostDTO decision)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (await _avanceVoyageRepository.FindAvanceVoyageByIdAsync(decision.RequestId) == null) return NotFound("AvanceVoyage is not found!");

            User? user = await _userRepository.GetUserByHttpContextAsync(HttpContext);
            if (await _userRepository.FindUserByUserIdAsync(user.Id) == null) return BadRequest("User not found!");

            int deciderRole = await _userRepository.GetUserRoleByUserIdAsync(user.Id);
            if (deciderRole != 3) return BadRequest("You are not authorized to decide upon requests!");

            ServiceResult result = await _avanceVoyageService.DecideOnAvanceVoyage(decision, user.Id);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Message);
        }

        [Authorize(Roles = "decider")]
        [HttpPut("Decide/Funds")]
        public async Task<IActionResult> MarkFundsAsPrepared(MarkFundsAsPreparedPutDTO action) // Only TR
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

            //if (advanceOption.ToLower() != "" && advanceOption.ToLower() != "")
            //    return BadRequest("Invalid Advance choice!");

            if (await _avanceVoyageRepository.FindAvanceVoyageByIdAsync(action.RequestId) == null)
                return BadRequest("Request is not found");

            ServiceResult result = await _avanceVoyageService.MarkFundsAsPrepared(action.RequestId, action.AdvanceOption, user.Id);
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

            if (await _avanceVoyageRepository.FindAvanceVoyageByIdAsync(action.RequestId) == null)
                return BadRequest("Request is not found");

            ServiceResult result = await _avanceVoyageService.ConfirmFundsDelivery(action.RequestId, action.ConfirmationNumber);
            if (!result.Success)
                return Forbid(result.Message);

            return Ok(result.Message);
        }

    }
}
