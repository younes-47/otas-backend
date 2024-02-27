using AutoMapper;
using Azure.Core;
using Humanizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Query.Internal;
using OTAS.Data;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.DTO.Put;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace OTAS.Services
{
    public class OrdreMissionService : IOrdreMissionService
    {
        private readonly IActualRequesterRepository _actualRequesterRepository;
        private readonly IAvanceVoyageRepository _avanceVoyageRepository;
        private readonly IAvanceVoyageService _avanceVoyageService;
        private readonly IOrdreMissionRepository _ordreMissionRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ITripRepository _tripRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IStatusHistoryRepository _statusHistoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMiscService _miscService;
        private readonly IDeciderRepository _deciderRepository;
        private readonly OtasContext _context;
        private readonly IMapper _mapper;

        public OrdreMissionService
            (
                IActualRequesterRepository actualRequesterRepository,
                IAvanceVoyageRepository avanceVoyageRepository,
                IAvanceVoyageService avanceVoyageService,
                IOrdreMissionRepository ordreMissionRepository,
                IWebHostEnvironment webHostEnvironment,
                ITripRepository tripRepository,
                IExpenseRepository expenseRepository,
                IStatusHistoryRepository statusHistoryRepository,
                IUserRepository userRepository,
                IMiscService miscService,
                IDeciderRepository deciderRepository,
                OtasContext context,
                IMapper mapper
            )
        {
            _actualRequesterRepository = actualRequesterRepository;
            _avanceVoyageRepository = avanceVoyageRepository;
            _avanceVoyageService = avanceVoyageService;
            _ordreMissionRepository = ordreMissionRepository;
            _webHostEnvironment = webHostEnvironment;
            _tripRepository = tripRepository;
            _expenseRepository = expenseRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _userRepository = userRepository;
            _miscService = miscService;
            _deciderRepository = deciderRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResult> CreateOrdreMissionWithAvanceVoyageAsDraft(OrdreMissionPostDTO ordreMission, int userId)
        {
            using var transaction = _context.Database.BeginTransaction();
            ServiceResult result = new();

            try
            {
                var mappedOM = _mapper.Map<OrdreMission>(ordreMission);

                DateTime smallestDate = DateTime.MaxValue;
                DateTime biggestDate = DateTime.MinValue;
                foreach (var trip in ordreMission.Trips)
                {
                    if (trip.DepartureDate <=  smallestDate)
                    {
                        smallestDate = trip.DepartureDate;
                    }
                    if(trip.ArrivalDate >= biggestDate)
                    {
                        biggestDate = trip.ArrivalDate;
                    }
                }
                mappedOM.DepartureDate = smallestDate;
                mappedOM.ReturnDate = biggestDate;
                mappedOM.UserId = userId;
                result = await _ordreMissionRepository.AddOrdreMissionAsync(mappedOM);
                if (!result.Success) return result;

                StatusHistory OM_status = new()
                {
                    OrdreMissionId = mappedOM.Id,
                    Status = mappedOM.LatestStatus
                };
                result = await _statusHistoryRepository.AddStatusAsync(OM_status);
                if (!result.Success)
                {
                    result.Message += " (OrdreMission)";
                    return result;
                }


                // Handle Actual Requester Info (if exists)
                if (ordreMission.OnBehalf == true)
                {
                    ActualRequester mappedActualRequester = _mapper.Map<ActualRequester>(ordreMission.ActualRequester);
                    mappedActualRequester.OrdreMissionId = mappedOM.Id;
                    mappedActualRequester.OrderingUserId = mappedOM.UserId;
                    mappedActualRequester.ManagerUserId = await _userRepository.GetUserIdByUsernameAsync(ordreMission.ActualRequester.ManagerUserName);
                    result = await _actualRequesterRepository.AddActualRequesterInfoAsync(mappedActualRequester);
                    if (!result.Success) return result;
                }

                result = await _avanceVoyageService.CreateAvanceVoyageForEachCurrency(mappedOM, ordreMission.Trips, ordreMission.Expenses);
                if (!result.Success) return result;
                await transaction.CommitAsync();

                result.Success = true;
                result.Message = "Created successfuly";
                result.Id = mappedOM.Id;
                return result;

            }
            catch (Exception exception)
            {
                transaction.Rollback();
                result.Success = false;
                result.Message = exception.Message + exception.Source + exception.StackTrace;
                return result;
            }
        }

        public async Task<ServiceResult> SubmitOrdreMissionWithAvanceVoyage(int ordreMissionId)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();

            try
            {
                var om = await _ordreMissionRepository.GetOrdreMissionByIdAsync(ordreMissionId);
                var user = await _userRepository.GetUserByUserIdAsync(om.UserId);
                int managerUserId = await _deciderRepository.GetManagerUserIdByUserIdAsync(user.Id);
                result = await _ordreMissionRepository.UpdateOrdreMissionStatusAsync(ordreMissionId, 1, managerUserId);
                if (!result.Success) return result;

                StatusHistory newOM_Status = new()
                {
                    NextDeciderUserId = managerUserId,
                    OrdreMissionId = ordreMissionId,
                    Status = 1,
                };
                result = await _statusHistoryRepository.AddStatusAsync(newOM_Status);
                if (!result.Success)
                {
                    result.Message += " for the newly submitted \"OrdreMission\"";
                    return result;
                }

                List<AvanceVoyage> avacesVoyage = await _avanceVoyageRepository.GetAvancesVoyageByOrdreMissionIdAsync(ordreMissionId);
                foreach (AvanceVoyage av in avacesVoyage)
                {
                    result = await _avanceVoyageService.SubmitAvanceVoyage(av.Id);
                    if (!result.Success) return result;
                }

                await transaction.CommitAsync();
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.Message = exception.Message;
                return result;
            }

            result.Success = true;
            result.Message = "OrdreMission & AvanceVoyage(s) are Submitted successfully";
            return result;

        }

        public async Task<ServiceResult> DecideOnOrdreMission(DecisionOnRequestPostDTO decision, int deciderUserId)
        {

            ServiceResult result = new();
            OrdreMission tempOM = await _ordreMissionRepository.GetOrdreMissionByIdAsync(decision.RequestId);

            //Test if the request is on a state where the decider is not supposed to decide upon it
            if (tempOM.NextDeciderUserId != deciderUserId)
            {
                result.Success = false;
                result.Message = "You can't decide on this request in this state! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks";
                return result;
            }

            bool IS_LONGER_THAN_ONEDAY = true;
            if (tempOM.ReturnDate.Day == tempOM.DepartureDate.Day) IS_LONGER_THAN_ONEDAY = false;

            // CASE: REJECTION / RETURN
            if (decision.DecisionString.ToLower() == "return" || decision.DecisionString.ToLower() == "reject")
            {
                var transaction = _context.Database.BeginTransaction();
                try
                {
                    // Handle OrdreMssion
                    var decidedOrdreMission = await _ordreMissionRepository.GetOrdreMissionByIdAsync(decision.RequestId);
                    decidedOrdreMission.DeciderComment = decision.DeciderComment;
                    decidedOrdreMission.DeciderUserId = deciderUserId;
                    decidedOrdreMission.NextDeciderUserId = null; /* next decider is set to null if returned or rejected for OM */
                    decidedOrdreMission.LatestStatus = decision.DecisionString.ToLower() == "return" ? 98 : 97 ;
                    result = await _ordreMissionRepository.UpdateOrdreMission(decidedOrdreMission);
                    if (!result.Success) return result;

                    StatusHistory decidedOrdreMission_SH = new()
                    {
                        OrdreMissionId = decision.RequestId,
                        DeciderUserId = deciderUserId,
                        DeciderComment = decision.DeciderComment,
                        Status = decision.DecisionString.ToLower() == "return" ? 98 : 97,
                        NextDeciderUserId = decidedOrdreMission.NextDeciderUserId
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(decidedOrdreMission_SH);
                    if (!result.Success) return result;

                    // Handle Related AvanceVoyage(s)
                    List<AvanceVoyage> decidedAvanceVoyages = await _avanceVoyageRepository.GetAvancesVoyageByOrdreMissionIdAsync(decidedOrdreMission.Id);
                    foreach( var av in  decidedAvanceVoyages )
                    {
                        av.DeciderComment = decidedOrdreMission.LatestStatus == 97 ? "Automatically rejected by the system because the related request has been rejected"
                            : "Automatically returned by the system because the related request has been returned"; ;
                        av.DeciderUserId = 1; // System 
                        av.NextDeciderUserId = null; /* next decider is set to null if returned or rejected for OM */
                        av.LatestStatus = decidedOrdreMission.LatestStatus;
                        result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(av);
                        if (!result.Success) return result;

                        StatusHistory decidedAvanceVoyage_SH = new()
                        {
                            Total = av.EstimatedTotal,
                            AvanceVoyageId = av.Id,
                            DeciderUserId = av.DeciderUserId,
                            DeciderComment = av.DeciderComment,
                            Status = av.LatestStatus,
                            NextDeciderUserId = av.NextDeciderUserId,
                        };
                        result = await _statusHistoryRepository.AddStatusAsync(decidedAvanceVoyage_SH);
                        if (!result.Success) return result;
                    }
                    

                    await transaction.CommitAsync();
                }
                catch (Exception exception)
                {
                    result.Success = false;
                    result.Message = exception.Message;
                    return result;
                }
            }
            // CASE: Approval
            else
            {
                var transaction = _context.Database.BeginTransaction();
                try
                {
                    var decidedOrdreMission = await _ordreMissionRepository.GetOrdreMissionByIdAsync(decision.RequestId);
                    decidedOrdreMission.DeciderUserId = deciderUserId;

                    switch (decidedOrdreMission.LatestStatus)
                    {
                        case 1:
                            decidedOrdreMission.NextDeciderUserId = await _ordreMissionRepository.GetOrdreMissionNextDeciderUserId("MG");
                            decidedOrdreMission.LatestStatus = 2;
                            break;
                        case 2:
                            decidedOrdreMission.NextDeciderUserId = await _ordreMissionRepository.GetOrdreMissionNextDeciderUserId("HR");
                            decidedOrdreMission.LatestStatus = 3;
                            break;
                        case 3:
                            decidedOrdreMission.NextDeciderUserId = await _ordreMissionRepository.GetOrdreMissionNextDeciderUserId("FM");
                            decidedOrdreMission.LatestStatus = 4;
                            break;
                        case 4:
                            decidedOrdreMission.NextDeciderUserId = await _ordreMissionRepository.GetOrdreMissionNextDeciderUserId("GD", IS_LONGER_THAN_ONEDAY);
                            decidedOrdreMission.LatestStatus = IS_LONGER_THAN_ONEDAY ? 5 : 7;  /* if it is not longer than one day skip VP*/
                            break;
                        case 5:
                            //decidedOrdreMission.NextDeciderUserId = await _ordreMissionRepository.GetOrdreMissionNextDeciderUserId("VP");
                            decidedOrdreMission.NextDeciderUserId = null; // end of process for Ordre Mission

                            decidedOrdreMission.LatestStatus = 7;
                            break;
                    }
                    result = await _ordreMissionRepository.UpdateOrdreMission(decidedOrdreMission);
                    if (!result.Success) return result;

                    StatusHistory decidedOrdreMission_SH = new()
                    {
                        OrdreMissionId = decision.RequestId,
                        DeciderUserId = deciderUserId,
                        DeciderComment = decision.DeciderComment,
                        Status = decidedOrdreMission.LatestStatus,
                        NextDeciderUserId = decidedOrdreMission.NextDeciderUserId,
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(decidedOrdreMission_SH);
                    if (!result.Success) return result;


                    await transaction.CommitAsync();
                }
                catch (Exception exception)
                {
                    result.Success = false;
                    result.Message = exception.Message;
                    return result;
                }
            }

            result.Success = true;
            result.Message = "OrdreMission is decided upon successfully";
            return result;

        }

        public async Task<ServiceResult> ModifyOrdreMissionWithAvanceVoyage(OrdreMissionPutDTO ordreMission) // action = save || submit
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();
            try
            {
                OrdreMission updatedOrdreMission = await _ordreMissionRepository.GetOrdreMissionByIdAsync(ordreMission.Id);

                // Handle Onbehalf case
                if (ordreMission.OnBehalf == true)
                {
                    //Delete actualrequester info first regardless
                    ActualRequester? actualRequester = await _actualRequesterRepository.FindActualrequesterInfoByOrdreMissionIdAsync(ordreMission.Id);
                    if (actualRequester != null)
                    {
                        result = await _actualRequesterRepository.DeleteActualRequesterInfoAsync(actualRequester);
                        if (!result.Success) return result;
                    }

                    //Insert the new one coming from request
                    ActualRequester mappedActualRequester = _mapper.Map<ActualRequester>(ordreMission.ActualRequester);
                    mappedActualRequester.OrdreMissionId = ordreMission.Id;
                    mappedActualRequester.OrderingUserId = updatedOrdreMission.UserId;
                    mappedActualRequester.ManagerUserId = await _userRepository.GetUserIdByUsernameAsync(ordreMission.ActualRequester.ManagerUserName);
                    result = await _actualRequesterRepository.AddActualRequesterInfoAsync(mappedActualRequester);
                    if (!result.Success) return result;
                }
                else
                {
                    ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByOrdreMissionIdAsync(ordreMission.Id);
                    if (actualRequesterInfo != null)
                    {
                        //Make sure to delete actualrequester in case the user modifies "OnBehalf" to false
                        result = await _actualRequesterRepository.DeleteActualRequesterInfoAsync(actualRequesterInfo);
                        if (!result.Success) return result;
                    }
                }

                updatedOrdreMission.DeciderComment = null;
                updatedOrdreMission.DeciderUserId = null;
                updatedOrdreMission.LatestStatus = ordreMission.Action.ToLower() == "save" ? 99 : 1;
                updatedOrdreMission.Description = ordreMission.Description;
                updatedOrdreMission.Abroad = ordreMission.Abroad;

                if(ordreMission.Action.ToLower() != "save")
                {
                    updatedOrdreMission.NextDeciderUserId = await _deciderRepository.GetManagerUserIdByUserIdAsync(updatedOrdreMission.UserId);
                }

                DateTime smallestDate = DateTime.MaxValue;
                DateTime biggestDate = DateTime.MinValue;
                foreach (var trip in ordreMission.Trips)
                {
                    if (trip.DepartureDate <= smallestDate)
                    {
                        smallestDate = trip.DepartureDate;
                    }
                    if (trip.ArrivalDate >= biggestDate)
                    {
                        biggestDate = trip.ArrivalDate;
                    }
                }
                updatedOrdreMission.DepartureDate = smallestDate;
                updatedOrdreMission.ReturnDate = biggestDate;

                updatedOrdreMission.OnBehalf = ordreMission.OnBehalf;

                if (ordreMission.Action.ToLower() == "submit")
                {
                    if(updatedOrdreMission.NextDeciderUserId != null)
                    {
                        updatedOrdreMission.NextDeciderUserId = await _deciderRepository.GetManagerUserIdByUserIdAsync(updatedOrdreMission.UserId);
                    }
                    result = await _ordreMissionRepository.UpdateOrdreMission(updatedOrdreMission);
                    if (!result.Success) return result;

                    StatusHistory OM_statusHistory = new()
                    {
                        NextDeciderUserId = updatedOrdreMission.NextDeciderUserId,
                        OrdreMissionId = updatedOrdreMission.Id,
                        Status = 1
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(OM_statusHistory);
                    if (!result.Success) return result;
                }
                else
                {
                    result = await _ordreMissionRepository.UpdateOrdreMission(updatedOrdreMission);
                    if (!result.Success) return result;
                }


                List<AvanceVoyage> DB_AvanceVoyages = await _avanceVoyageRepository.GetAvancesVoyageByOrdreMissionIdAsync(ordreMission.Id);

                // Index of 0 couldn't be null, because no ordremission existant in database doesn't have at least one associated AvanceVoyage
                List<Expense> DB_expenses = await _expenseRepository.GetAvanceVoyageExpensesByAvIdAsync(DB_AvanceVoyages[0].Id);
                List<Trip> DB_trips = await _tripRepository.GetAvanceVoyageTripsByAvIdAsync(DB_AvanceVoyages[0].Id);

                if (DB_AvanceVoyages.Count == 2)
                {
                    DB_expenses.AddRange(await _expenseRepository.GetAvanceVoyageExpensesByAvIdAsync(DB_AvanceVoyages[1].Id));
                    DB_trips.AddRange(await _tripRepository.GetAvanceVoyageTripsByAvIdAsync(DB_AvanceVoyages[1].Id));
                }

                // Delete all expenses and trips associated with AvanceVoyage(s) which are associated with this OrdreMission
                if(DB_expenses.Count > 0) // There might be a case where there is no expenses
                {
                    result = await _expenseRepository.DeleteExpenses(DB_expenses);
                    if (!result.Success) return result;
                }
                result = await _tripRepository.DeleteTrips(DB_trips);
                if (!result.Success) return result;
                
                // Call the function in AV Service which will take care of the rest
                result = await _avanceVoyageService.ModifyAvanceVoyagesForEachCurrency(updatedOrdreMission, DB_AvanceVoyages, ordreMission.Trips, ordreMission.Expenses, ordreMission.Action);
                if(!result.Success) return result;


                await transaction.CommitAsync();
                result.Id = updatedOrdreMission.Id;
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.Message = exception.Message + exception.GetType() + exception.StackTrace;
                return result;
            }

            if (ordreMission.Action.ToLower() == "submit")
            {
                result.Success = true;
                result.Message = "OrdreMission is resubmitted successfully";
                return result;
            }

            result.Success = true;
            result.Message = "Changes made to \"OrdreMission\" are saved successfully";
            return result;
        }

        public async Task<ServiceResult> DeleteDraftedOrdreMissionWithAvanceVoyages(OrdreMission ordreMission)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();
            try
            {
                var avanceVoyages = await _avanceVoyageRepository.GetAvancesVoyageByOrdreMissionIdAsync(ordreMission.Id);
                
                // Delete trips & expenses & Status History related to AvanceVoayage
                foreach(AvanceVoyage avanceVoyage in avanceVoyages)
                {
                    List<Trip> trips = await _tripRepository.GetAvanceVoyageTripsByAvIdAsync(avanceVoyage.Id);
                    List<Expense> expenses = await _expenseRepository.GetAvanceVoyageExpensesByAvIdAsync(avanceVoyage.Id);
                    List<StatusHistory> statusHistories = await _statusHistoryRepository.GetAvanceVoyageStatusHistory(avanceVoyage.Id);

                    if(trips.Count > 0)
                    {
                        result = await _tripRepository.DeleteTrips(trips);
                        if (!result.Success) return result;
                    }

                    if(expenses.Count > 0)
                    {
                        result = await _expenseRepository.DeleteExpenses(expenses);
                        if (!result.Success) return result;
                    }

                    result = await _statusHistoryRepository.DeleteStatusHistories(statusHistories);
                    if (!result.Success) return result;
                }

                // Delete Status History related to AvanceVoyage
                var OM_SH = await _statusHistoryRepository.GetOrdreMissionStatusHistory(ordreMission.Id);
                result = await _statusHistoryRepository.DeleteStatusHistories(OM_SH);
                if (!result.Success) return result;

                if(ordreMission.OnBehalf == true)
                {
                    ActualRequester actualRequester = await _actualRequesterRepository.FindActualrequesterInfoByOrdreMissionIdAsync(ordreMission.Id);
                    result = await _actualRequesterRepository.DeleteActualRequesterInfoAsync(actualRequester);
                    if (!result.Success) return result;
                }

                // delete AV
                result = await _avanceVoyageRepository.DeleteAvanceVoyagesRangeAsync(avanceVoyages);
                if(!result.Success) return result;

                // delete OM
                result = await _ordreMissionRepository.DeleteOrdreMissionAsync(ordreMission);
                if (!result.Success) return result;


                await transaction.CommitAsync();

            }
            catch (Exception ex)
            {
                transaction.Rollback();
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
            result.Success = true;
            result.Message = "\"OrdreMission\" has been deleted successfully";
            return result;
        }

        public async Task<ServiceResult> ApproveOrdreMissionWithAvanceVoyage(int ordreMissionId, int deciderUserId)
        {
            // Verify Request
            ServiceResult result = new();
            OrdreMission tempOM = await _ordreMissionRepository.GetOrdreMissionByIdAsync(ordreMissionId);

            //Test if the request is on a state where the decider is not supposed to decide upon it
            if (tempOM.NextDeciderUserId != deciderUserId)
            {
                result.Success = false;
                result.Message = "You can't decide on this request in this state! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks";
                return result;
            }

            bool IS_LONGER_THAN_ONEDAY = true;
            if (tempOM.ReturnDate.Day == tempOM.DepartureDate.Day) IS_LONGER_THAN_ONEDAY = false;
            // Verify Request

            var transaction = _context.Database.BeginTransaction();
            try
            {
                // Handle OrdreMission
                var decidedOrdreMission = await _ordreMissionRepository.GetOrdreMissionByIdAsync(ordreMissionId);
                decidedOrdreMission.DeciderUserId = deciderUserId;

                switch (decidedOrdreMission.LatestStatus)
                {
                    case 1:
                        decidedOrdreMission.NextDeciderUserId = await _ordreMissionRepository.GetOrdreMissionNextDeciderUserId("MG");
                        decidedOrdreMission.LatestStatus = 2;
                        break;
                    case 2:
                        throw new Exception("This decider cannot use the function approve all");
                    case 3:
                        decidedOrdreMission.NextDeciderUserId = await _ordreMissionRepository.GetOrdreMissionNextDeciderUserId("FM");
                        decidedOrdreMission.LatestStatus = 4;
                        break;
                    case 4:
                        decidedOrdreMission.NextDeciderUserId = await _ordreMissionRepository.GetOrdreMissionNextDeciderUserId("GD", IS_LONGER_THAN_ONEDAY);
                        decidedOrdreMission.LatestStatus = IS_LONGER_THAN_ONEDAY ? 5 : 7;  /* if it is not longer than one day skip VP*/
                        break;
                    case 5:
                        //decidedOrdreMission.NextDeciderUserId = await _ordreMissionRepository.GetOrdreMissionNextDeciderUserId("VP");
                        decidedOrdreMission.NextDeciderUserId = null; // end of process for Ordre Mission

                        decidedOrdreMission.LatestStatus = 7;
                        break;
                }
                result = await _ordreMissionRepository.UpdateOrdreMission(decidedOrdreMission);
                if (!result.Success) return result;

                StatusHistory decidedOrdreMission_SH = new()
                {
                    OrdreMissionId = ordreMissionId,
                    DeciderUserId = deciderUserId,
                    DeciderComment = null,
                    Status = decidedOrdreMission.LatestStatus,
                    NextDeciderUserId = decidedOrdreMission.NextDeciderUserId,
                };
                result = await _statusHistoryRepository.AddStatusAsync(decidedOrdreMission_SH);
                if (!result.Success) return result;
                // Handle OrdreMission
                // Handle AvanceVoyage
                List<AvanceVoyage> DB_AvanceVoyages = await _avanceVoyageRepository.GetAvancesVoyageByOrdreMissionIdAsync(ordreMissionId);
                foreach (AvanceVoyage decidedAvanceVoyage in DB_AvanceVoyages)
                {

                    decidedAvanceVoyage.DeciderUserId = deciderUserId;

                    switch (decidedAvanceVoyage.LatestStatus)
                    {
                        case 1:
                            decidedAvanceVoyage.NextDeciderUserId = await _avanceVoyageRepository.GetAvanceVoyageNextDeciderUserId("MG");
                            decidedAvanceVoyage.LatestStatus = 3;
                            break;
                        case 3:
                            decidedAvanceVoyage.NextDeciderUserId = await _avanceVoyageRepository.GetAvanceVoyageNextDeciderUserId("FM");
                            decidedAvanceVoyage.LatestStatus = 4;
                            break;
                        case 4:
                            decidedAvanceVoyage.NextDeciderUserId = await _avanceVoyageRepository.GetAvanceVoyageNextDeciderUserId("GD", IS_LONGER_THAN_ONEDAY);
                            decidedAvanceVoyage.LatestStatus = IS_LONGER_THAN_ONEDAY ? 5 : 8;  /* if it is not longer than one day skip VP*/
                            break;
                        case 5:
                            decidedAvanceVoyage.NextDeciderUserId = await _avanceVoyageRepository.GetAvanceVoyageNextDeciderUserId("VP");
                            decidedAvanceVoyage.LatestStatus = 8;
                            break;
                    }

                    result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(decidedAvanceVoyage);
                    if (!result.Success) return result;

                    StatusHistory decidedAvanceVoyage_SH = new()
                    {
                        AvanceVoyageId = decidedAvanceVoyage.Id,
                        DeciderUserId = deciderUserId,
                        DeciderComment = null,
                        Status = decidedAvanceVoyage.LatestStatus,
                        NextDeciderUserId = decidedAvanceVoyage.NextDeciderUserId,
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(decidedAvanceVoyage_SH);
                    if (!result.Success) return result;
                }
                // Handle AvanceVoyage

                await transaction.CommitAsync();
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.Message = exception.Message;
                return result;
            }

            result.Success = true;
            result.Message = "OrdreMission & AvanceVoyage(s) are Approved successfully";
            return result;
        }


        public async Task<string> GenerateOrdreMissionWordDocument(int ordreMissionId)
        {
            OrdreMissionDocumentDetailsDTO ordreMissionDetails = await _ordreMissionRepository.GetOrdreMissionDocumentDetailsByIdAsync(ordreMissionId);


            var signaturesDir = Path.Combine(_webHostEnvironment.WebRootPath, "Static-Files\\Signatures");
            var docPath = Path.Combine(_webHostEnvironment.WebRootPath, "Static-Files", "ORDRE_MISSION_DOCUMENT.docx");

            Guid tempName = Guid.NewGuid();
            var tempDir = Path.Combine(_webHostEnvironment.WebRootPath, "Temp-Files");

            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }
            var tempFile = Path.Combine(_webHostEnvironment.WebRootPath, "Temp-Files", tempName.ToString());

            Xceed.Words.NET.DocX docx = DocX.Load(docPath);

            try
            {
                // the following regex is to find all the placeholders in the document that are between "<" and ">"
                if (docx.FindUniqueByPattern(@"<([^>]+)>", RegexOptions.IgnoreCase).Count > 0)
                {
                    var replaceTextOptions = new FunctionReplaceTextOptions()
                    {
                        FindPattern = "<(.*?)>",
                        RegexMatchHandler = (match) => {
                            return ReplaceOrdreMissionDocumentPlaceHolders(match, ordreMissionDetails);
                        },
                        RegExOptions = RegexOptions.IgnoreCase,
                        NewFormatting = new Formatting() { FontFamily = new Xceed.Document.NET.Font("Arial"), Bold = false }
                    };
                    docx.ReplaceText(replaceTextOptions);
#pragma warning disable CS0618 // func is obsolete

                    // Replace the expenses table
                    List<ExpenseDTO> expenses = new();

                    foreach (var av in ordreMissionDetails.AvanceVoyagesDetails)
                    {
                        expenses.AddRange(av.Expenses);
                    }
                    if (expenses.Count > 0)
                    {
                        docx.ReplaceTextWithObject("expenses", _miscService.GenerateExpesnesTableForDocuments(docx, expenses));
                    }
                    else
                    {
                        docx.ReplaceText("expenses", "(Pas de dépense)");
                    }

                    // Replace the trips table
                    List<TripDTO> trips = new();

                    foreach (var av in ordreMissionDetails.AvanceVoyagesDetails)
                    {
                        trips.AddRange(av.Trips);
                    }
                    docx.ReplaceTextWithObject("trips", _miscService.GenerateTripsTableForDocuments(docx, trips));

                    // Replace the signatures
                    if (ordreMissionDetails.Signers.Any(s => s.Level == "MG"))
                    {
                        docx.ReplaceText("%mg_signature%", ordreMissionDetails.Signers.Where(s => s.Level == "MG")
                                .Select(s => $"{s.FirstName} {s.LastName}")
                                .First());
                        docx.ReplaceText("%mg_signature_date%", ordreMissionDetails.Signers.Where(s => s.Level == "MG")
                                .Select(s => s.SignDate.ToString("dd/MM/yyyy hh:mm"))
                                .First());

                        string? imgName = ordreMissionDetails.Signers.Where(s => s.Level == "MG")
                                        .Select(s => s.SignatureImageName)
                                        .First();

                        Xceed.Document.NET.Image signature_img = docx.AddImage(signaturesDir + $"\\{imgName}");
                        Xceed.Document.NET.Picture signature_pic = signature_img.CreatePicture(75.84f, 92.16f);
                        docx.ReplaceTextWithObject("%mg_signature_img%", signature_pic, false, RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        docx.ReplaceText("%mg_signature%", "");
                        docx.ReplaceText("%mg_signature_date%", "");
                        docx.ReplaceText("%mg_signature_img%", "");
                    }
                    if (ordreMissionDetails.Signers.Any(s => s.Level == "HR"))
                    {
                        docx.ReplaceText("%hr_signature%", ordreMissionDetails.Signers.Where(s => s.Level == "HR")
                                .Select(s => $"{s.FirstName} {s.LastName}")
                                .First());
                        docx.ReplaceText("%hr_signature_date%", ordreMissionDetails.Signers.Where(s => s.Level == "HR")
                                .Select(s => s.SignDate.ToString("dd/MM/yyyy hh:mm"))
                                .First());

                        string? imgName = ordreMissionDetails.Signers.Where(s => s.Level == "HR")
                                        .Select(s => s.SignatureImageName)
                                        .First();

                        Xceed.Document.NET.Image signature_img = docx.AddImage(signaturesDir + $"\\{imgName}");
                        Xceed.Document.NET.Picture signature_pic = signature_img.CreatePicture(75.84f, 92.16f);
                        docx.ReplaceTextWithObject("%hr_signature_img%", signature_pic, false, RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        docx.ReplaceText("%hr_signature%", "");
                        docx.ReplaceText("%hr_signature_date%", "");
                        docx.ReplaceText("%hr_signature_img%", "");
                    }
                    if (ordreMissionDetails.Signers.Any(s => s.Level == "FM"))
                    {
                        docx.ReplaceText("%fm_signature%", ordreMissionDetails.Signers.Where(s => s.Level == "FM")
                                .Select(s => $"{s.FirstName} {s.LastName}")
                                .First());
                        docx.ReplaceText("%fm_signature_date%", ordreMissionDetails.Signers.Where(s => s.Level == "FM")
                                .Select(s => s.SignDate.ToString("dd/MM/yyyy hh:mm"))
                                .First());

                        string? imgName = ordreMissionDetails.Signers.Where(s => s.Level == "FM")
                                        .Select(s => s.SignatureImageName)
                                        .First();

                        Xceed.Document.NET.Image signature_img = docx.AddImage(signaturesDir + $"\\{imgName}");
                        Xceed.Document.NET.Picture signature_pic = signature_img.CreatePicture(75.84f, 92.16f);
                        docx.ReplaceTextWithObject("%fm_signature_img%", signature_pic, false, RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        docx.ReplaceText("%fm_signature%", "");
                        docx.ReplaceText("%fm_signature_date%", "");
                        docx.ReplaceText("%fm_signature_img%", "");
                    }
                    if (ordreMissionDetails.Signers.Any(s => s.Level == "GD"))
                    {
                        docx.ReplaceText("%gd_signature%", ordreMissionDetails.Signers.Where(s => s.Level == "GD")
                                .Select(s => $"{s.FirstName} {s.LastName}")
                                .First());
                        docx.ReplaceText("%gd_signature_date%", ordreMissionDetails.Signers.Where(s => s.Level == "GD")
                                .Select(s => s.SignDate.ToString("dd/MM/yyyy hh:mm"))
                                .First());

                        string? imgName = ordreMissionDetails.Signers.Where(s => s.Level == "GD")
                                        .Select(s => s.SignatureImageName)
                                        .First();

                        Xceed.Document.NET.Image signature_img = docx.AddImage(signaturesDir + $"\\{imgName}");
                        Xceed.Document.NET.Picture signature_pic = signature_img.CreatePicture(75.84f, 92.16f);
                        docx.ReplaceTextWithObject("%gd_signature_img%", signature_pic, false, RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        docx.ReplaceText("%gd_signature%", "");
                        docx.ReplaceText("%gd_signature_date%", "");
                        docx.ReplaceText("%gd_signature_img%", "");
                    }
#pragma warning restore CS0618 // func is obsolete
                    docx.SaveAs(tempFile);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }


            return tempName.ToString();

        }

        public string ReplaceOrdreMissionDocumentPlaceHolders(string placeHolder, OrdreMissionDocumentDetailsDTO ordreMissionDetails)
        {
            #pragma warning disable CS8604 // null warning.
            #pragma warning disable CS8602 // Dereference of a possibly null reference.
            Dictionary<string, string> _replacePatterns = new Dictionary<string, string>()
              {
                { "id", ordreMissionDetails.Id.ToString() },
                { "full_name", ordreMissionDetails.FirstName + " " + ordreMissionDetails.LastName },
                { "date", ordreMissionDetails.SubmitDate.ToString("dd/MM/yyyy") },
                { "amount", ordreMissionDetails.AvanceVoyagesDetails
                                                .Where(av => av.Currency == "MAD")
                                                .Any() ?
                                                ordreMissionDetails.AvanceVoyagesDetails
                                                .Where(av => av.Currency == "MAD")
                                                .Select(av => av.EstimatedTotal)
                                                .FirstOrDefault()
                                                .ToString().FormatWith(new CultureInfo("fr-FR"))
                                                : ""},
                { "currency", ordreMissionDetails.AvanceVoyagesDetails
                                .Where(av => av.Currency == "MAD")
                                .Any() ?
                                ordreMissionDetails.AvanceVoyagesDetails
                                .Where(av => av.Currency == "MAD")
                                .Select(av => av.Currency)
                                .FirstOrDefault()
                                .ToString().FormatWith(new CultureInfo("fr-FR"))
                                : ""},
                { "amount_2", ordreMissionDetails.AvanceVoyagesDetails
                                                .Where(av => av.Currency == "EUR")
                                                .Any() ?
                                                ordreMissionDetails.AvanceVoyagesDetails
                                                .Where(av => av.Currency == "EUR")
                                                .Select(av => av.EstimatedTotal)
                                                .FirstOrDefault()
                                                .ToString().FormatWith(new CultureInfo("fr-FR"))
                                                : ""},
                { "currency_2", ordreMissionDetails.AvanceVoyagesDetails
                                .Where(av => av.Currency == "EUR")
                                .Any() ?
                                ordreMissionDetails.AvanceVoyagesDetails
                                .Where(av => av.Currency == "EUR")
                                .Select(av => av.Currency)
                                .FirstOrDefault()
                                .ToString().FormatWith(new CultureInfo("fr-FR"))
                                : ""},
                { "worded_amount", ordreMissionDetails.AvanceVoyagesDetails
                                                .Where(av => av.Currency == "MAD")
                                                .Any() ? 
                                                ((int)ordreMissionDetails
                                                .AvanceVoyagesDetails
                                                .Where(av => av.Currency == "MAD")
                                                .Select(av => av.EstimatedTotal)
                                                .FirstOrDefault()).ToWords(new CultureInfo("fr-FR")) 
                                                : "" },
                { "worded_amount_2", ordreMissionDetails.AvanceVoyagesDetails
                                .Where(av => av.Currency == "EUR")
                                .Any() ?
                                $@"ET {((int)ordreMissionDetails
                                        .AvanceVoyagesDetails
                                        .Where(av => av.Currency == "EUR")
                                        .Select(av => av.EstimatedTotal)
                                        .FirstOrDefault()).ToWords(new CultureInfo("fr-FR"))}"
                                : "" },
                { "description", ordreMissionDetails.Description.ToString() },
              };
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // null warning
            if (_replacePatterns.ContainsKey(placeHolder))
            {
                return _replacePatterns[placeHolder];
            }
            return placeHolder;
        }



    }
}
