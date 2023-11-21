using AutoMapper;
using OTAS.Data;
using OTAS.DTO.Post;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;

namespace OTAS.Services
{
    public class OrdreMissionService : IOrdreMissionService
    {
        private readonly IActualRequesterService _actualRequesterService;
        private readonly IAvanceVoyageRepository _avanceVoyageRepository;
        private readonly IOrdreMissionRepository _ordreMissionRepository;
        private readonly ITripRepository _tripRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IStatusHistoryRepository _statusHistoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly OtasContext _context;
        private readonly IMapper _mapper;

        public OrdreMissionService
            (
                IActualRequesterService actualRequester,
                IAvanceVoyageRepository avanceVoyageRepository,
                IOrdreMissionRepository ordreMissionRepository,
                ITripRepository tripRepository,
                IExpenseRepository expenseRepository,
                IStatusHistoryRepository statusHistoryRepository,
                IUserRepository userRepository,
                OtasContext context,
                IMapper mapper
            ) 
        {
            _actualRequesterService = actualRequester;
            _avanceVoyageRepository = avanceVoyageRepository;
            _ordreMissionRepository = ordreMissionRepository;
            _tripRepository = tripRepository;
            _expenseRepository = expenseRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _userRepository = userRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResult> CreateAvanceVoyageForEachCurrency(OrdreMission mappedOM, List<TripPostDTO> mixedTrips, List<ExpensePostDTO>? mixedExpenses)
        {
            ServiceResult result = new();

            // Sort Trips
            List<TripPostDTO>? trips_in_mad = new();
            List<TripPostDTO>? trips_in_eur = new();

            foreach (TripPostDTO trip in mixedTrips)
            {
                if (trip.Unit == "MAD" || trip.Unit == "KM")
                {
                    trips_in_mad.Add(trip);
                }
                else if (trip.Unit == "EUR")
                {
                    trips_in_eur.Add(trip);
                }
            }

            // Sort Expenses
            List<ExpensePostDTO>? expenses_in_mad = new();
            List<ExpensePostDTO>? expenses_in_eur = new();

            foreach (ExpensePostDTO expense in mixedExpenses)
            {
                if (expense.Currency == "MAD")
                {
                    expenses_in_mad.Add(expense);
                }
                else if (expense.Currency == "EUR")
                {
                    expenses_in_eur.Add(expense);
                }
            }

            // trips & expenses li bel MAD ghadi ykono related l' Avance de Voyage bo7dha
            if (trips_in_mad.Count > 0 || expenses_in_mad.Count > 0)
            {
                decimal estm_total_mad = 0;

                if (trips_in_mad.Count > 0)
                {
                    // Factoring in total fees of the trip(s) in the estimated total of the whole "AvanceVoyage" in MAD
                    foreach (TripPostDTO trip in trips_in_mad)
                    {
                        estm_total_mad += CalculateTripEstimatedFee(_mapper.Map<Trip>(trip));
                    }
                }

                if (expenses_in_mad.Count > 0)
                {
                    // Factoring in total fees of the trip(s) in the estimated total of the whole "AvanceVoyage" in MAD
                    foreach (ExpensePostDTO expense in expenses_in_mad)
                    {
                        estm_total_mad += expense.EstimatedFee;
                    }
                }

                AvanceVoyage avanceVoyage_in_mad = new()
                {
                    //Map the unmapped properties by Automapper
                    OrdreMissionId = mappedOM.Id,
                    UserId = mappedOM.UserId,
                    EstimatedTotal = estm_total_mad,
                    Currency = "MAD",
                };
                result = await _avanceVoyageRepository.AddAvanceVoyageAsync(avanceVoyage_in_mad);
                if(!result.Success)
                {
                    result.Message += " (MAD)";
                    return result;
                }

                // Inserting the initial status of the "AvanceVoyage" in MAD in StatusHistory
                StatusHistory AV_status = new()
                {
                    AvanceVoyage = avanceVoyage_in_mad,
                };
                result = await _statusHistoryRepository.AddStatusAsync(AV_status);
                if (!result.Success)
                {
                    result.Message += " (Avancevoyage in mad)";
                    return result;
                }


                //There might be a case where an AV has no Trips in mad
                if (trips_in_mad.Count > 0)
                {
                    // Inserting the trip(s) related to the "AvanceVoyage" in MAD (at least one trip)
                    var mappedTrips = _mapper.Map<List<Trip>>(trips_in_mad);
                    foreach (Trip mappedTrip in mappedTrips)
                    {
                        mappedTrip.AvanceVoyageId = avanceVoyage_in_mad.Id;
                        mappedTrip.EstimatedFee = CalculateTripEstimatedFee(mappedTrip);
                    }
                    result = await _tripRepository.AddTripsAsync(mappedTrips);
                    if (!result.Success)
                    {
                        result.Message = "(MAD).";
                        return result;
                    }
                }


                //There might be a case where an AV has no other expenses in mad
                if (expenses_in_mad.Count > 0)
                {
                    // Inserting the expense(s) related to the "AvanceVoyage" in MAD
                    var mappedExpenses = _mapper.Map<List<Expense>>(expenses_in_mad);
                    foreach (Expense mappedExpense in mappedExpenses)
                    {
                        //mappedExpense.AvanceVoyage = avanceVoyage_in_mad;
                        mappedExpense.AvanceVoyageId = avanceVoyage_in_mad.Id;
                    }
                    result = await _expenseRepository.AddExpensesAsync(mappedExpenses);
                    if (!result.Success)
                    {
                        result.Message += " (MAD)";
                        return result;
                    }
                }

            }


            // trips & expenses li bel EUR ghadi ykono related l' Avance de Voyage bo7dha
            if (trips_in_eur.Count > 0 || expenses_in_eur.Count > 0)
            {
                decimal estm_total_eur = 0;


                if (trips_in_eur.Count > 0)
                {
                    // Factoring in total fees of the trip(s) in the estimated total of the whole "AvanceVoyage" in EUR
                    foreach (TripPostDTO trip in trips_in_eur)
                    {
                        estm_total_eur += CalculateTripEstimatedFee(_mapper.Map<Trip>(trip));
                    }
                }

                if (expenses_in_eur.Count > 0)
                {
                    // Factoring in total fees of the expense(s) in the estimated total of the whole "AvanceVoyage" in EUR
                    foreach (ExpensePostDTO expense in expenses_in_eur)
                    {
                        estm_total_eur += expense.EstimatedFee;
                    }
                }

                // Inserting the "AvanceVoyage" in EUR
                AvanceVoyage avanceVoyage_in_eur = new()
                {
                    //Map the unmapped properties by Automapper manually
                    OrdreMissionId = mappedOM.Id,
                    UserId = mappedOM.UserId,
                    EstimatedTotal = estm_total_eur,
                    Currency = "EUR",
                };
                result = await _avanceVoyageRepository.AddAvanceVoyageAsync(avanceVoyage_in_eur);
                if (!result.Success)
                {
                    result.Message += " (AvanceVoyage in EUR)";
                    return result;
                }

                // Inserting the initial status of the "AvanceVoyage" in EUR in StatusHistory
                StatusHistory AV_status = new()
                {
                    AvanceVoyage = avanceVoyage_in_eur,
                };
                result = await _statusHistoryRepository.AddStatusAsync(AV_status);
                if (!result.Success)
                {
                    result.Message += " (Avancevoyage in eur)";
                    return result;
                }

                // There might be a case where an "OrdreMission" has no trip in EUR
                if (trips_in_eur.Count > 0)
                {
                    // Inserting the trips related to the "AvanceVoyage" in EUR
                    var mappedTrips = _mapper.Map<List<Trip>>(trips_in_eur);
                    foreach (Trip mappedTrip in mappedTrips)
                    {
                        if (mappedTrip.Unit == "EUR")
                        {
                            mappedTrip.AvanceVoyageId = avanceVoyage_in_eur.Id;
                            mappedTrip.EstimatedFee = CalculateTripEstimatedFee(mappedTrip);
                        }
                    }
                    result = await _tripRepository.AddTripsAsync(mappedTrips);
                    if (!result.Success)
                    {
                        result.Message += " (EUR).";
                        return result;
                    }
                }

                //There might be a case where an "OrdreMission" has no other expenses in EUR
                if (expenses_in_eur.Count > 0)
                {
                    // Inserting the expenses related to the "AvanceVoyage" in EUR
                    var mappedExpenses = _mapper.Map<List<Expense>>(expenses_in_eur);
                    foreach (Expense mappedExpense in mappedExpenses)
                    {
                        mappedExpense.AvanceVoyageId = avanceVoyage_in_eur.Id;
                    }
                    result = await _expenseRepository.AddExpensesAsync(mappedExpenses);
                    if (!result.Success)
                    {
                        result.Message += " (EUR)";
                        return result;
                    }
                }
            }
            result.Success = true;
            result.Message = "Avance de voyage has been submitted successfully";
            return result;
        }

        public async Task<ServiceResult> CreateOrdreMissionWithAvanceVoyageAsDraft(OrdreMissionPostDTO ordreMission)
        {
            using var transaction = _context.Database.BeginTransaction();
            ServiceResult result = new();
            try
            {
                var mappedOM = _mapper.Map<OrdreMission>(ordreMission);
                result = await _ordreMissionRepository.AddOrdreMissionAsync(mappedOM);
                if (!result.Success) return result;

                StatusHistory OM_status = new()
                {
                    OrdreMissionId = mappedOM.Id,
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
                    if (ordreMission.ActualRequester == null) 
                    {
                        result.Success = false;
                        result.Message = "You must fill actual requester's information";
                        return result;
                    }

                    ActualRequester mappedActualRequester = _mapper.Map<ActualRequester>(ordreMission.ActualRequester);
                    mappedActualRequester.OrdreMissionId = mappedOM.Id;
                    mappedActualRequester.OrderingUserId = mappedOM.UserId;
                    result = await _actualRequesterService.AddActualRequesterInfoAsync(mappedActualRequester);

                }



                result = await CreateAvanceVoyageForEachCurrency(mappedOM, ordreMission.Trips, ordreMission.Expenses);



                await transaction.CommitAsync();

                result.Success = true;
                result.Message = "Created successfuly";
                return result;

            }
            catch (Exception exception)
            {
                transaction.Rollback();

                result.Success = false;
                result.Message = exception.Message;

                return result;

            }
        }

        public async Task<ServiceResult> SubmitOrdreMissionWithAvanceVoyage(int ordreMissionId)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();

            try
            {
                result = await _ordreMissionRepository.UpdateOrdreMissionStatusAsync(ordreMissionId, 1);
                if (!result.Success) return result;

                StatusHistory newOM_Status = new()
                {
                    OrdreMissionId = ordreMissionId,
                    Status = 1,
                };
                result = await _statusHistoryRepository.AddStatusAsync(newOM_Status);
                if (!result.Success)
                {
                    result.Message += " for the newly updated \"OrdreMission\"";
                    return result;
                }

                List<AvanceVoyage> avacesVoyage = await _avanceVoyageRepository.GetAvancesVoyageByOrdreMissionIdAsync(ordreMissionId);
                foreach(AvanceVoyage av in  avacesVoyage)
                {
                    result = await _avanceVoyageRepository.UpdateAvanceVoyageStatusAsync(av.Id, 1);
                    if (!result.Success)
                    {
                        result.Message = "Something went wrong while submitting \"AvanceVoyages\"";
                        return result;
                    };

                    StatusHistory newAV_Status = new()
                    {
                        AvanceVoyageId = av.Id,
                        Status = 1,
                    };

                    result = await _statusHistoryRepository.AddStatusAsync(newAV_Status);
                    if (!result.Success)
                    {
                        result.Message += " (Avancevoyage)";
                        return result;
                    }

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

        public async Task<ServiceResult> DecideOnOrdreMissionWithAvanceVoyage(int ordreMissionId,int deciderUserId,string? deciderComment,int decision)
        {

            ServiceResult result = new();

            //Test If decider already decided upon it by the decider or it has been forwarded to the next decider
            OrdreMission tempOM = await _ordreMissionRepository.GetOrdreMissionByIdAsync(ordreMissionId);
            int deciderRole_Request = await _userRepository.GetUserRoleByUserIdAsync(deciderUserId);
            int deciderRole_DB = 0;
            if(tempOM.DeciderUserId != null)
            {
                deciderRole_DB = await _userRepository.GetUserRoleByUserIdAsync((int)tempOM.DeciderUserId);
            }
            if (tempOM.DeciderUserId == deciderUserId || deciderRole_Request < deciderRole_DB)
            {
                result.Success = false;
                result.Message = "OrdreMission is already decided upon! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks";
                return result;
            }

            const int REJECTION_STATUS = 97;
            const int RETURN_STATUS = 98;

            // CASE: REJECTION
            if (decision == REJECTION_STATUS)
            {
                var transaction = _context.Database.BeginTransaction();
                try
                {
                    var decidedOrdreMission = await _ordreMissionRepository.GetOrdreMissionByIdAsync(ordreMissionId);
                    decidedOrdreMission.DeciderComment = deciderComment;
                    decidedOrdreMission.DeciderUserId = deciderUserId;
                    decidedOrdreMission.LatestStatus = REJECTION_STATUS;
                    result = await _ordreMissionRepository.UpdateOrdreMission(decidedOrdreMission);
                    if (!result.Success) return result;

                    StatusHistory decidedOrdreMission_SH = new()
                    {
                        OrdreMissionId = ordreMissionId,
                        DeciderUserId = deciderUserId,
                        DeciderComment = deciderComment,
                        Status = REJECTION_STATUS,
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(decidedOrdreMission_SH);
                    if (!result.Success) return result;

                    var decidedAvanceVoyage = await _avanceVoyageRepository.GetAvancesVoyageByOrdreMissionIdAsync(ordreMissionId);
                    foreach (AvanceVoyage av in decidedAvanceVoyage)
                    {
                        av.LatestStatus = REJECTION_STATUS;
                        av.DeciderComment = deciderComment;
                        av.DeciderUserId = deciderUserId;
                        result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(av);
                        if (!result.Success) return result;
                        StatusHistory decidedAvanceVoyage_SH = new()
                        {
                            AvanceVoyageId = av.Id,
                            DeciderUserId = deciderUserId,
                            DeciderComment = deciderComment,
                            Status = REJECTION_STATUS,
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
            // CASE: Return
            else if (decision == RETURN_STATUS)
            {
                var transaction = _context.Database.BeginTransaction();
                try
                {
                    var decidedOrdreMission = await _ordreMissionRepository.GetOrdreMissionByIdAsync(ordreMissionId);
                    decidedOrdreMission.DeciderComment = deciderComment;
                    decidedOrdreMission.DeciderUserId = deciderUserId;
                    decidedOrdreMission.LatestStatus = RETURN_STATUS; //returned status
                    result = await _ordreMissionRepository.UpdateOrdreMission(decidedOrdreMission);
                    if (!result.Success) return result;

                    StatusHistory decidedOrdreMission_SH = new()
                    {
                        OrdreMissionId = ordreMissionId,
                        DeciderUserId = deciderUserId,
                        DeciderComment = deciderComment,
                        Status = RETURN_STATUS,
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(decidedOrdreMission_SH);
                    if (!result.Success) return result;

                    var decidedAvanceVoyage = await _avanceVoyageRepository.GetAvancesVoyageByOrdreMissionIdAsync(ordreMissionId);
                    foreach (AvanceVoyage av in decidedAvanceVoyage)
                    {
                        av.LatestStatus = RETURN_STATUS;
                        av.DeciderComment = deciderComment;
                        av.DeciderUserId = deciderUserId;
                        result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(av);
                        if (!result.Success) return result;
                        StatusHistory decidedAvanceVoyage_SH = new()
                        {
                            AvanceVoyageId = av.Id,
                            DeciderUserId = deciderUserId,
                            DeciderComment = deciderComment,
                            Status = RETURN_STATUS,
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
            // CASE: Approve
            else
            {
                var transaction = _context.Database.BeginTransaction();
                try
                {
                    var decidedOrdreMission = await _ordreMissionRepository.GetOrdreMissionByIdAsync(ordreMissionId);
                    decidedOrdreMission.DeciderUserId = deciderUserId;
                    decidedOrdreMission.LatestStatus++; //Incrementig the status to apppear on the next decider table.
                    result = await _ordreMissionRepository.UpdateOrdreMission(decidedOrdreMission);
                    if (!result.Success) return result;

                    StatusHistory decidedOrdreMission_SH = new()
                    {
                        OrdreMissionId = ordreMissionId,
                        DeciderUserId = deciderUserId,
                        Status = decidedOrdreMission.LatestStatus,
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(decidedOrdreMission_SH);
                    if (!result.Success) return result;

                    var decidedAvanceVoyage = await _avanceVoyageRepository.GetAvancesVoyageByOrdreMissionIdAsync(ordreMissionId);
                    foreach (AvanceVoyage av in decidedAvanceVoyage)
                    {
                        if (av.LatestStatus == 5) av.LatestStatus = 7; //Preparing funds in case of TR Approval
                        av.LatestStatus++; //Otherwise next step of approval process
                        av.DeciderUserId = deciderUserId;
                        result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(av);
                        if (!result.Success) return result;

                        StatusHistory decidedAvanceVoyage_SH = new()
                        {
                            AvanceVoyageId = av.Id,
                            DeciderUserId = deciderUserId,
                            Status = av.LatestStatus,
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

            result.Success = true;
            result.Message = "OrdreMission & AvanceVoyage(s) are decided upon successfully";
            return result;

        }

        public async Task<ServiceResult> ModifyOrdreMission(OrdreMissionPostDTO ordreMission, int action) // action = 99 || 1
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();
            try
            {
                OrdreMission updatedOrdreMission = await _ordreMissionRepository.GetOrdreMissionByIdAsync(ordreMission.Id);
                if (updatedOrdreMission.LatestStatus != 98 && updatedOrdreMission.LatestStatus != 99)
                {
                    result.Success = false;
                    result.Message = "The OrdreMission you are trying to modify is not a draft nor returned! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks";
                    return result;
                }
                updatedOrdreMission.DeciderComment = null;
                updatedOrdreMission.DeciderUserId = null;
                updatedOrdreMission.LatestStatus = action;
                result = await _ordreMissionRepository.UpdateOrdreMission(updatedOrdreMission);
                if (!result.Success) return result;

                if(action == 1)
                {
                    StatusHistory OM_statusHistory = new()
                    {
                        OrdreMissionId = updatedOrdreMission.Id,
                        Status = 1
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(OM_statusHistory);
                    if (!result.Success) return result;
                }

                List<AvanceVoyage> DB_AvanceVoyages = await _avanceVoyageRepository.GetAvancesVoyageByOrdreMissionIdAsync(ordreMission.Id);

                AvanceVoyage avanceVoyage_MAD = new();
                AvanceVoyage avanceVoyage_EUR = new();

                if (DB_AvanceVoyages.Count == 2)
                {
                    if (DB_AvanceVoyages[0].Currency == "MAD")
                    {
                        avanceVoyage_MAD = DB_AvanceVoyages[0];
                        avanceVoyage_EUR = DB_AvanceVoyages[1];
                    }
                    else
                    {
                        avanceVoyage_MAD = DB_AvanceVoyages[1];
                        avanceVoyage_EUR = DB_AvanceVoyages[0];
                    }
                }
                else
                {
                    if (DB_AvanceVoyages[0].Currency == "MAD")
                    {
                        avanceVoyage_MAD = DB_AvanceVoyages[0];
                    }
                    else
                    {
                        avanceVoyage_EUR = DB_AvanceVoyages[0];
                    }
                }


                /*In case of adding new expenses in different curreny, and there is no AV that already contain that curreny
                 we will need to gather these expenses and add them later on along with a new AV since we can't add them without
                an AvId*/
                List<Expense> newCurrencyExpensesWithNoExistingAvanceVoyage = new();

                if (ordreMission.Expenses == null || ordreMission.Expenses.Count <= 0)
                {
                    List<Expense> expensesToDelete = await _expenseRepository.GetAvanceVoyageExpensesByAvIdAsync(DB_AvanceVoyages[0].Id);
                    if (DB_AvanceVoyages.Count == 2 && DB_AvanceVoyages[1] != null)
                    {
                        expensesToDelete.AddRange(await _expenseRepository.GetAvanceVoyageExpensesByAvIdAsync(DB_AvanceVoyages[1].Id));
                    }
                    //var expensesToDelete = _mapper.Map<List<Expense>>(ordreMission.Expenses);
                    result = await _expenseRepository.DeleteExpenses(expensesToDelete);
                    if (!result.Success) return result;
                }
                else
                {
                    //List of expenses Ids so that if an object exists in database and not in request list, we delete it
                    List<int> expensesIdsFromRequest = new();

                    foreach (ExpensePostDTO expense in ordreMission.Expenses)
                    {
                        expensesIdsFromRequest.Add(expense.Id);
                    }

                    // In case of existing expense in DB and not existant in request list, delete it
                    var DB_Expenses = await _expenseRepository.GetAvanceVoyageExpensesByAvIdAsync(DB_AvanceVoyages[0].Id);
                    if (DB_AvanceVoyages.Count == 2 && DB_AvanceVoyages[1] != null) DB_Expenses.AddRange(await _expenseRepository.GetAvanceVoyageExpensesByAvIdAsync(DB_AvanceVoyages[1].Id));

                    foreach (Expense expenseInDB in DB_Expenses)
                    {
                        if (!expensesIdsFromRequest.Contains(expenseInDB.Id))
                        {
                            await _expenseRepository.DeleteExpense(expenseInDB);
                        }
                    }


                    foreach (ExpensePostDTO expense in ordreMission.Expenses)
                    {
                        //If the expense is already in the DB update it
                        Expense expenseToUpdate = await _expenseRepository.FindExpenseAsync(expense.Id);
                        if (expenseToUpdate != null)
                        {
                            result = await _expenseRepository.UpdateExpense(expenseToUpdate);
                            if (!result.Success) return result;
                        }                        
                        //If it's not in database, insert it. If its currency is MAD add it to AV in MAD (if AV is null add the expense to the created list for new currency, it will be added later) - (same with EUR)
                        else
                        {
                            if(expense.Currency == "MAD")
                            {
                                if (avanceVoyage_MAD.Id != 0)
                                {
                                    Expense mappedExpense = _mapper.Map<Expense>(expense);
                                    mappedExpense.AvanceVoyageId = avanceVoyage_MAD.Id;
                                    result = await _expenseRepository.AddExpenseAsync(mappedExpense);
                                    if(!result.Success) return result;
                                }
                                else
                                {
                                    Expense mappedExpense = _mapper.Map<Expense>(expense);
                                    newCurrencyExpensesWithNoExistingAvanceVoyage.Add(mappedExpense);
                                }
                            }
                            else
                            {
                                if (avanceVoyage_EUR.Id != 0)
                                {
                                    Expense mappedExpense = _mapper.Map<Expense>(expense);
                                    mappedExpense.AvanceVoyageId = avanceVoyage_EUR.Id;
                                    result = await _expenseRepository.AddExpenseAsync(mappedExpense);
                                    if (!result.Success) return result;
                                }
                                else
                                {
                                    Expense mappedExpense = _mapper.Map<Expense>(expense);
                                    newCurrencyExpensesWithNoExistingAvanceVoyage.Add(mappedExpense);
                                }
                            }
                        }
                    }
                }

                //I think the list name is self explanatory - A list that will contain all trips with a currency that doesn't already have an AV in DB 
                List<Trip> newCurrencyTripsWithNoExistingAvanceVoyage = new();

                //List of trips Ids so that if an object exists in database and not in request list, we delete it
                List<int> tripsIdsFromRequest = new();
                foreach(TripPostDTO trip in ordreMission.Trips)
                {
                    tripsIdsFromRequest.Add(trip.Id);
                }

                // In case of existing trip in DB and not existant in request list, delete it
                var DB_Trips = await _tripRepository.GetAvanceVoyageTripsByAvIdAsync(DB_AvanceVoyages[0].Id);
                if (DB_AvanceVoyages.Count == 2 && DB_AvanceVoyages[1] != null) DB_Trips.AddRange(await _tripRepository.GetAvanceVoyageTripsByAvIdAsync(DB_AvanceVoyages[1].Id));

                foreach (Trip tripInDB in DB_Trips)
                {
                    if (!tripsIdsFromRequest.Contains(tripInDB.Id))
                    {
                        await _tripRepository.DeleteTrip(tripInDB);
                    }
                }

                foreach (TripPostDTO trip in ordreMission.Trips)
                {

                    //If the trip is already there update it
                    Trip tripToUpdate = await _tripRepository.FindTripAsync(trip.Id);
                    if (tripToUpdate != null)
                    {
                        result = await _tripRepository.UpdateTrip(tripToUpdate);
                        if (!result.Success) return result;
                    }
                    //If it's not in database, insert it. If its currency is MAD add it to AV in MAD (if AV is null add the expense to the created list for new currency, it will be added later) - (same with EUR)
                    else
                    {
                        if (trip.Unit == "MAD" || trip.Unit == "KM")
                        {
                            if (avanceVoyage_MAD.Id != 0)
                            {
                                Trip mappedTrip = _mapper.Map<Trip>(trip);

                                mappedTrip.AvanceVoyageId = avanceVoyage_MAD.Id;
                                mappedTrip.EstimatedFee = CalculateTripEstimatedFee(mappedTrip);
                                result = await _tripRepository.AddTripAsync(mappedTrip);
                                if (!result.Success) return result;
                            }
                            else
                            {
                                Trip mappedTrip = _mapper.Map<Trip>(trip);
                                mappedTrip.EstimatedFee = CalculateTripEstimatedFee(mappedTrip);
                                newCurrencyTripsWithNoExistingAvanceVoyage.Add(mappedTrip);
                            }
                        }
                        else
                        {
                            if (avanceVoyage_EUR.Id != 0)
                            {
                                Trip mappedTrip = _mapper.Map<Trip>(trip);
                                mappedTrip.AvanceVoyageId = avanceVoyage_EUR.Id;
                                mappedTrip.EstimatedFee = CalculateTripEstimatedFee(mappedTrip);
                                result = await _tripRepository.AddTripAsync(mappedTrip);
                                if (!result.Success) return result;
                            }
                            else
                            {
                                Trip mappedTrip = _mapper.Map<Trip>(trip);
                                mappedTrip.EstimatedFee = CalculateTripEstimatedFee(mappedTrip);
                                newCurrencyTripsWithNoExistingAvanceVoyage.Add(mappedTrip);
                            }
                        }

                    }
                }

                //Updating AvancesVoyage estimatedTotal and latest status props (existant in Database)
                foreach (AvanceVoyage avanceVoyage in DB_AvanceVoyages)
                {
                    decimal NewEstimatedTotal = CalculateTripsEstimatedTotal(await _tripRepository.GetAvanceVoyageTripsByAvIdAsync(avanceVoyage.Id))
                        + CalculateExpensesEstimatedTotal(await _expenseRepository.GetAvanceVoyageExpensesByAvIdAsync(avanceVoyage.Id));
                    avanceVoyage.EstimatedTotal = NewEstimatedTotal;
                    avanceVoyage.LatestStatus = action;
                    avanceVoyage.DeciderComment = null;
                    avanceVoyage.DeciderUserId = null;
                    result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyage);
                    if (!result.Success) return result;

                    if(action == 1)
                    {
                        StatusHistory AV_statusHistory = new()
                        {
                            AvanceVoyageId = avanceVoyage.Id,
                            Status = 1
                        };
                        result = await _statusHistoryRepository.AddStatusAsync(AV_statusHistory);
                        if (!result.Success) return result;
                    }
                }

                //Adding new AvanceVoyage in case user has updated expenses/trips with new currency
                if (newCurrencyExpensesWithNoExistingAvanceVoyage.Count > 0 || newCurrencyTripsWithNoExistingAvanceVoyage.Count > 0)
                {
                    AvanceVoyage avanceVoyageNewCurrency = new();

                    if (newCurrencyTripsWithNoExistingAvanceVoyage.Count > 0)
                    {
                        avanceVoyageNewCurrency.OrdreMissionId = ordreMission.Id;
                        avanceVoyageNewCurrency.LatestStatus = action;
                        avanceVoyageNewCurrency.EstimatedTotal = CalculateTripsEstimatedTotal(newCurrencyTripsWithNoExistingAvanceVoyage)
                                        + CalculateExpensesEstimatedTotal(newCurrencyExpensesWithNoExistingAvanceVoyage);
                        avanceVoyageNewCurrency.Currency = newCurrencyTripsWithNoExistingAvanceVoyage[0].Unit == "MAD"
                                    || newCurrencyTripsWithNoExistingAvanceVoyage[0].Unit == "KM" ? "MAD" : "EUR";
                        avanceVoyageNewCurrency.DeciderComment = null;
                        avanceVoyageNewCurrency.DeciderUserId = null;
                    }

                    if (newCurrencyExpensesWithNoExistingAvanceVoyage.Count > 0)
                    {
                        avanceVoyageNewCurrency.OrdreMissionId = ordreMission.Id;
                        avanceVoyageNewCurrency.LatestStatus = action;
                        avanceVoyageNewCurrency.EstimatedTotal = CalculateTripsEstimatedTotal(newCurrencyTripsWithNoExistingAvanceVoyage)
                                        + CalculateExpensesEstimatedTotal(newCurrencyExpensesWithNoExistingAvanceVoyage);
                        avanceVoyageNewCurrency.Currency = newCurrencyExpensesWithNoExistingAvanceVoyage[0].Currency;
                        avanceVoyageNewCurrency.DeciderComment = null;
                        avanceVoyageNewCurrency.DeciderUserId = null;
                    }

                    result = await _avanceVoyageRepository.AddAvanceVoyageAsync(avanceVoyageNewCurrency);
                    if (!result.Success) return result;

                    StatusHistory statusHistory = new()
                    {
                        AvanceVoyageId = avanceVoyageNewCurrency.Id,
                        Status = action
                    };

                    result = await _statusHistoryRepository.AddStatusAsync(statusHistory);
                    if (!result.Success) return result;

                    foreach (Expense expense in newCurrencyExpensesWithNoExistingAvanceVoyage)
                    {
                        expense.AvanceVoyageId = avanceVoyageNewCurrency.Id;
                        result = await _expenseRepository.AddExpenseAsync(expense);
                        if (!result.Success) return result;
                    }
                    foreach (Trip trip in newCurrencyTripsWithNoExistingAvanceVoyage)
                    {
                        trip.AvanceVoyageId = avanceVoyageNewCurrency.Id;
                        result = await _tripRepository.AddTripAsync(trip);
                        if (!result.Success) return result;
                    }
                }


                await transaction.CommitAsync();

            }
            catch (Exception exception)
            {
                result.Success = false;
                result.Message = exception.Message + exception.GetType() +exception.StackTrace ;
                return result;
            }

            if(action == 1)
            {
                result.Success = true;
                result.Message = "OrdreMission is resubmitted successfully";
                return result;
            }

            result.Success = true;
            result.Message = "Changes made to \"OrdreMission\" are saved successfully";
            return result;
        }

        public decimal CalculateTripEstimatedFee(Trip trip)
        {
            decimal estimatedFee = 0;
            const decimal MILEAGE_ALLOWANCE = 2.5m; // 2.5DH PER KM
            if (trip.Unit == "KM")
            {
                estimatedFee += trip.HighwayFee + (trip.Value * MILEAGE_ALLOWANCE);
            }
            else
            {
                estimatedFee += trip.Value;
            }
            

            return estimatedFee;
        }

        public decimal CalculateTripsEstimatedTotal(List<Trip> trips)
        {
            decimal estimatedTotal = 0;
            const decimal MILEAGE_ALLOWANCE = 2.5m; // 2.5DH PER KM
            foreach(Trip trip in trips)
            {
                if (trip.Unit == "KM")
                {
                    estimatedTotal += trip.HighwayFee + (trip.Value * MILEAGE_ALLOWANCE);
                }
                else
                {
                    estimatedTotal += trip.Value;
                }
            }

            return estimatedTotal;
        }

        public decimal CalculateExpensesEstimatedTotal(List<Expense> expenses)
        {
            decimal estimatedTotal = 0;
            foreach (Expense expense in expenses)
            {
                estimatedTotal += expense.EstimatedFee;
            }

            return estimatedTotal;
        }

    }
}
