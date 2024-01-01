using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using OTAS.Data;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using OTAS.Repository;

namespace OTAS.Services
{
    public class LiquidationService : ILiquidationService
    {
        private readonly ILiquidationRepository _liquidationRepository;
        private readonly IAvanceVoyageRepository _avanceVoyageRepository;
        private readonly IAvanceCaisseRepository _avanceCaisseRepository;
        private readonly IStatusHistoryRepository _statusHistoryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMiscService _miscService;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ITripRepository _tripRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly OtasContext _context;

        public LiquidationService(ILiquidationRepository liquidationRepository,
            ITripRepository tripRepository,
            IExpenseRepository expenseRepository,
            IAvanceVoyageRepository avanceVoyageRepository,
            IAvanceCaisseRepository avanceCaisseRepository,
            IStatusHistoryRepository statusHistoryRepository,
            IWebHostEnvironment webHostEnvironment,
            IMiscService miscService,
            IUserRepository userRepository,
            IMapper mapper,
            OtasContext context)
        {
            _liquidationRepository = liquidationRepository;
            _avanceVoyageRepository = avanceVoyageRepository;
            _avanceCaisseRepository = avanceCaisseRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _webHostEnvironment = webHostEnvironment;
            _miscService = miscService;
            _userRepository = userRepository;
            _mapper = mapper;
            _tripRepository = tripRepository;
            _expenseRepository = expenseRepository;
            _context = context;
        }


        public async Task<ServiceResult> LiquidateAvanceVoyage(AvanceVoyage avanceVoyageDB, AvanceVoyageLiquidationPostDTO avanceVoyageLiquidation, int userId)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();

            try
            {
                decimal actualTotal = 0.0m;

                // Providing the actual spent amount the old trips
                foreach (TripLiquidationPostDTO tripRequest in avanceVoyageLiquidation.TripsLiquidations)
                {
                    var tripDB = await _tripRepository.GetTripAsync(tripRequest.TripId);
                    if(tripDB.Unit == "KM")
                    {
                        tripDB.HighwayFee = tripRequest.ActualHighwayFee;
                        tripDB.Value = tripRequest.ActualValue;
                        tripDB.ActualFee = _miscService.CalculateTripEstimatedFee(tripDB);
                        actualTotal += tripDB.ActualFee;
                    }
                    else
                    {
                        tripDB.ActualFee = tripRequest.ActualValue;
                        actualTotal += tripDB.ActualFee;
                    }

                    tripDB.UpdateDate = DateTime.Now;
                    result = await _tripRepository.UpdateTrip(tripDB);
                    if(!result.Success) return result;
                }

                // Providing the actual spent amount the old expenses
                foreach (ExpenseLiquidationPostDTO expenseRequest in avanceVoyageLiquidation.ExpensesLiquidations)
                {
                    var expenseDB = await _expenseRepository.GetExpenseAsync(expenseRequest.ExpenseId);
                    expenseDB.ActualFee = expenseRequest.ActualFee;
                    actualTotal += expenseDB.ActualFee;
                    expenseDB.UpdateDate = DateTime.Now;
                    result = await _expenseRepository.UpdateExpense(expenseDB);
                    if (!result.Success) return result;
                }

                // Adding new Trips
                foreach (TripPostDTO newTrip in avanceVoyageLiquidation.NewTrips)
                {
                    Trip mappedTrip = _mapper.Map<Trip>(newTrip);
                    mappedTrip.AvanceVoyageId = avanceVoyageLiquidation.AvanceVoyageId;
                    if(newTrip.Unit == "KM")
                    {
                        mappedTrip.Unit = newTrip.Unit;
                        mappedTrip.HighwayFee = newTrip.HighwayFee;
                        mappedTrip.ActualFee = _miscService.CalculateTripEstimatedFee(mappedTrip);
                        actualTotal += mappedTrip.ActualFee;
                    }
                    else
                    {
                        mappedTrip.Unit = avanceVoyageDB.Currency;
                        mappedTrip.ActualFee = newTrip.Value;
                        actualTotal -= mappedTrip.ActualFee;
                    }
                    result = await _tripRepository.AddTripAsync(mappedTrip);
                    if (!result.Success) return result;
                }

                // Adding new Expenses
                foreach (ExpensePostDTO newExpense in avanceVoyageLiquidation.NewExpenses)
                {
                    var mappedExpense = _mapper.Map<Expense>(newExpense);
                    mappedExpense.AvanceVoyageId = avanceVoyageLiquidation.AvanceVoyageId;
                    mappedExpense.Currency = avanceVoyageDB.Currency;
                    mappedExpense.ActualFee = newExpense.EstimatedFee;
                    actualTotal += mappedExpense.ActualFee;
                    result = await _expenseRepository.AddExpenseAsync(mappedExpense);
                    if (!result.Success) return result;
                }

                avanceVoyageDB.ActualTotal = actualTotal;
                result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyageDB);
                if (!result.Success) return result;

                Liquidation liquidation = new();

                liquidation.AvanceVoyageId = avanceVoyageLiquidation.AvanceVoyageId;
                liquidation.OnBehalf = avanceVoyageDB.OnBehalf;
                liquidation.ActualTotal = actualTotal;
                liquidation.Result = (int)(avanceVoyageDB.EstimatedTotal - actualTotal);


