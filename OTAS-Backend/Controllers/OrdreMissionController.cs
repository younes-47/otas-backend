using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OTAS.Data;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.Interfaces.IRepository;
using OTAS.Models;

namespace OTAS.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdreMissionController : ControllerBase
    {
        private readonly IOrdreMissionRepository _ordreMissionRepository;
        private readonly IAvanceVoyageRepository _avanceVoyageRepository;
        private readonly ITripRepository _tripRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IStatusHistoryRepository _statusHistoryRepository;
        private readonly IMapper _mapper;
        private readonly OtasContext _context;
        public OrdreMissionController(IOrdreMissionRepository ordreMissionRepository,
            IAvanceVoyageRepository avanceVoyageRepository,
            ITripRepository tripRepository,
            IExpenseRepository expenseRepository,
            IStatusHistoryRepository statusHistoryRepository,
            OtasContext context,
            IMapper mapper)
        {
            _ordreMissionRepository = ordreMissionRepository;
            _avanceVoyageRepository = avanceVoyageRepository;
            _tripRepository = tripRepository;
            _expenseRepository = expenseRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _context = context;
            _mapper = mapper;
        }

        /* This get /Table endpoint is within the Request section and it is shown for all the roles
         * since everyone can perform a request, it shows all the Missions ordered */
        //[HttpGet("Table")]
        //public IActionResult OrdreMissionTable(int status)
        //{
        //    var oms = _mapper.Map<List<OrdreMissionDTO>>(_ordreMissionRepository.GetOrdresMissionByRequesterUsername());

        //    if (!ModelState.IsValid)
        //        return BadRequest(ModelState);

        //    return Ok(oms);

        //}



        /* This post /Request endpoint is within the Request section and it is shown for all the roles
         * since everyone can perform a request */
        [HttpPost("Request")]
        public IActionResult RequestOrdreMission([FromBody] OrdreMissionRequestDTO ordreMissionRequest)
        {
   
            if(!ModelState.IsValid) return BadRequest(ModelState);
            using var transaction = _context.Database.BeginTransaction();


            try
            {
                var mappedOM = _mapper.Map<OrdreMission>(ordreMissionRequest);
                //mappedOM.User = _userRepository.GetUserByUserId(ordreMissionRequest.UserId);
                if (!_ordreMissionRepository.AddOrdreMission(mappedOM))
                {
                    ModelState.AddModelError("", "Something went wrong while saving the mission order");
                    return BadRequest(ModelState);
                }

                StatusHistory OM_status = new()
                {
                    //OrdreMission = mappedOM,
                    OrdreMissionId = mappedOM.Id,
                };
                if (!_statusHistoryRepository.AddStatus(OM_status))
                {
                    ModelState.AddModelError("", "Something went wrong while saving the Status of Mission Order in the status history table");
                    return BadRequest(ModelState);
                }


                ICollection<OrdreMissionTripOnRequestDTO> trips_in_mad = new List<OrdreMissionTripOnRequestDTO>();
                ICollection<OrdreMissionTripOnRequestDTO> trips_in_eur = new List<OrdreMissionTripOnRequestDTO>();

                ICollection<OrdreMissionExpenseOnRequestDTO> expenses_in_mad = new List<OrdreMissionExpenseOnRequestDTO>();
                ICollection<OrdreMissionExpenseOnRequestDTO> expenses_in_eur = new List<OrdreMissionExpenseOnRequestDTO>();

                // Sorti trips & expenses li bel MAD bo7dhom o li bel EUR bo7dhom
                foreach (OrdreMissionTripOnRequestDTO trip in ordreMissionRequest.Trips)
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

                foreach (OrdreMissionExpenseOnRequestDTO expense in ordreMissionRequest.Expenses)
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

                // trips & expenses li bel mad ghadi ykono related l' Avance de Voyage bo7dha
                if (trips_in_mad.Count > 0 || expenses_in_mad.Count > 0)
                {
                    decimal estm_total_mad = 0;

                    if (trips_in_mad.Count > 0)
                    {
                        // Factoring in total fees of the trip(s) in the estimated total of the whole "AvanceVoyage" in MAD
                        foreach (OrdreMissionTripOnRequestDTO trip in trips_in_mad)
                        {
                            estm_total_mad += trip.EstimatedFee;
                        }
                    }

                    if (expenses_in_mad.Count > 0)
                    {
                        // Factoring in total fees of the trip(s) in the estimated total of the whole "AvanceVoyage" in MAD
                        foreach (OrdreMissionExpenseOnRequestDTO expense in expenses_in_mad)
                        {
                            estm_total_mad += expense.EstimatedFee;
                        }
                    }

                    AvanceVoyage avanceVoyage_in_mad = new()
                    {
                        //Map the unmapped properties by Automapper
                        OrdreMissionId = mappedOM.Id,
                        //OrdreMission = mappedOM,
                        UserId = mappedOM.UserId,
                        //User = _userRepository.GetUserByUserId(mappedOM.UserId),
                        EstimatedTotal = estm_total_mad,
                        Currency = "MAD",
                    };

                    if (!_avanceVoyageRepository.AddAvanceVoyage(avanceVoyage_in_mad))
                    {
                        ModelState.AddModelError("", "Something went wrong while saving the AV_in_MAD");
                        return BadRequest(ModelState);
                    }

                    // Inserting the initial status of the "AvanceVoyage" in MAD in StatusHistory
                    StatusHistory AV_status = new()
                    {
                        AvanceVoyage = avanceVoyage_in_mad,
                    };
                    if (!_statusHistoryRepository.AddStatus(AV_status))
                    {
                        ModelState.AddModelError("", "Something went wrong while saving the Status of Mission Order in the status history table");
                        return BadRequest(ModelState);
                    }



                    // Inserting the trip(s) related to the "AvanceVoyage" in MAD (at least one trip)
                    var mappedTrips = _mapper.Map<List<Trip>>(trips_in_mad);
                    foreach (Trip mappedTrip in mappedTrips)
                    {
                        //mappedTrip.AvanceVoyage = avanceVoyage_in_mad;
                        mappedTrip.AvanceVoyageId = avanceVoyage_in_mad.Id;
                    }
                    if (!_tripRepository.AddTrips(mappedTrips))
                    {
                        ModelState.AddModelError("", "Something went wrong while saving the TRIPS_in_MAD");
                        return BadRequest(ModelState);
                    }

                    //There might be a case where an AV has no other expenses
                    if (ordreMissionRequest.Expenses.Count > 0)
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
                            ModelState.AddModelError("", "Something went wrong while saving the EXPENSES_in_MAD");
                            return BadRequest(ModelState);
                        }
                    }

                }


                // trips & expenses li bel eur ghadi ykono related l' Avance de Voyage bo7dha
                if (trips_in_eur.Count > 0 || expenses_in_eur.Count > 0)
                {
                    decimal estm_total_eur = 0;


                    if (trips_in_eur.Count > 0)
                    {
                        // Factoring in total fees of the trip(s) in the estimated total of the whole "AvanceVoyage" in EUR
                        foreach (OrdreMissionTripOnRequestDTO trip in trips_in_eur)
                        {
                            estm_total_eur += trip.EstimatedFee;
                        }
                    }

                    if (expenses_in_eur.Count > 0)
                    {
                        // Factoring in total fees of the expense(s) in the estimated total of the whole "AvanceVoyage" in EUR
                        foreach (OrdreMissionExpenseOnRequestDTO expense in expenses_in_eur)
                        {
                            estm_total_eur += expense.EstimatedFee;
                        }
                    }

                    // Inserting the "AvanceVoyage" in EUR
                    AvanceVoyage avanceVoyage_in_eur = new()
                    {
                        //Map the unmapped properties by Automapper
                        OrdreMissionId = mappedOM.Id,
                        OrdreMission = mappedOM,
                        UserId = mappedOM.UserId,
                        User = mappedOM.User,
                        EstimatedTotal = estm_total_eur,
                        Currency = "EUR",
                    };

                    if (!_avanceVoyageRepository.AddAvanceVoyage(avanceVoyage_in_eur))
                    {
                        ModelState.AddModelError("", "Something went wrong while saving the AV_in_EUR");
                        return BadRequest(ModelState);
                    }


                    // Inserting the initial status of the "AvanceVoyage" in EUR in StatusHistory
                    StatusHistory AV_status = new()
                    {
                        AvanceVoyage = avanceVoyage_in_eur,
                    };
                    if (!_statusHistoryRepository.AddStatus(AV_status))
                    {
                        ModelState.AddModelError("", "Something went wrong while saving the Status of Mission Order in the status history table");
                        return BadRequest(ModelState);
                    }




                    // Inserting the trips related to the "AvanceVoyage" in EUR
                    var mappedTrips = _mapper.Map<List<Trip>>(trips_in_eur);
                    foreach (Trip mappedTrip in mappedTrips)
                    {
                        if (mappedTrip.Unit == "EUR")
                        {
                            mappedTrip.AvanceVoyage = avanceVoyage_in_eur;
                            mappedTrip.AvanceVoyageId = avanceVoyage_in_eur.Id;
                        }
                    }
                    if (!_tripRepository.AddTrips(mappedTrips))
                    {
                        ModelState.AddModelError("", "Something went wrong while saving the TRIPS_in_EUR");
                        return BadRequest(ModelState);
                    }

                    //There might be a case where an AV has no other expenses
                    if (ordreMissionRequest.Expenses.Count > 0)
                    {
                        // Inserting the expenses related to the "AvanceVoyage" in EUR
                        var mappedExpenses = _mapper.Map<List<Expense>>(expenses_in_eur);
                        foreach (Expense mappedExpense in mappedExpenses)
                        {
                            mappedExpense.AvanceVoyage = avanceVoyage_in_eur;
                            mappedExpense.AvanceVoyageId = avanceVoyage_in_eur.Id;
                        }
                        if (!_expenseRepository.AddExpenses(mappedExpenses))
                        {
                            ModelState.AddModelError("", "Something went wrong while saving the EXPENSES_in_EUR");
                            return BadRequest(ModelState);
                        }

                    }


                }

                transaction.CommitAsync();

            }
            catch(Exception ex)
            {
                transaction.Rollback();
                return BadRequest(ex.Message);
            }



            return Ok("Mission Order has been submitted successfully");

        }





    }
}
