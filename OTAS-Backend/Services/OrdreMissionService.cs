using AutoMapper;
using OTAS.Data;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
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
        private readonly IOrdreMissionRepository _ordreMissionRepository;
        private readonly ITripRepository _tripRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IStatusHistoryRepository _statusHistoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly OtasContext _context;
        private readonly IMapper _mapper;

        public OrdreMissionService
            (
                IActualRequesterRepository actualRequesterRepository,
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
            _actualRequesterRepository = actualRequesterRepository;
            _avanceVoyageRepository = avanceVoyageRepository;
            _ordreMissionRepository = ordreMissionRepository;
            _tripRepository = tripRepository;
            _expenseRepository = expenseRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _userRepository = userRepository;
            _context = context;
            _mapper = mapper;
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

        public async Task<ServiceResult> UpdateAvanceVoyageForEachCurrency(OrdreMission ordreMission, List<AvanceVoyage> avanceVoyages, List<TripPostDTO> mixedTrips, List<ExpensePostDTO>? mixedExpenses)
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

            // If there is no trips or expenses in X currency and we actually have an AV in DB that has an estimated Total in that currency, we need to zero that value
            if((trips_in_mad.Count == 0 && expenses_in_mad.Count == 0) || (trips_in_eur.Count == 0 && expenses_in_eur.Count == 0)) //&& (avanceVoyages[0].Currency == "MAD" || avanceVoyages.Count == 2)
            { 
                //test mad
                if(trips_in_mad.Count == 0 && expenses_in_mad.Count == 0)
                {
                    //Identify in which index in the list the AV in mad is stored
                    if (avanceVoyages[0].Currency == "MAD")
                    {
                        avanceVoyages[0].EstimatedTotal = 0.0m;
                        result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyages[0]);
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
                            result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyages[0]);
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
                        result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyages[0]);
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
                            result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyages[0]);
                            if (!result.Success)
                            {
                                result.Message += " (EUR)";
                                return result;
                            }
                        }
                    }

                }

            }


            // Now after we sorted the trips & expenses, we will create two AV objects just in case if we need to insert them in case of new Currency is detected
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
                    result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyages[0]);
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
                        result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyages[0]);
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

                    result = await _avanceVoyageRepository.AddAvanceVoyageAsync(avanceVoyage_in_mad);
                    if (!result.Success)
                    {
                        result.Message += " (MAD)";
                        return result;
                    }
                    StatusHistory AV_MAD_Status = new()
                    {
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
                    result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyages[0]);
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
                        result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyages[0]);
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
                        Status = avanceVoyage_in_eur.LatestStatus
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

        public async Task<ServiceResult> CreateOrdreMissionWithAvanceVoyageAsDraft(OrdreMissionPostDTO ordreMission)
        {
            using var transaction = _context.Database.BeginTransaction();
            ServiceResult result = new();

            // Bad requests handling
            if (ordreMission.Trips.Count < 2)
            {
                result.Success = false;
                result.Message = "OrdreMission must have at least two trips!";
                return result;
            }

            var sortedTrips = ordreMission.Trips.OrderBy(trip => trip.DepartureDate).ToList();
            for( int i = 0; i< sortedTrips.Count; i++)
            {
                if (sortedTrips[i].DepartureDate > sortedTrips[i].ArrivalDate)
                {
                    result.Success = false;
                    result.Message = "One of the trips has a departure date bigger than its arrival date!";
                    return result;
                }
                if (i == sortedTrips.Count - 1) break; // prevent out of range index exception
                if (sortedTrips[i].ArrivalDate > sortedTrips[i + 1].DepartureDate)
                {
                    result.Success = false;
                    result.Message = "Trips dates don't make sense! You can't start another trip before you arrive from the previous one.";
                    return result;
                }
            }

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
                    if (ordreMission.ActualRequester == null)
                    {
                        result.Success = false;
                        result.Message = "You must fill actual requester's info in case you are filing this request on behalf of someone";
                        return result;
                    }

                    ActualRequester mappedActualRequester = _mapper.Map<ActualRequester>(ordreMission.ActualRequester);
                    mappedActualRequester.OrdreMissionId = mappedOM.Id;
                    mappedActualRequester.OrderingUserId = mappedOM.UserId;
                    result = await _actualRequesterRepository.AddActualRequesterInfoAsync(mappedActualRequester);
                    if (!result.Success) return result;
                }

                result = await CreateAvanceVoyageForEachCurrency(mappedOM, ordreMission.Trips, ordreMission.Expenses);
                if (!result.Success) return result;
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
                    result.Message += " for the newly submitted \"OrdreMission\"";
                    return result;
                }

                List<AvanceVoyage> avacesVoyage = await _avanceVoyageRepository.GetAvancesVoyageByOrdreMissionIdAsync(ordreMissionId);
                foreach (AvanceVoyage av in avacesVoyage)
                {
                    result = await _avanceVoyageRepository.UpdateAvanceVoyageStatusAsync(av.Id, 1);
                    if (!result.Success)
                    {
                        result.Message = "Something went wrong while submitting \"AvanceVoyages\"";
                        return result;
                    };

                    StatusHistory newAV_Status = new()
                    {
                        Total = av.EstimatedTotal,
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

        public async Task<ServiceResult> DecideOnOrdreMissionWithAvanceVoyage(int ordreMissionId, string? advanceOption, int deciderUserId, string? deciderComment, int decision)
        {

            ServiceResult result = new();
            OrdreMission tempOM = await _ordreMissionRepository.GetOrdreMissionByIdAsync(ordreMissionId);

            //Test if the request is not on a state where you can't decide upon (99, 98, 97)
            if (tempOM.LatestStatus == 99 || tempOM.LatestStatus == 98 || tempOM.LatestStatus == 97)
            {
                result.Success = false;
                result.Message = "You can't decide on this request in this state! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks";
                return result;
            }


            //Test If decider already decided upon it by the decider or it has been forwarded to the next decider
            int deciderRole_Request = await _userRepository.GetUserRoleByUserIdAsync(deciderUserId);
            int deciderRole_DB = 0;
            if (tempOM.DeciderUserId != null)
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

            // CASE: REJECTION / RETURN
            if (decision == REJECTION_STATUS || decision == RETURN_STATUS)
            {
                var transaction = _context.Database.BeginTransaction();
                try
                {
                    var decidedOrdreMission = await _ordreMissionRepository.GetOrdreMissionByIdAsync(ordreMissionId);
                    decidedOrdreMission.DeciderComment = deciderComment;
                    decidedOrdreMission.DeciderUserId = deciderUserId;
                    decidedOrdreMission.LatestStatus = decision;
                    result = await _ordreMissionRepository.UpdateOrdreMission(decidedOrdreMission);
                    if (!result.Success) return result;

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
                    foreach (AvanceVoyage av in decidedAvanceVoyage)
                    {
                        av.LatestStatus = decision;
                        av.DeciderComment = deciderComment;
                        av.DeciderUserId = deciderUserId;
                        result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(av);
                        if (!result.Success) return result;
                        StatusHistory decidedAvanceVoyage_SH = new()
                        {
                            Total = av.EstimatedTotal,
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
            else
            {
                var transaction = _context.Database.BeginTransaction();
                try
                {
                    var decidedOrdreMission = await _ordreMissionRepository.GetOrdreMissionByIdAsync(ordreMissionId);
                    if (await _userRepository.GetUserRoleByUserIdAsync(deciderUserId) != (decidedOrdreMission.LatestStatus + 2))
                    // Here the role of the decider should always be equal to the latest status of the request + 2 (refer to one note). If not, he is trying to approve on a different level from his.
                    {
                        result.Success = false;
                        result.Message = "You are not allowed to approve on this level!";
                    }
                    decidedOrdreMission.DeciderUserId = deciderUserId;
                    decidedOrdreMission.LatestStatus++; //Incrementig the status to apppear on the next decider table.
                    result = await _ordreMissionRepository.UpdateOrdreMission(decidedOrdreMission);
                    if (!result.Success) return result;

                    StatusHistory decidedOrdreMission_SH = new()
                    {
                        OrdreMissionId = ordreMissionId,
                        DeciderUserId = deciderUserId,
                        DeciderComment = deciderComment,
                        Status = decidedOrdreMission.LatestStatus,
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(decidedOrdreMission_SH);
                    if (!result.Success) return result;

                    var decidedAvanceVoyage = await _avanceVoyageRepository.GetAvancesVoyageByOrdreMissionIdAsync(ordreMissionId);
                    foreach (AvanceVoyage av in decidedAvanceVoyage)
                    {
                        if (av.LatestStatus == 5) av.LatestStatus = 7; //Preparing funds in case of TR Approval (6 is approved, and it will be assigned only for OM not AV)
                        av.LatestStatus++; // Otherwise next step of approval process
                        av.DeciderUserId = deciderUserId;
                        av.AdvanceOption = advanceOption;
                        result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(av);
                        if (!result.Success) return result;

                        StatusHistory decidedAvanceVoyage_SH = new()
                        {
                            Total = av.EstimatedTotal,
                            AvanceVoyageId = av.Id,
                            DeciderUserId = deciderUserId,
                            DeciderComment = deciderComment,
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

        public async Task<ServiceResult> ModifyOrdreMissionWithAvanceVoyage(OrdreMissionPostDTO ordreMission, int action) // action = 99 || 1
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();
            try
            {
                OrdreMission updatedOrdreMission = await _ordreMissionRepository.GetOrdreMissionByIdAsync(ordreMission.Id);

                /*Validations*/
                if (updatedOrdreMission.LatestStatus == 97)
                {
                    result.Success = false;
                    result.Message = "You can't modify or resubmit a rejected request!";
                    return result;
                }

                if (updatedOrdreMission.LatestStatus != 98 && updatedOrdreMission.LatestStatus != 99)
                {
                    result.Success = false;
                    result.Message = "The OrdreMission you are trying to modify is not a draft nor returned! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks";
                    return result;
                }

                if (action == 99 && updatedOrdreMission.LatestStatus == 98)
                {
                    result.Success = false;
                    result.Message = "You can't modify and save a returned request as a draft again. You may want to apply your modifications to you request and resubmit it directly";
                    return result;
                }

                if(ordreMission.Trips.Count < 1)
                {
                    result.Success = false;
                    result.Message = "OrdreMission must have at least one trip! You can't request an OrdreMission with no trip.";
                    return result;
                }

                // Handle Onbehalf case
                if (ordreMission.OnBehalf == true)
                {
                    if (ordreMission.ActualRequester == null)
                    {
                        result.Success = false;
                        result.Message = "You must fill actual requester's info in case you are filling this request on behalf of someone";
                        return result;
                    }

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
                    mappedActualRequester.OrderingUserId = ordreMission.UserId;
                    result = await _actualRequesterRepository.AddActualRequesterInfoAsync(mappedActualRequester);
                    if (!result.Success) return result;
                }
                else
                {
                    ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByOrdreMissionIdAsync(ordreMission.Id);
                    if (actualRequesterInfo != null)
                    {
                        //Make sure to delete actualrequester in case the user modifies "OnBehalf" Prop to false
                        result = await _actualRequesterRepository.DeleteActualRequesterInfoAsync(actualRequesterInfo);
                        if (!result.Success) return result;
                    }
                }

                updatedOrdreMission.DeciderComment = null;
                updatedOrdreMission.DeciderUserId = null;
                updatedOrdreMission.LatestStatus = action;
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

                result = await _ordreMissionRepository.UpdateOrdreMission(updatedOrdreMission);
                if (!result.Success) return result;

                if (action == 1)
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
                
                // Call the local function which will take care of the rest
                result = await UpdateAvanceVoyageForEachCurrency(updatedOrdreMission, DB_AvanceVoyages, ordreMission.Trips, ordreMission.Expenses);
                if(!result.Success) return result;


                await transaction.CommitAsync();

            }
            catch (Exception exception)
            {
                result.Success = false;
                result.Message = exception.Message + exception.GetType() + exception.StackTrace;
                return result;
            }

            if (action == 1)
            {
                result.Success = true;
                result.Message = "OrdreMission is resubmitted successfully";
                return result;
            }

            result.Success = true;
            result.Message = "Changes made to \"OrdreMission\" are saved successfully";
            return result;
        }

        public async Task<List<OrdreMissionDTO>> GetOrdreMissionsForDeciderTable(int userId)
        {
            int deciderRole = await _userRepository.GetUserRoleByUserIdAsync(userId);
            List<OrdreMission> ordreMissions = await _ordreMissionRepository.GetOrdresMissionByStatusAsync(deciderRole - 1); //See oneNote sketches to understand why it is role-1
            List<OrdreMissionDTO> mappedOrdreMissions = _mapper.Map<List<OrdreMissionDTO>>(ordreMissions);
            return mappedOrdreMissions;
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
