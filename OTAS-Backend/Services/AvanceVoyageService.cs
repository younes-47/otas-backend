﻿using AutoMapper;
using OTAS.Data;
using OTAS.DTO.Post;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using OTAS.Repository;
using System;

namespace OTAS.Services
{
    public class AvanceVoyageService : IAvanceVoyageService
    {
        private readonly IAvanceVoyageRepository _avanceVoyageRepository;
        private readonly IOrdreMissionRepository _ordreMissionRepository;
        private readonly IUserRepository _userRepository;
        private readonly IDeciderRepository _deciderRepository;
        private readonly IStatusHistoryRepository _statusHistoryRepository;
        private readonly ITripRepository _tripRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly OtasContext _context;
        private readonly IMapper _mapper;

        public AvanceVoyageService(IAvanceVoyageRepository avanceVoyageRepository,
            IOrdreMissionRepository ordreMissionRepository,
            IUserRepository userRepository,
            IDeciderRepository deciderRepository,
            IStatusHistoryRepository statusHistoryRepository,
            ITripRepository tripRepository,
            IExpenseRepository expenseRepository,
            OtasContext context, 
            IMapper mapper)
        {
            _avanceVoyageRepository = avanceVoyageRepository;
            _ordreMissionRepository = ordreMissionRepository;
            _userRepository = userRepository;
            _deciderRepository = deciderRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _tripRepository = tripRepository;
            _expenseRepository = expenseRepository;
            _context = context;
            _mapper = mapper;
        }