                /* _webHostEnvironment.WebRootPath == wwwroot\ (the default folder to store files) */
                var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Liquidation-Avance-Voyage-Receipts");
                if (!Directory.Exists(uploadsFolderPath))
                {
                    Directory.CreateDirectory(uploadsFolderPath);
                }
                /* Create file name by concatenating a random string + DC + username + .pdf extension */
                var user = await _userRepository.GetUserByUserIdAsync(userId);
                string uniqueReceiptsFileName = _miscService.GenerateRandomString(10) + "_DC_" + user.Username + ".pdf";
                /* Combine the folder path with the file name to create full path */
                var filePath = Path.Combine(uploadsFolderPath, uniqueReceiptsFileName);
                /* The creation of the file occurs after the commitment of DB changes */

                liquidation.ReceiptsFileName = uniqueReceiptsFileName;

                result = await _liquidationRepository.AddLiquidationAsync(liquidation);
                if (!result.Success) return result;

                // Status History
                StatusHistory DC_status = new()
                {
                    Total = liquidation.ActualTotal,
                    LiquidationId = liquidation.Id,
                    Status = liquidation.LatestStatus,
                };
                result = await _statusHistoryRepository.AddStatusAsync(DC_status);

                if (!result.Success)
                {
                    result.Message += " (depenseCaisse)";
                    return result;
                }

                await transaction.CommitAsync();

                /* Create the file if everything went as expected*/
                await System.IO.File.WriteAllBytesAsync(filePath, avanceVoyageLiquidation.ReceiptsFile);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message + ex.Source + ex.StackTrace;
                return result;
            }

            result.Success = true;
            result.Message = "Liquidation for this AvanceVoayage has been successfully saved!";
            return result;
        }

        public async Task<ServiceResult> LiquidateAvanceCaisse(AvanceCaisse avanceCaisseDB, AvanceCaisseLiquidationPostDTO avanceCaisseLiquidation, int userId)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();

            try
            {
                decimal actualTotal = 0.0m;

                // Providing the actual spent amount the old expenses
                foreach (ExpenseLiquidationPostDTO expenseRequest in avanceCaisseLiquidation.ExpensesLiquidations)
                {
                    var expenseDB = await _expenseRepository.GetExpenseAsync(expenseRequest.ExpenseId);
                    expenseDB.ActualFee = expenseRequest.ActualFee;
                    actualTotal += expenseDB.ActualFee;
                    expenseDB.UpdateDate = DateTime.Now;
                    result = await _expenseRepository.UpdateExpense(expenseDB);
                    if (!result.Success) return result;
                }


                // Adding new Expenses
                foreach (ExpensePostDTO newExpense in avanceCaisseLiquidation.NewExpenses)
                {
                    var mappedExpense = _mapper.Map<Expense>(newExpense);
                    mappedExpense.AvanceCaisseId = avanceCaisseLiquidation.AvanceCaisseId;
                    mappedExpense.Currency = avanceCaisseDB.Currency;
                    mappedExpense.ActualFee = newExpense.EstimatedFee;
                    actualTotal += mappedExpense.ActualFee;
                    result = await _expenseRepository.AddExpenseAsync(mappedExpense);
                    if (!result.Success) return result;
                }

                avanceCaisseDB.ActualTotal = actualTotal;
                result = await _avanceCaisseRepository.UpdateAvanceCaisseAsync(avanceCaisseDB);
                if (!result.Success) return result;

                Liquidation liquidation = new();

                liquidation.AvanceCaisseId = avanceCaisseLiquidation.AvanceCaisseId;
                liquidation.OnBehalf = avanceCaisseDB.OnBehalf;
                liquidation.ActualTotal = actualTotal;
                liquidation.Result = (int)(avanceCaisseDB.EstimatedTotal - actualTotal);


                /* _webHostEnvironment.WebRootPath == wwwroot\ (the default folder to store files) */
                var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Liquidation-Avance-Voyage-Receipts");
                if (!Directory.Exists(uploadsFolderPath))
                {
                    Directory.CreateDirectory(uploadsFolderPath);
                }
                /* Create file name by concatenating a random string + DC + username + .pdf extension */
                var user = await _userRepository.GetUserByUserIdAsync(userId);
                string uniqueReceiptsFileName = _miscService.GenerateRandomString(10) + "_DC_" + user.Username + ".pdf";
                /* Combine the folder path with the file name to create full path */
                var filePath = Path.Combine(uploadsFolderPath, uniqueReceiptsFileName);
                /* The creation of the file occurs after the commitment of DB changes */

                liquidation.ReceiptsFileName = uniqueReceiptsFileName;

                result = await _liquidationRepository.AddLiquidationAsync(liquidation);
                if (!result.Success) return result;

                // Status History
                StatusHistory DC_status = new()
                {
                    Total = liquidation.ActualTotal,
                    LiquidationId = liquidation.Id,
                    Status = liquidation.LatestStatus,
                };
                result = await _statusHistoryRepository.AddStatusAsync(DC_status);

                if (!result.Success)
                {
                    result.Message += " (depenseCaisse)";
                    return result;
                }

                await transaction.CommitAsync();

                /* Create the file if everything went as expected*/
                await System.IO.File.WriteAllBytesAsync(filePath, avanceCaisseLiquidation.ReceiptsFile);
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message + ex.Source + ex.StackTrace;
                return result;
            }

            result.Success = true;
            result.Message = "Liquidation for this AvanceVoayage has been successfully saved!";
            return result;
        }

    }
}
