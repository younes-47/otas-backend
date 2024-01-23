using AutoMapper;
using Azure.Core;
using Microsoft.EntityFrameworkCore.Query.Internal;
using OTAS.Data;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.DTO.Put;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using System.Collections.Generic;

namespace OTAS.Services
{
    public class OrdreMissionService : IOrdreMissionService
    {
        private readonly IActualRequesterRepository _actualRequesterRepository;
        private readonly IAvanceVoyageRepository _avanceVoyageRepository;
        private readonly IAvanceVoyageService _avanceVoyageService;
        private readonly IOrdreMissionRepository _ordreMissionRepository;
        private readonly ITripRepository _tripRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IStatusHistoryRepository _statusHistoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDeciderRepository _deciderRepository;
        private readonly OtasContext _context;
        private readonly IMapper _mapper;

        public OrdreMissionService
            (
                IActualRequesterRepository actualRequesterRepository,
                IAvanceVoyageRepository avanceVoyageRepository,
                IAvanceVoyageService avanceVoyageService,
                IOrdreMissionRepository ordreMissionRepository,
                ITripRepository tripRepository,
                IExpenseRepository expenseRepository,
                IStatusHistoryRepository statusHistoryRepository,
                IUserRepository userRepository,
                IDeciderRepository deciderRepository,
                OtasContext context,
                IMapper mapper
            )
        {
            _actualRequesterRepository = actualRequesterRepository;
            _avanceVoyageRepository = avanceVoyageRepository;
            _avanceVoyageService = avanceVoyageService;
            _ordreMissionRepository = ordreMissionRepository;
            _tripRepository = tripRepository;
            _expenseRepository = expenseRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _userRepository = userRepository;
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

                    var deciderLevel = await _deciderRepository.GetDeciderLevelByUserId(deciderUserId);
                    switch (deciderLevel)
                    {
                        case "MG":
                            decidedOrdreMission.NextDeciderUserId = await _ordreMissionRepository.GetOrdreMissionNextDeciderUserId("MG");
                            decidedOrdreMission.LatestStatus = 2;
                            break;
                        case "HR":
                            decidedOrdreMission.NextDeciderUserId = await _ordreMissionRepository.GetOrdreMissionNextDeciderUserId("HR");
                            decidedOrdreMission.LatestStatus = 3;
                            break;
                        case "FM":
                            decidedOrdreMission.NextDeciderUserId = await _ordreMissionRepository.GetOrdreMissionNextDeciderUserId("FM");
                            decidedOrdreMission.LatestStatus = 4;
                            break;
                        case "GD":
                            decidedOrdreMission.NextDeciderUserId = await _ordreMissionRepository.GetOrdreMissionNextDeciderUserId("GD", IS_LONGER_THAN_ONEDAY);
                            decidedOrdreMission.LatestStatus = IS_LONGER_THAN_ONEDAY ? 5 : 7;  /* if it is not longer than one day skip VP*/
                            break;
                        case "VP":
                            decidedOrdreMission.NextDeciderUserId = await _ordreMissionRepository.GetOrdreMissionNextDeciderUserId("VP");
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

                // delete AV
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

    }
}