        public async Task<ServiceResult> DecideOnAvanceVoyage(DecisionOnRequestPostDTO decision)
        {
            ServiceResult result = new();

            AvanceVoyage decidedAvanceVoyage = await _avanceVoyageRepository.GetAvanceVoyageByIdAsync(decision.RequestId);

            //Test if the request is on a state where the decider is not supposed to decide upon it
            if (decidedAvanceVoyage.NextDeciderUserId != decision.DeciderUserId)
            {
                result.Success = false;
                result.Message = "You can't decide on this request in this state! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks";
                return result;
            }

            bool IS_LONGER_THAN_ONEDAY = true;

            var tempOM = await _ordreMissionRepository.GetOrdreMissionByIdAsync(decidedAvanceVoyage.OrdreMissionId);
            if (tempOM.ReturnDate.Day == tempOM.DepartureDate.Day) IS_LONGER_THAN_ONEDAY = false;


            if (decision.DecisionString.ToLower() == "return" || decision.DecisionString.ToLower() == "reject")
            {
                var transaction = _context.Database.BeginTransaction();
                try
                {
                    decidedAvanceVoyage.DeciderComment = decision.DeciderComment;
                    decidedAvanceVoyage.DeciderUserId = decision.DeciderUserId;
                    if (decision.ReturnedToFMByTR)
                    {
                        decidedAvanceVoyage.NextDeciderUserId = await _avanceVoyageRepository.GetAvanceVoyageNextDeciderUserId("TR", null, decision.ReturnedToFMByTR, null);
                        decidedAvanceVoyage.LatestStatus = 14; /* Returned to finance dept for missing info */
                    } 
                    else if (decision.ReturnedToTRByFM)
                    {
                        decidedAvanceVoyage.NextDeciderUserId = await _avanceVoyageRepository.GetAvanceVoyageNextDeciderUserId("FM", null, null, decision.ReturnedToTRByFM);
                        decidedAvanceVoyage.LatestStatus = 6; /* pending TR decision */
                    }
                    else
                    {
                        decidedAvanceVoyage.NextDeciderUserId = null; /* next decider is set to null if returned or rejected by deciders other than TR */
                        decidedAvanceVoyage.LatestStatus = decision.DecisionString.ToLower() == "return" ? 98 : 97; /* returned normaly or rejected */
                    }
                    result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(decidedAvanceVoyage);
                    if (!result.Success) return result;

                    StatusHistory decidedAvanceVoyage_SH = new()
                    {
                        AvanceVoyageId = decision.RequestId,
                        DeciderUserId = decision.DeciderUserId,
                        DeciderComment = decision.DeciderComment,
                        Status = decidedAvanceVoyage.LatestStatus,
                        NextDeciderUserId = decidedAvanceVoyage.NextDeciderUserId,
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(decidedAvanceVoyage_SH);
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
            }
            // CASE: Approval
            else
            {
                var transaction = _context.Database.BeginTransaction();
                try
                {
                    decidedAvanceVoyage.DeciderUserId = decision.DeciderUserId;

                    var deciderLevel = await _deciderRepository.GetDeciderLevelByUserId(decision.DeciderUserId);
                    switch (deciderLevel)
                    {
                        case "MG":
                            decidedAvanceVoyage.NextDeciderUserId = await _avanceVoyageRepository.GetAvanceVoyageNextDeciderUserId("MG");
                            decidedAvanceVoyage.LatestStatus = 3;
                            break;
                        case "FM":
                            decidedAvanceVoyage.NextDeciderUserId = await _avanceVoyageRepository.GetAvanceVoyageNextDeciderUserId("FM");
                            decidedAvanceVoyage.LatestStatus = 4;
                            break;
                        case "GD":
                            decidedAvanceVoyage.NextDeciderUserId = await _avanceVoyageRepository.GetAvanceVoyageNextDeciderUserId("GD", IS_LONGER_THAN_ONEDAY);
                            decidedAvanceVoyage.LatestStatus = IS_LONGER_THAN_ONEDAY ? 5 : 8;  /* if it is not longer than one day skip VP*/
                            break;
                        case "VP":
                            decidedAvanceVoyage.NextDeciderUserId = await _avanceVoyageRepository.GetAvanceVoyageNextDeciderUserId("VP");
                            decidedAvanceVoyage.LatestStatus = 8;
                            break;
                    }

                    result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(decidedAvanceVoyage);
                    if (!result.Success) return result;

                    StatusHistory decidedAvanceVoyage_SH = new()
                    {
                        AvanceVoyageId = decision.RequestId,
                        DeciderUserId = decision.DeciderUserId,
                        DeciderComment = decision.DeciderComment,
                        Status = decidedAvanceVoyage.LatestStatus,
                        NextDeciderUserId = decidedAvanceVoyage.NextDeciderUserId,
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(decidedAvanceVoyage_SH);
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
            result.Message = "AvanceVoyage(s) are decided upon successfully";
            return result;
        }

        public async Task<ServiceResult> ModifyAvanceVoyagesForEachCurrency(OrdreMission ordreMission, List<AvanceVoyage> avanceVoyages, List<TripPostDTO> mixedTrips, List<ExpensePostDTO>? mixedExpenses, string action)
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
                else
                {
                    result.Success = false;
                    result.Message = "Invalid unit in one of the trips!";
                    return result;
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
                else
                {
                    result.Success = false;
                    result.Message = "Invalid currency in one of the expenses!";
                    return result;
                }
            }

            // If there is no trips AND expenses in X currency and we actually have an AV in DB that has an estimated Total in that currency, we need to zero that value
            if ((trips_in_mad.Count == 0 && expenses_in_mad.Count == 0) || (trips_in_eur.Count == 0 && expenses_in_eur.Count == 0)) //&& (avanceVoyages[0].Currency == "MAD" || avanceVoyages.Count == 2)
            {
                //test mad
                if (trips_in_mad.Count == 0 && expenses_in_mad.Count == 0)
                {
                    //Identify in which index in the list the AV in mad is stored
                    if (avanceVoyages[0].Currency == "MAD")
                    {
                        avanceVoyages[0].EstimatedTotal = 0.0m;

                        if (action.ToLower() != "save") avanceVoyages[0].NextDeciderUserId = ordreMission.NextDeciderUserId;
                        result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyages[0]);
                        if (action.ToLower() != "save")
                        {
                            StatusHistory AV_statusHistory = new()
                            {
                                Total = avanceVoyages[0].EstimatedTotal,
                                NextDeciderUserId = avanceVoyages[0].NextDeciderUserId,
                                AvanceVoyageId = avanceVoyages[0].Id,
                                Status = 1
                            };
                            result = await _statusHistoryRepository.AddStatusAsync(AV_statusHistory);
                            if (!result.Success) return result;
                        }
                        if (!result.Success)
                        {
                            result.Message += " (MAD)";
                            return result;
                        }
                    }
                    if (avanceVoyages.Count == 2)
                    {
                        if (avanceVoyages[1].Currency == "MAD")
                        {
                            avanceVoyages[1].EstimatedTotal = 0.0m;
                            if (action.ToLower() != "save") avanceVoyages[1].NextDeciderUserId = ordreMission.NextDeciderUserId;
                            result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyages[1]);
                            if (action.ToLower() != "save")
                            {
                                StatusHistory AV_statusHistory = new()
                                {
                                    Total = avanceVoyages[1].EstimatedTotal,
                                    NextDeciderUserId = avanceVoyages[1].NextDeciderUserId,
                                    AvanceVoyageId = avanceVoyages[1].Id,
                                    Status = 1
                                };
                                result = await _statusHistoryRepository.AddStatusAsync(AV_statusHistory);
                                if (!result.Success) return result;
                            }
                            if (!result.Success)
                            {
                                result.Message += " (MAD)";
                                return result;
                            }
                        }
                    }

                }

                //test eur
                if (trips_in_eur.Count == 0 && expenses_in_eur.Count == 0)
                {
                    //Identify in which index in the list the AV in eur is stored
                    if (avanceVoyages[0].Currency == "EUR")
                    {
                        avanceVoyages[0].EstimatedTotal = 0.0m;
                        if (action.ToLower() != "save") avanceVoyages[0].NextDeciderUserId = ordreMission.NextDeciderUserId;
                        result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyages[0]);
                        if (action.ToLower() != "save")
                        {
                            StatusHistory AV_statusHistory = new()
                            {
                                Total = avanceVoyages[0].EstimatedTotal,
                                NextDeciderUserId = avanceVoyages[0].NextDeciderUserId,
                                AvanceVoyageId = avanceVoyages[0].Id,
                                Status = 1
                            };
                            result = await _statusHistoryRepository.AddStatusAsync(AV_statusHistory);
                            if (!result.Success) return result;
                        }
                        if (!result.Success)
                        {
                            result.Message += " (EUR)";
                            return result;
                        }
                    }
                    if (avanceVoyages.Count == 2)
                    {
                        if (avanceVoyages[1].Currency == "EUR")
                        {
                            avanceVoyages[1].EstimatedTotal = 0.0m;
                            if (action.ToLower() != "save") avanceVoyages[1].NextDeciderUserId = ordreMission.NextDeciderUserId;
                            result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyages[1]);
                            if (action.ToLower() != "save")
                            {
                                StatusHistory AV_statusHistory = new()
                                {
                                    Total = avanceVoyages[0].EstimatedTotal,
                                    NextDeciderUserId = avanceVoyages[0].NextDeciderUserId,
                                    AvanceVoyageId = avanceVoyages[0].Id,
                                    Status = 1
                                };
                                result = await _statusHistoryRepository.AddStatusAsync(AV_statusHistory);
                                if (!result.Success) return result;
                            }
                            if (!result.Success)
                            {
                                result.Message += " (EUR)";
                                return result;
                            }
                        }
                    }

                }

            }


            // Now after we sorted the trips & expenses, we will create two AV objects just in case if we need to insert them if a new Currency is detected
            AvanceVoyage avanceVoyage_in_mad = new();
            AvanceVoyage avanceVoyage_in_eur = new();

            // trips & expenses li bel MAD ghadi ykono related l' Avance de Voyage bo7dha
            if (trips_in_mad.Count > 0 || expenses_in_mad.Count > 0)
            {
                decimal estm_total_mad = 0.0m;

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
                    // Factoring in total fees of the expense(s) in the estimated total of the whole "AvanceVoyage" in MAD
                    foreach (ExpensePostDTO expense in expenses_in_mad)
                    {
                        estm_total_mad += expense.EstimatedFee;
                    }
                }

                // In case of AV in mad is already existant in index 0
                if (avanceVoyages[0].Currency == "MAD")
                {
                    avanceVoyages[0].EstimatedTotal = estm_total_mad;
                    if (action.ToLower() != "save") avanceVoyages[0].NextDeciderUserId = ordreMission.NextDeciderUserId;
                    result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyages[0]);
                    if (action.ToLower() != "save")
                    {
                        StatusHistory AV_statusHistory = new()
                        {
                            Total = avanceVoyages[0].EstimatedTotal,
                            NextDeciderUserId = avanceVoyages[0].NextDeciderUserId,
                            AvanceVoyageId = avanceVoyages[0].Id,
                            Status = 1
                        };
                        result = await _statusHistoryRepository.AddStatusAsync(AV_statusHistory);
                        if (!result.Success) return result;
                    }
                    if (!result.Success)
                    {
                        result.Message += " (MAD)";
                        return result;
                    }
                }
                // In case of AV in mad is already existant in index 1
                else if (avanceVoyages.Count == 2)
                {
                    if (avanceVoyages[1].Currency == "MAD")
                    {
                        avanceVoyages[1].EstimatedTotal = estm_total_mad;
                        if (action.ToLower() != "save") avanceVoyages[1].NextDeciderUserId = ordreMission.NextDeciderUserId;
                        result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyages[1]);
                        if (action.ToLower() != "save")
                        {
                            StatusHistory AV_statusHistory = new()
                            {
                                Total = avanceVoyages[1].EstimatedTotal,
                                NextDeciderUserId = avanceVoyages[1].NextDeciderUserId,
                                AvanceVoyageId = avanceVoyages[1].Id,
                                Status = 1
                            };
                            result = await _statusHistoryRepository.AddStatusAsync(AV_statusHistory);
                            if (!result.Success) return result;
                        }
                        if (!result.Success)
                        {
                            result.Message += " (MAD)";
                            return result;
                        }
                    }
                }
                // In case of AV in mad doesn't exist in DB -> we map & insert the already initiated one with a new Statushistory
                else
                {
                    //Map
                    avanceVoyage_in_mad.OrdreMissionId = ordreMission.Id;
                    avanceVoyage_in_mad.UserId = ordreMission.UserId;
                    avanceVoyage_in_mad.EstimatedTotal = estm_total_mad;
                    avanceVoyage_in_mad.Currency = "MAD";

                    if (action.ToLower() != "save") avanceVoyage_in_mad.NextDeciderUserId = ordreMission.NextDeciderUserId;
                    result = await _avanceVoyageRepository.AddAvanceVoyageAsync(avanceVoyage_in_mad);
                    if (!result.Success)
                    {
                        result.Message += " (MAD)";
                        return result;
                    }
                    StatusHistory AV_MAD_Status = new()
                    {
                        NextDeciderUserId = avanceVoyage_in_mad.NextDeciderUserId,
                        Total = avanceVoyage_in_mad.EstimatedTotal,
                        AvanceVoyageId = avanceVoyage_in_mad.Id,
                        Status = avanceVoyage_in_mad.LatestStatus
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(AV_MAD_Status);
                    if (!result.Success)
                    {
                        result.Message += " (Avancevoyage in mad)";
                        return result;
                    }
                }

                /*
                * Now we map and insert the new expenses coming from the request & associate them with an appropiate AV 
                */

                // we check the trips in mad count cuz There might be a case where an AV has no Trips in mad
                if (trips_in_mad.Count > 0)
                {
                    // Inserting the trip(s) related to the "AvanceVoyage" in MAD (at least one trip)
                    var mappedTrips = _mapper.Map<List<Trip>>(trips_in_mad);
                    foreach (Trip mappedTrip in mappedTrips)
                    {
                        if (avanceVoyages[0].Currency == "MAD")
                        {
                            mappedTrip.AvanceVoyageId = avanceVoyages[0].Id;
                        }
                        else if (avanceVoyages.Count == 2)
                        {
                            if (avanceVoyages[1].Currency == "MAD")
                            {
                                mappedTrip.AvanceVoyageId = avanceVoyages[1].Id;
                            }
                        }
                        else
                        {
                            mappedTrip.AvanceVoyageId = avanceVoyage_in_mad.Id;
                        }
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
                        if (avanceVoyages[0].Currency == "MAD")
                        {
                            mappedExpense.AvanceVoyageId = avanceVoyages[0].Id;
                        }
                        else if (avanceVoyages.Count == 2)
                        {
                            if (avanceVoyages[1].Currency == "MAD")
                            {
                                mappedExpense.AvanceVoyageId = avanceVoyages[1].Id;
                            }
                        }
                        else
                        {
                            mappedExpense.AvanceVoyageId = avanceVoyage_in_mad.Id;
                        }
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

                // In case of an existing AV in eur in DB in index 0
                if (avanceVoyages[0].Currency == "EUR")
                {
                    avanceVoyages[0].EstimatedTotal = estm_total_eur;
                    if (action.ToLower() != "save") avanceVoyages[0].NextDeciderUserId = ordreMission.NextDeciderUserId;
                    result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyages[0]);
                    if (action.ToLower() != "save")
                    {
                        StatusHistory AV_statusHistory = new()
                        {
                            Total = avanceVoyages[0].EstimatedTotal,
                            NextDeciderUserId = avanceVoyages[0].NextDeciderUserId,
                            AvanceVoyageId = avanceVoyages[0].Id,
                            Status = 1
                        };
                        result = await _statusHistoryRepository.AddStatusAsync(AV_statusHistory);
                        if (!result.Success) return result;
                    }
                    if (!result.Success)
                    {
                        result.Message += " (EUR)";
                        return result;
                    }
                }
                // In case of AV in eur is already existant in index 1
                else if (avanceVoyages.Count == 2)
                {
                    if (avanceVoyages[1].Currency == "EUR")
                    {
                        avanceVoyages[1].EstimatedTotal = estm_total_eur;
                        if (action.ToLower() != "save") avanceVoyages[1].NextDeciderUserId = ordreMission.NextDeciderUserId;
                        if (action.ToLower() != "save")
                        {
                            StatusHistory AV_statusHistory = new()
                            {
                                Total = avanceVoyages[1].EstimatedTotal,
                                NextDeciderUserId = avanceVoyages[1].NextDeciderUserId,
                                AvanceVoyageId = avanceVoyages[1].Id,
                                Status = 1
                            };
                            result = await _statusHistoryRepository.AddStatusAsync(AV_statusHistory);
                            if (!result.Success) return result;
                        }
                        result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyages[1]);
                        if (!result.Success)
                        {
                            result.Message += " (EUR)";
                            return result;
                        }
                    }
                }
                // In case of AV in eur doesn't exist in DB -> we insert the already initiated one with new Statushistory
                else
                {
                    //Map the unmapped properties by Automapper
                    avanceVoyage_in_eur.OrdreMissionId = ordreMission.Id;
                    avanceVoyage_in_eur.UserId = ordreMission.UserId;
                    avanceVoyage_in_eur.EstimatedTotal = estm_total_eur;
                    avanceVoyage_in_eur.Currency = "EUR";

                    if (action.ToLower() != "save") avanceVoyage_in_eur.NextDeciderUserId = ordreMission.NextDeciderUserId;
                    result = await _avanceVoyageRepository.AddAvanceVoyageAsync(avanceVoyage_in_mad);
                    if (!result.Success)
                    {
                        result.Message += " (EUR)";
                        return result;
                    }
                    StatusHistory AV_EUR_Status = new()
                    {
                        Total = avanceVoyage_in_eur.EstimatedTotal,
                        AvanceVoyageId = avanceVoyage_in_eur.Id,
                        Status = avanceVoyage_in_eur.LatestStatus,
                        NextDeciderUserId = avanceVoyage_in_eur.NextDeciderUserId
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(AV_EUR_Status);
                    if (!result.Success)
                    {
                        result.Message += " (Avancevoyage in EUR)";
                        return result;
                    }
                }


                /*
                * Now we map and insert the new expenses coming from the request & associate them with an appropiate AV 
                */


                // There might be a case where an "OrdreMission" has no trip in EUR
                if (trips_in_eur.Count > 0)
                {
                    // Inserting the trips related to the "AvanceVoyage" in EUR
                    List<Trip> mappedTrips = _mapper.Map<List<Trip>>(trips_in_eur);
                    foreach (Trip mappedTrip in mappedTrips)
                    {
                        if (avanceVoyages[0].Currency == "EUR")
                        {
                            mappedTrip.AvanceVoyageId = avanceVoyages[0].Id;
                        }
                        else if (avanceVoyages.Count == 2)
                        {
                            if (avanceVoyages[1].Currency == "EUR")
                            {
                                mappedTrip.AvanceVoyageId = avanceVoyages[1].Id;
                            }
                        }
                        else
                        {
                            mappedTrip.AvanceVoyageId = avanceVoyage_in_eur.Id;
                        }
                        mappedTrip.EstimatedFee = CalculateTripEstimatedFee(mappedTrip);
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
                        if (avanceVoyages[0].Currency == "EUR")
                        {
                            mappedExpense.AvanceVoyageId = avanceVoyages[0].Id;
                        }
                        else if (avanceVoyages.Count == 2)
                        {
                            if (avanceVoyages[1].Currency == "EUR")
                            {
                                mappedExpense.AvanceVoyageId = avanceVoyages[1].Id;
                            }
                        }
                        else
                        {
                            mappedExpense.AvanceVoyageId = avanceVoyage_in_eur.Id;
                        }
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

        public async Task<ServiceResult> CreateAvanceVoyageForEachCurrency(OrdreMission ordreMission, List<TripPostDTO> mixedTrips, List<ExpensePostDTO>? mixedExpenses)
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
                else
                {
                    result.Success = false;
                    result.Message = "Invalid unit in one of the trips!";
                    return result;
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
                else
                {
                    result.Success = false;
                    result.Message = "Invalid currency in one of the expenses!";
                    return result;
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
                    OrdreMissionId = ordreMission.Id,
                    UserId = ordreMission.UserId,
                    EstimatedTotal = estm_total_mad,
                    Currency = "MAD",
                };

                result = await _avanceVoyageRepository.AddAvanceVoyageAsync(avanceVoyage_in_mad);
                if (!result.Success)
                {
                    result.Message += " (MAD)";
                    return result;
                }

                // Inserting the initial status of the "AvanceVoyage" in MAD in StatusHistory in case of CREATE
                StatusHistory AV_status = new()
                {
                    Total = avanceVoyage_in_mad.EstimatedTotal,
                    AvanceVoyageId = avanceVoyage_in_mad.Id,
                    Status = avanceVoyage_in_mad.LatestStatus
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
                    OrdreMissionId = ordreMission.Id,
                    UserId = ordreMission.UserId,
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
                    Total = avanceVoyage_in_eur.EstimatedTotal,
                    AvanceVoyageId = avanceVoyage_in_eur.Id,
                    Status = avanceVoyage_in_eur.LatestStatus
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

        public async Task<ServiceResult> SubmitAvanceVoyage(int avanceVoyageId)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();

            try
            {
                var av = await _avanceVoyageRepository.GetAvanceVoyageByIdAsync(avanceVoyageId);
                var user = await _userRepository.GetUserByUserIdAsync(av.UserId);
                int managerUserId = await _deciderRepository.GetManagerUserIdByUserIdAsync(user.Id);
                result = await _avanceVoyageRepository.UpdateAvanceVoyageStatusAsync(avanceVoyageId, 1, managerUserId);
                if (!result.Success) return result;

                StatusHistory newOM_Status = new()
                {
                    NextDeciderUserId = managerUserId,
                    AvanceVoyageId = avanceVoyageId,
                    Total = av.EstimatedTotal,
                    Status = 1,
                };
                result = await _statusHistoryRepository.AddStatusAsync(newOM_Status);
                if (!result.Success)
                {
                    result.Message += " for the newly submitted \"AvanceVoyage\"";
                    return result;
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
            result.Message = "AvanceVoyage is Submitted successfully";
            return result;

        }


        // These functions should be in their respective services but whatever
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
            foreach (Trip trip in trips)
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