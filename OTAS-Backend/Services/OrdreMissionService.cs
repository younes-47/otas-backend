using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OTAS.Data;
using OTAS.DTO.Post;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using OTAS.Repository;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

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


                //There might be a case where an AV has no other expenses
                if (trips_in_mad.Count > 0)
                {
                    // Inserting the trip(s) related to the "AvanceVoyage" in MAD (at least one trip)
                    var mappedTrips = _mapper.Map<List<Trip>>(trips_in_mad);
                    foreach (Trip mappedTrip in mappedTrips)
                    {
                        mappedTrip.AvanceVoyageId = avanceVoyage_in_mad.Id;
                    }
                    result = await _tripRepository.AddTripsAsync(mappedTrips);
                    if (!result.Success)
                    {
                        result.Message = "(MAD).";
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
                        result.Message = "Something went wrong while saving the expenses in MAD.";
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
                    if (!_expenseRepository.AddExpenses(mappedExpenses))
                    {
                        result.Success = false;
                        result.Message = "Something went wrong while saving the expenses in EUR.";
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
                
                if (! await _ordreMissionRepository.AddOrdreMissionAsync(mappedOM))
                {
                    result.Success = false;
                    result.Message = "Something went wrong while saving \"OrdreMission\"";
                    return result;
                }

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
                if (ordreMission.OnBehalf == 1)
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

        public async Task<ServiceResult> DecideOnOrdreMissionWithAvanceVoyage
            (int ordreMissionId,
             int deciderUserId,
             string? deciderComment,
             int decision)
        {
            ServiceResult result = new();

            // CASE: REJECTION
            if (decision == 97)
            {
                var transaction = _context.Database.BeginTransaction();
                try
                {
                    var decidedOrdreMission = await _ordreMissionRepository.GetOrdreMissionByIdAsync(ordreMissionId);
                    decidedOrdreMission.DeciderComment = deciderComment;
                    decidedOrdreMission.DeciderUserId = deciderUserId;
                    decidedOrdreMission.LatestStatus = decision;
                    result = await _ordreMissionRepository.UpdateOrdreMission(decidedOrdreMission);
                    if(!result.Success) return result;

                    StatusHistory decidedOrdreMission_SH = new()
                    {
                        OrdreMissionId = ordreMissionId,
                        DeciderUserId = deciderUserId,
                        DeciderComment = deciderComment,
                        Status = decision,
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(decidedOrdreMission_SH);
                    if (!result.Success) return result;
                    
                    var decidedAvanceVoyage = await _avanceVoyageRepository.GetAvancesVoyageByOrdreMissionIdAsync(ordreMissionId);
                    foreach(AvanceVoyage av in  decidedAvanceVoyage)
                    {
                        av.LatestStatus = decision;
                        av.DeciderComment = deciderComment;
                        av.DeciderUserId = deciderUserId;
                        result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(av);
                        if (!result.Success) return result;
                        StatusHistory decidedAvanceVoyage_SH = new()
                        {
                            AvanceVoyageId = av.Id,
                            DeciderUserId = deciderUserId,
                            DeciderComment = deciderComment,
                            Status = decision,
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
            else if (decision == 98)
            {
                
            }
            // CASE: Approve
            else
            {
                
            }
            result.Success = true;
            result.Message = "OrdreMission & AvanceVoyage(s) are decided upon successfully";
            return result;
        }
    }
}
