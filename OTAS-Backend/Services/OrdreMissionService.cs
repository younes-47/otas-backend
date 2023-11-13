using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OTAS.Data;
using OTAS.DTO.Post;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using OTAS.Repository;
using System.Collections.Generic;

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
        private readonly OtasContext _context;
        private readonly IMapper _mapper;
        //private readonly ILogger _logger;

        public OrdreMissionService
            (
                IActualRequesterService actualRequester,
                IAvanceVoyageRepository avanceVoyageRepository,
                IOrdreMissionRepository ordreMissionRepository,
                ITripRepository tripRepository,
                IExpenseRepository expenseRepository,
                IStatusHistoryRepository statusHistoryRepository,
                OtasContext context,
                IMapper mapper
                //ILogger logger
            ) 
        {
            _actualRequesterService = actualRequester;
            _avanceVoyageRepository = avanceVoyageRepository;
            _ordreMissionRepository = ordreMissionRepository;
            _tripRepository = tripRepository;
            _expenseRepository = expenseRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _context = context;
            _mapper = mapper;
            //_logger = logger;
        }

        public async Task<ServiceResult> CreateAvanceVoyageForEachCurrency(OrdreMission mappedOM, List<TripPostDTO> mixedTrips, List<ExpensePostDTO> mixedExpenses)
        {
            ServiceResult result = new();

            // Sort Trips
            List<TripPostDTO> trips_in_mad = new();
            List<TripPostDTO> trips_in_eur = new();

            foreach (TripPostDTO trip in mixedTrips)
            {
                if (trip.Unit == "MAD")
                {
                    trips_in_mad.Add(trip);
                }
                else if (trip.Unit == "EUR")
                {
                    trips_in_eur.Add(trip);
                }
            }

            // Sort Expenses
            List<ExpensePostDTO> expenses_in_mad = new();
            List<ExpensePostDTO> expenses_in_eur = new();

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
                        estm_total_mad += trip.EstimatedFee;
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

                if (! await _avanceVoyageRepository.AddAvanceVoyageAsync(avanceVoyage_in_mad))
                {
                    ServiceResult serviceResult = new()
                    {
                        Success = false,
                        ErrorMessage = "Something went wrong while saving the AV in MAD"
                    };

                    return serviceResult;
                }

                // Inserting the initial status of the "AvanceVoyage" in MAD in StatusHistory
                StatusHistory AV_status = new()
                {
                    AvanceVoyage = avanceVoyage_in_mad,
                };
                if (!await _statusHistoryRepository.AddStatusAsync(AV_status))
                {
                    result.Success = false;
                    result.ErrorMessage = "Something went wrong while saving the Status of Mission Order in the status history table.";
                    return result;
                }


                //There might be a case where an AV has no other expenses
                if (trips_in_mad.Count > 0)
                {
                    // Inserting the trip(s) related to the "AvanceVoyage" in MAD (at least one trip)
                    var mappedTrips = _mapper.Map<List<Trip>>(trips_in_mad);
                    foreach (Trip mappedTrip in mappedTrips)
                    {
                        mappedTrip.AvanceVoyageId = avanceVoyage_in_mad.Id;
                    }
                    if (!_tripRepository.AddTrips(mappedTrips))
                    {
                        result.Success = false;
                        result.ErrorMessage = "Something went wrong while saving the trips in MAD.";
                        return result;
                    }
                }


                //There might be a case where an AV has no other expenses
                if (expenses_in_mad.Count > 0)
                {
                    // Inserting the expense(s) related to the "AvanceVoyage" in MAD
                    var mappedExpenses = _mapper.Map<List<Expense>>(expenses_in_mad);
                    foreach (Expense mappedExpense in mappedExpenses)
                    {
                        //mappedExpense.AvanceVoyage = avanceVoyage_in_mad;
                        mappedExpense.AvanceVoyageId = avanceVoyage_in_mad.Id;
                    }
                    if (!_expenseRepository.AddExpenses(mappedExpenses))
                    {
                        result.Success = false;
                        result.ErrorMessage = "Something went wrong while saving the expenses in MAD.";
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
                        estm_total_eur += trip.EstimatedFee;
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

                if (!await _avanceVoyageRepository.AddAvanceVoyageAsync(avanceVoyage_in_eur))
                {
                    result.Success = false;
                    result.ErrorMessage = "Something went wrong while saving the AV in EUR.";
                    return result;
                }

                // Inserting the initial status of the "AvanceVoyage" in EUR in StatusHistory
                StatusHistory AV_status = new()
                {
                    AvanceVoyage = avanceVoyage_in_eur,
                };
                if (! await _statusHistoryRepository.AddStatusAsync(AV_status))
                {
                    result.Success = false;
                    result.ErrorMessage = "Something went wrong while saving the Status of Mission Order in the status history table.";
                    return result;

                }

                // There might be a case where an "OrdreMission" has no trip in EUR
                if(trips_in_eur.Count > 0)
                {
                    // Inserting the trips related to the "AvanceVoyage" in EUR
                    var mappedTrips = _mapper.Map<List<Trip>>(trips_in_eur);
                    foreach (Trip mappedTrip in mappedTrips)
                    {
                        if (mappedTrip.Unit == "EUR")
                        {
                            mappedTrip.AvanceVoyageId = avanceVoyage_in_eur.Id;
                        }
                    }
                    if (!_tripRepository.AddTrips(mappedTrips))
                    {
                        result.Success = false;
                        result.ErrorMessage = "Something went wrong while saving the trips in EUR.";
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
                    if (!_expenseRepository.AddExpenses(mappedExpenses))
                    {
                        result.Success = false;
                        result.ErrorMessage = "Something went wrong while saving the expenses in EUR.";
                        return result;

                    }
                }
            }
            result.Success = true;
            result.SuccessMessage = "Avance de voyage has been submitted successfully";
            return result;
        }

        public async Task<ServiceResult> CreateOrdreMissionWithAvanceVoyageAsDraft(OrdreMissionPostDTO ordreMission)
        {
            using var transaction = _context.Database.BeginTransaction();
            ServiceResult result = new();
            try
            {
                var mappedOM = _mapper.Map<OrdreMission>(ordreMission);
                
                if (! await _ordreMissionRepository.AddOrdreMissionAsync(mappedOM))
                {
                    result.Success = false;
                    result.ErrorMessage = "Something went wrong while saving \"OrdreMission\"";
                    return result;
                }

                StatusHistory OM_status = new()
                {
                    OrdreMissionId = mappedOM.Id,
                };
                if (!await _statusHistoryRepository.AddStatusAsync(OM_status))
                {
                    result.Success = false;
                    result.ErrorMessage = "Something went wrong while saving the Status of \"OrdreMission\" in the StatusHistory table";
                    return result;
                }

                // Handle Actual Requester Info (if exists)
                if (ordreMission.OnBehalf == 1)
                {
                    if (ordreMission.ActualRequester == null) 
                    {
                        result.Success = false;
                        result.ErrorMessage = "You must fill actual requester's information";
                        return result;
                    }

                    ActualRequester mappedActualRequester = _mapper.Map<ActualRequester>(ordreMission.ActualRequester);
                    mappedActualRequester.OrdreMissionId = mappedOM.Id;
                    mappedActualRequester.OrderingUserId = mappedOM.UserId;
                    result = await _actualRequesterService.AddActualRequesterInfo(mappedActualRequester);

                }



                result = await CreateAvanceVoyageForEachCurrency(mappedOM, ordreMission.Trips, ordreMission.Expenses);



                await transaction.CommitAsync();

                result.Success = true;
                result.SuccessMessage = "Created successfuly";
                return result;

            }
            catch (Exception exception)
            {
                transaction.Rollback();

                result.Success = false;
                result.ErrorMessage = exception.Message;

                return result;

            }
        }

        public async Task<ServiceResult> SubmitOrdreMissionWithAvanceVoyage(int ordreMissionId)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();

            try
            {
                await _ordreMissionRepository.UpdateOrdreMissionStatusAsync(ordreMissionId, 0);

                StatusHistory newOM_Status = new()
                {
                    OrdreMissionId = ordreMissionId,
                    Status = 1,
                };
                //_logger.LogInformation($"newOM_Status details: {newOM_Status}");

                if (!await _statusHistoryRepository.AddStatusAsync(newOM_Status))
                {
                    result.Success = false;
                    result.ErrorMessage = "Something went wrong while saving StatusHistory for the newly updated \"OrdreMission\"!";
                    return result;
                }

                List<AvanceVoyage> avacesVoyage = await _avanceVoyageRepository.GetAvancesVoyageByOrdreMissionIdAsync(ordreMissionId);
                foreach(AvanceVoyage av in  avacesVoyage)
                {
                    result = await _avanceVoyageRepository.UpdateAvanceVoyageStatusAsync(av.Id, 0);
                    if (!result.Success)
                    {
                        result.ErrorMessage = "Something went wrong while submitting \"AvanceVoyages\"";
                        return result;
                    };

                    StatusHistory newAV_Status = new()
                    {
                        AvanceVoyageId = av.Id,
                        Status = 1,
                    };
                    System.Diagnostics.Debug.WriteLine(newAV_Status.Status);
                    //_logger.LogInformation($"newAV_Status details: {newAV_Status}");
                    if (!await _statusHistoryRepository.AddStatusAsync(newAV_Status))
                    {
                        result.Success = false;
                        result.ErrorMessage = "Something went wrong while saving StatusHistory for \"AvanceVoyage\"!";
                        return result;
                    }
                }

                await transaction.CommitAsync();
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.ErrorMessage = exception.Message;
                return result;
            }

            result.Success = true;
            result.SuccessMessage = "\"OrdreMission\" and its corresponding \"Avance(s)Voyae\" are Submitted successfuly";
            return result;

        }
    }
}
