﻿using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Identity.Web;
using OTAS.Data;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.DTO.Put;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using OTAS.Repository;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Xceed.Document.NET;
using Xceed.Words.NET;

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
        private readonly IDeciderRepository _deciderRepository;
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
            IDeciderRepository deciderRepository,
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
            _deciderRepository = deciderRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _tripRepository = tripRepository;
            _expenseRepository = expenseRepository;
            _context = context;
        }


        public async Task<ServiceResult> LiquidateAvanceVoyage(AvanceVoyage avanceVoyageDB, LiquidationPostDTO avanceVoyageLiquidation, int userId)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();

            try
            {
                decimal actualTotal = 0.0m;

                // Providing the actual spent amount the old trips
                foreach (TripLiquidationPutDTO tripRequest in avanceVoyageLiquidation.TripsLiquidations)
                {
                    var tripDB = await _tripRepository.GetTripAsync(tripRequest.TripId);
                    tripDB.ActualFee = tripRequest.ActualFee;
                    actualTotal += tripDB.ActualFee;
                    tripDB.UpdateDate = DateTime.Now;
                    result = await _tripRepository.UpdateTrip(tripDB);
                    if(!result.Success) return result;
                }

                // Providing the actual spent amount the old expenses
                foreach (ExpenseLiquidationPutDTO expenseRequest in avanceVoyageLiquidation.ExpensesLiquidations)
                {
                    var expenseDB = await _expenseRepository.GetExpenseAsync(expenseRequest.ExpenseId);
                    expenseDB.ActualFee = expenseRequest.ActualFee;
                    actualTotal += expenseDB.ActualFee;
                    expenseDB.UpdateDate = DateTime.Now;
                    result = await _expenseRepository.UpdateExpense(expenseDB);
                    if (!result.Success) return result;
                }


                Liquidation liquidation = new();

                liquidation.AvanceVoyageId = avanceVoyageLiquidation.RequestId;
                liquidation.OnBehalf = avanceVoyageDB.OnBehalf;
                liquidation.ActualTotal = actualTotal;
                liquidation.Result = (int)(actualTotal - avanceVoyageDB.EstimatedTotal);
                liquidation.UserId = avanceVoyageDB.UserId;
                liquidation.Currency = avanceVoyageDB.Currency;




                avanceVoyageDB.ActualTotal = actualTotal;
                result = await _avanceVoyageRepository.UpdateAvanceVoyageAsync(avanceVoyageDB);
                if (!result.Success) return result;


                /* _webHostEnvironment.WebRootPath == wwwroot\ (the default folder to store files) */
                var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Liquidation-Avance-Voyage-Receipts");
                if (!Directory.Exists(uploadsFolderPath))
                {
                    Directory.CreateDirectory(uploadsFolderPath);
                }
                /* Create file name by concatenating a random string + DC + username + .pdf extension */
                var user = await _userRepository.GetUserByUserIdAsync(userId);
                string uniqueReceiptsFileName = _miscService.GenerateRandomString(10) + "_LQ_AV_" + user.Username + ".pdf";
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

                // Adding new Expenses
                foreach (var newExpense in avanceVoyageLiquidation.NewExpenses)
                {
                    var mappedExpense = _mapper.Map<Expense>(newExpense);
                    mappedExpense.LiquidationId = liquidation.Id;
                    mappedExpense.Currency = avanceVoyageDB.Currency;
                    mappedExpense.ActualFee = newExpense.EstimatedFee;
                    actualTotal += mappedExpense.ActualFee;
                    result = await _expenseRepository.AddExpenseAsync(mappedExpense);
                    if (!result.Success) return result;
                }

                // Adding new Trips
                foreach (TripPostDTO newTrip in avanceVoyageLiquidation.NewTrips)
                {
                    Trip mappedTrip = _mapper.Map<Trip>(newTrip);
                    mappedTrip.LiquidationId = liquidation.Id;
                    if (newTrip.Unit == "KM")
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



                if (!result.Success)
                {
                    result.Message += " (liquidation)";
                    return result;
                }

                result.Id = liquidation.Id;

                await transaction.CommitAsync();

                /* Create the file if everything went as expected*/
                await System.IO.File.WriteAllBytesAsync(filePath, avanceVoyageLiquidation.ReceiptsFile);
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.Message = exception.Message + " ERROR TYPE: " + exception.GetType();
                return result;
            }

            result.Success = true;
            result.Message = "Liquidation for this AvanceVoyage has been successfully saved!";
            return result;
        }

        public async Task<ServiceResult> LiquidateAvanceCaisse(AvanceCaisse avanceCaisseDB, LiquidationPostDTO avanceCaisseLiquidation, int userId)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();

            try
            {
                decimal actualTotal = 0.0m;

                // Providing the actual spent amount for the old expenses
                foreach (ExpenseLiquidationPutDTO expenseRequest in avanceCaisseLiquidation.ExpensesLiquidations)
                {
                    var expenseDB = await _expenseRepository.GetExpenseAsync(expenseRequest.ExpenseId);
                    expenseDB.ActualFee = expenseRequest.ActualFee;
                    actualTotal += expenseDB.ActualFee;
                    expenseDB.UpdateDate = DateTime.Now;
                    result = await _expenseRepository.UpdateExpense(expenseDB);
                    if (!result.Success) return result;
                }


                avanceCaisseDB.ActualTotal = actualTotal;
                result = await _avanceCaisseRepository.UpdateAvanceCaisseAsync(avanceCaisseDB);
                if (!result.Success) return result;

                Liquidation liquidation = new();

                liquidation.AvanceCaisseId = avanceCaisseLiquidation.RequestId;
                liquidation.OnBehalf = avanceCaisseDB.OnBehalf;
                liquidation.ActualTotal = actualTotal;
                liquidation.Result = (int)(actualTotal - avanceCaisseDB.EstimatedTotal);
                liquidation.UserId = avanceCaisseDB.UserId;
                liquidation.Currency = avanceCaisseDB.Currency;

                /* _webHostEnvironment.WebRootPath == wwwroot\ (the default folder to store files) */
                var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Liquidation-Avance-Caisse-Receipts");
                if (!Directory.Exists(uploadsFolderPath))
                {
                    Directory.CreateDirectory(uploadsFolderPath);
                }
                /* Create file name by concatenating a random string + DC + username + .pdf extension */
                var user = await _userRepository.GetUserByUserIdAsync(userId);
                string uniqueReceiptsFileName = _miscService.GenerateRandomString(10) + "_LQ_AC_" + user.Username + ".pdf";
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

                // Adding new Expenses
                foreach (var newExpense in avanceCaisseLiquidation.NewExpenses)
                {
                    var mappedExpense = _mapper.Map<Expense>(newExpense);
                    mappedExpense.LiquidationId = liquidation.Id;
                    mappedExpense.Currency = avanceCaisseDB.Currency;
                    mappedExpense.ActualFee = newExpense.EstimatedFee;
                    actualTotal += mappedExpense.ActualFee;
                    result = await _expenseRepository.AddExpenseAsync(mappedExpense);
                    if (!result.Success) return result;
                }

                if (!result.Success)
                {
                    result.Message += " (liquidation)";
                    return result;
                }

                result.Id = liquidation.Id;

                await transaction.CommitAsync();

                /* Create the file if everything went as expected*/
                await System.IO.File.WriteAllBytesAsync(filePath, avanceCaisseLiquidation.ReceiptsFile);
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.Message = exception.Message + " ERROR TYPE: " + exception.GetType();
                return result;
            }

            result.Success = true;
            result.Message = "Liquidation for this AvanceCaisse has been successfully saved!";
            return result;
        }

        public async Task<ServiceResult> DeleteDraftedLiquidation(Liquidation liquidation)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();
            try
            {
                AvanceCaisse? relatedAvanceCaisse = null;
                AvanceVoyage? relatedAvanceVoyage = null;

                if (liquidation.AvanceCaisseId != null)
                {
                    relatedAvanceCaisse = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync((int)liquidation.AvanceCaisseId);
                }
                else if (liquidation.AvanceVoyageId != null)
                {
                    relatedAvanceVoyage = await _avanceVoyageRepository.GetAvanceVoyageByIdAsync((int)liquidation.AvanceVoyageId);
                }

                // Delete expenses and trips added during liquidation + update actualfee to zero
                if(relatedAvanceVoyage != null)
                {
                    var av_Expenses = await _expenseRepository.GetAvanceVoyageExpensesByAvIdAsync(relatedAvanceVoyage.Id);
                    foreach(var expense in av_Expenses)
                    {
                        if (expense.EstimatedFee == 0) 
                        {
                            await _expenseRepository.DeleteExpense(expense);
                        }
                        else
                        {
                            expense.ActualFee = 0;
                            await _expenseRepository.UpdateExpense(expense);
                        }
                    }

                    var av_Trips = await _tripRepository.GetAvanceVoyageTripsByAvIdAsync(relatedAvanceVoyage.Id);
                    foreach (var trip in av_Trips)
                    {
                        if (trip.EstimatedFee == 0)
                        {
                            await _tripRepository.DeleteTrip(trip);
                        }
                        else
                        {
                            trip.ActualFee = 0;
                            await _tripRepository.UpdateTrip(trip);
                        }
                    }

                    relatedAvanceVoyage.ActualTotal = 0;
                    await _avanceVoyageRepository.UpdateAvanceVoyageAsync(relatedAvanceVoyage);
                }

                if (relatedAvanceCaisse != null)
                {
                    var ac_Expenses = await _expenseRepository.GetAvanceCaisseExpensesByAcIdAsync(relatedAvanceCaisse.Id);
                    foreach (var expense in ac_Expenses)
                    {
                        if (expense.EstimatedFee == 0)
                        {
                            await _expenseRepository.DeleteExpense(expense);
                        }
                        else
                        {
                            expense.ActualFee = 0;
                            await _expenseRepository.UpdateExpense(expense);
                        }
                    }

                    relatedAvanceCaisse.ActualTotal = 0;
                    await _avanceCaisseRepository.UpdateAvanceCaisseAsync(relatedAvanceCaisse);
                }

                // Delete Status History
                var statusHistories = await _statusHistoryRepository.GetLiquidationStatusHistory(liquidation.Id);
                result = await _statusHistoryRepository.DeleteStatusHistories(statusHistories);
                if (!result.Success) return result;


                // Delete the file from the system
                /* _webHostEnvironment.WebRootPath == wwwroot\ (the default folder to store files) */
                string uploadsFolderPath;
                if (relatedAvanceVoyage != null)
                {
                    uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Liquidation-Avance-Voyage-Receipts");
                }
                else
                {
                    uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Liquidation-Avance-Caisse-Receipts");
                }

                /* Find the file */
                var filePath = Path.Combine(uploadsFolderPath, liquidation.ReceiptsFileName);

                // delete Liquidation from DB
                result = await _liquidationRepository.DeleteLiquidationAync(liquidation);
                if (!result.Success) return result;

                await transaction.CommitAsync();

                /* Delete file */
                System.IO.File.Delete(filePath);
            }
            catch (Exception exception)
            {
                transaction.Rollback();
                result.Success = false;
                result.Message = exception.Message + " ERROR TYPE: " + exception.GetType();
                return result;
            }
            result.Success = true;
            result.Message = "\"Liquidation\" has been deleted successfully";
            return result;
        }

        public async Task<ServiceResult> ModifyLiquidation(LiquidationPutDTO liquidation_REQ, Liquidation liquidation_DB)
        {
            var transaction = _context.Database.BeginTransaction();
            ServiceResult result = new();
            try
            {
                decimal newTotal = 0.0m;

                //Delete all new expenses and trips from DB then add the new ones + Update the actual Fee and update date for old one + recalculate the actual Total
                if (liquidation_REQ.RequestType == "AV")
                {
                    var expenses_DB = await _expenseRepository.GetAvanceVoyageExpensesByAvIdAsync(liquidation_REQ.RequestId);
                    foreach(var expense in expenses_DB)
                    {
                        if(expense.EstimatedFee == 0.0m)
                        {
                            await _expenseRepository.DeleteExpense(expense);
                        }
                        else
                        {
                            expense.ActualFee = liquidation_REQ.ExpensesLiquidations.Find(ex => ex.ExpenseId == expense.Id)?.ActualFee ?? expense.ActualFee;
                            expense.UpdateDate = DateTime.Now;
                            newTotal += expense.ActualFee;
                            result = await _expenseRepository.UpdateExpense(expense);
                            if(!result.Success) return result;
                        }
                    }

                    var trips_DB = await _tripRepository.GetAvanceVoyageTripsByAvIdAsync(liquidation_REQ.RequestId);
                    foreach (var trip in trips_DB)
                    {
                        if (trip.EstimatedFee == 0.0m)
                        {
                            await _tripRepository.DeleteTrip(trip);
                        }
                        else
                        {
                            trip.ActualFee = liquidation_REQ.TripsLiquidations.Find(tp => tp.TripId == trip.Id)?.ActualFee ?? trip.ActualFee;
                            trip.UpdateDate = DateTime.Now;
                            newTotal += trip.ActualFee;
                            result = await _tripRepository.UpdateTrip(trip);
                            if (!result.Success) return result;
                            newTotal += trip.ActualFee;
                        }
                    }

                    if(liquidation_REQ.NewExpenses.Count > 0)
                    {
                        var mappedExpenses = _mapper.Map<List<Expense>>(liquidation_REQ.NewExpenses);
                        foreach (Expense expense in mappedExpenses)
                        {
                            expense.LiquidationId = liquidation_REQ.Id;
                            expense.Currency = liquidation_DB.Currency;
                            newTotal += expense.ActualFee;
                        }
                        result = await _expenseRepository.AddExpensesAsync(mappedExpenses);
                        if (!result.Success) return result;
                    }

                    if (liquidation_REQ.NewTrips.Count > 0)
                    {
                        var mappedTrips = _mapper.Map<List<Trip>>(liquidation_REQ.NewTrips);
                        foreach (Trip trip in mappedTrips)
                        {
                            trip.LiquidationId = liquidation_REQ.Id;
                            newTotal += trip.ActualFee;
                        }
                        result = await _tripRepository.AddTripsAsync(mappedTrips);
                        if (!result.Success) return result;
                    }
                }
                else
                {
                    var expenses_DB = await _expenseRepository.GetAvanceCaisseExpensesByAcIdAsync(liquidation_REQ.RequestId);
                    foreach (var expense in expenses_DB)
                    {
                        if (expense.EstimatedFee == 0.0m)
                        {
                            await _expenseRepository.DeleteExpense(expense);
                        }
                        else
                        {
                            expense.ActualFee = liquidation_REQ.ExpensesLiquidations.Find(ex => ex.ExpenseId == expense.Id)?.ActualFee ?? expense.ActualFee;
                            expense.UpdateDate = DateTime.Now;
                            newTotal += expense.ActualFee;
                            result = await _expenseRepository.UpdateExpense(expense);
                            if(!result.Success) return result;
                        }                         
                    }

                    if(liquidation_REQ.NewExpenses.Count > 0)
                    {
                        var mappedExpenses = _mapper.Map<List<Expense>>(liquidation_REQ.NewExpenses);
                        foreach (Expense expense in mappedExpenses)
                        {
                            expense.LiquidationId = liquidation_REQ.Id;
                            expense.Currency = liquidation_DB.Currency;
                            newTotal += expense.ActualFee;
                        }
                        result = await _expenseRepository.AddExpensesAsync(mappedExpenses);
                        if (!result.Success) return result;
                    }
                }

                //Map the fetched DP from the DB with the new values and update it
                liquidation_DB.DeciderComment = null;
                liquidation_DB.DeciderUserId = null;
                decimal estimatedTotal = Math.Abs(liquidation_DB.ActualTotal - liquidation_DB.Result);
                liquidation_DB.ActualTotal = newTotal;
                liquidation_DB.Result = newTotal - estimatedTotal;
                if (liquidation_DB.LatestStatus != 15)
                {
                    liquidation_DB.LatestStatus = liquidation_REQ.Action.ToLower() == "save" ? 99 : 1;
                }


                //CHECK IF THE MODIFICATION REQUEST HAS A NEW FILE UPLOADED => DELETE OLD ONE AND CREATE NEW ONE
                String newFilePath = "";
                String oldFilePath = ""; 
                String uniqueReceiptsFileName = liquidation_DB.ReceiptsFileName;

                if (liquidation_REQ.ReceiptsFile != null)
                {
                    String uploadsFolderPath;
                    /* _webHostEnvironment.WebRootPath == wwwroot\ (the default folder to store files) */
                    if (liquidation_REQ.RequestType == "AV")
                    {
                        uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Liquidation-Avance-Voyage-Receipts");
                    }
                    else
                    {
                        uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Liquidation-Avance-Caisse-Receipts");
                    }

                    if (!Directory.Exists(uploadsFolderPath))
                    {
                        Directory.CreateDirectory(uploadsFolderPath);
                    }

                    /* concatenate a random string + LQ + reqtype + username + .pdf extension */
                    var user = await _userRepository.GetUserByUserIdAsync(liquidation_DB.UserId);

                    if (liquidation_REQ.RequestType == "AV")
                    {
                        uniqueReceiptsFileName = _miscService.GenerateRandomString(10) + "_LQ_AV_" + user.Username + ".pdf";
                    }
                    else
                    {
                        uniqueReceiptsFileName = _miscService.GenerateRandomString(10) + "_LQ_AC_" + user.Username + ".pdf";
                    }
                    /* combine the folder path with the file name */
                    newFilePath = Path.Combine(uploadsFolderPath, uniqueReceiptsFileName);
                    /* Find the old file */
                    oldFilePath = Path.Combine(uploadsFolderPath, liquidation_DB.ReceiptsFileName);

                    /* Deletion and creation of the file occur after the commitment of DB changes (you don't want to create a file if the req hasnt been saved) */
                }

                liquidation_DB.ReceiptsFileName = uniqueReceiptsFileName;


                //Insert new status history in case of a submit action
                if (liquidation_REQ.Action.ToLower() == "submit")
                {
                    if (liquidation_DB.LatestStatus != 15)
                    {
                        var managerUserId = await _deciderRepository.GetManagerUserIdByUserIdAsync(liquidation_DB.UserId);
                        liquidation_DB.NextDeciderUserId = managerUserId;
                        result = await _liquidationRepository.UpdateLiquidationAsync(liquidation_DB);
                        if (!result.Success) return result;
                        StatusHistory OM_statusHistory = new()
                        {
                            NextDeciderUserId = managerUserId,
                            Total = liquidation_DB.ActualTotal,
                            LiquidationId = liquidation_DB.Id,
                            Status = 1
                        };
                        result = await _statusHistoryRepository.AddStatusAsync(OM_statusHistory);
                        if (!result.Success) return result;
                    }
                    else
                    {
                        liquidation_DB.LatestStatus = 13;
                        liquidation_DB.NextDeciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("TR");
                        result = await _liquidationRepository.UpdateLiquidationAsync(liquidation_DB);
                        if (!result.Success) return result;
                        StatusHistory OM_statusHistory = new()
                        {
                            NextDeciderUserId = liquidation_DB.NextDeciderUserId,
                            Total = liquidation_DB.ActualTotal,
                            LiquidationId = liquidation_DB.Id,
                            Status = 13
                        };
                        result = await _statusHistoryRepository.AddStatusAsync(OM_statusHistory);
                        if (!result.Success) return result;
                    }
                }
                else
                {
                    result = await _liquidationRepository.UpdateLiquidationAsync(liquidation_DB);
                    if (!result.Success) return result;
                    // Just Update Status History Total in case of saving
                    result = await _statusHistoryRepository.UpdateStatusHistoryTotal(liquidation_DB.Id, "LQ", liquidation_DB.ActualTotal);
                    if (!result.Success) return result;
                }

                await transaction.CommitAsync();

                if (liquidation_REQ.ReceiptsFile != null)
                {
                    /* Delete old file */
                    System.IO.File.Delete(oldFilePath);
                    /* Create the new file */
                    await System.IO.File.WriteAllBytesAsync(newFilePath, liquidation_REQ.ReceiptsFile);
                }
                result.Id = liquidation_DB.Id;
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.Message = exception.Message + " ERROR TYPE: " + exception.GetType();
                return result;
            }

            if (liquidation_REQ.Action.ToLower() == "save")
            {
                result.Success = true;
                result.Message = "Liquidation is resubmitted successfully";
                return result;
            }

            result.Success = true;
            result.Message = "Changes made to liquidation are saved successfully";
            return result;
        }

        public async Task<ServiceResult> SubmitLiquidation(int liquidationId)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();

            try
            {
                Liquidation liquidation_DB = await _liquidationRepository.GetLiquidationByIdAsync(liquidationId);
                var nextDeciderUserId = await _deciderRepository.GetManagerUserIdByUserIdAsync(liquidation_DB.UserId);
                liquidation_DB.NextDeciderUserId = nextDeciderUserId;
                liquidation_DB.LatestStatus = 1;
                result = await _liquidationRepository.UpdateLiquidationAsync(liquidation_DB);
                StatusHistory newOM_Status = new()
                {
                    Total = liquidation_DB.ActualTotal,
                    LiquidationId = liquidationId,
                    Status = 1,
                    NextDeciderUserId = nextDeciderUserId,
                };
                result = await _statusHistoryRepository.AddStatusAsync(newOM_Status);
                if (!result.Success)
                {
                    result.Message += " for the newly submitted \"Liquidation\"";
                    return result;
                }

                await transaction.CommitAsync();
                // send email to the next decider
                await _miscService.SendMailToDecider("LQ", liquidation_DB.NextDeciderUserId, liquidation_DB.Id);
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.Message = exception.Message + " ERROR TYPE: " + exception.GetType();
                return result;
            }

            result.Success = true;
            result.Message = "Liquidation is Submitted successfully";
            return result;

        }

        public async Task<ServiceResult> DecideOnLiquidation(DecisionOnRequestPostDTO decision, int deciderUserId)
        {
            ServiceResult result = new();
            Liquidation decidedLiquidation = await _liquidationRepository.GetLiquidationByIdAsync(decision.RequestId);

            // test if the decider is the one who is supposed to decide upon it
            if (decidedLiquidation.NextDeciderUserId != deciderUserId)
            {
                result.Success = false;
                result.Message = "You can't decide on this request in this state! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks";
                return result;
            }

            // CASE: RETURN
            if (decision.DecisionString == "return")
            {
                var transaction = _context.Database.BeginTransaction();
                try
                {
                    decidedLiquidation.DeciderComment = decision.DeciderComment;
                    decidedLiquidation.DeciderUserId = deciderUserId;
                    if (decision.ReturnedToFMByTR)
                    {
                        decidedLiquidation.NextDeciderUserId = await _liquidationRepository.GetLiquidationNextDeciderUserId("TR", decision.ReturnedToFMByTR, null);
                        decidedLiquidation.LatestStatus = 14; /* Returned to finance dept for missing info */
                    }
                    else if (decision.ReturnedToTRByFM)
                    {
                        decidedLiquidation.NextDeciderUserId = await _liquidationRepository.GetLiquidationNextDeciderUserId("FM", null, decision.ReturnedToTRByFM);
                        decidedLiquidation.LatestStatus = 13; /* pending TR validation */
                    }
                    else if (decision.ReturnedToRequesterByTR)
                    {
                        decidedLiquidation.NextDeciderUserId = null;
                        decidedLiquidation.LatestStatus = 15; /* returned for missing evidences + next decider is still TR */
                    }
                    else
                    {
                        decidedLiquidation.NextDeciderUserId = null; /* next decider is set to null if returned normally */
                        decidedLiquidation.LatestStatus = 98; /* returned normaly */
                    }
                    result = await _liquidationRepository.UpdateLiquidationAsync(decidedLiquidation);
                    if (!result.Success) return result;

                    StatusHistory decidedAvanceCaisse_SH = new()
                    {
                        LiquidationId = decision.RequestId,
                        DeciderUserId = deciderUserId,
                        DeciderComment = decision.DeciderComment,
                        Total = decidedLiquidation.ActualTotal,
                        Status = 98,
                        NextDeciderUserId = decidedLiquidation.NextDeciderUserId,
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(decidedAvanceCaisse_SH);
                    if (!result.Success) return result;

                    await transaction.CommitAsync();
                }
                catch (Exception exception)
                {
                    result.Success = false;
                    result.Message = exception.Message + " ERROR TYPE: " + exception.GetType();
                    return result;
                }

            }
            // CASE: APPROVE
            else
            {
                var transaction = _context.Database.BeginTransaction();
                try
                {
                    decidedLiquidation.DeciderUserId = deciderUserId;
                    switch (decidedLiquidation.LatestStatus)
                    {
                        case 1:
                            decidedLiquidation.NextDeciderUserId = await _liquidationRepository.GetLiquidationNextDeciderUserId("MG");
                            decidedLiquidation.LatestStatus = 3;
                            break;
                        case 3:
                            decidedLiquidation.NextDeciderUserId = await _liquidationRepository.GetLiquidationNextDeciderUserId("FM", null, decision.ReturnedToTRByFM);
                            decidedLiquidation.LatestStatus = 4;
                            break;
                        case 4:
                            decidedLiquidation.NextDeciderUserId = await _liquidationRepository.GetLiquidationNextDeciderUserId("GD");
                            decidedLiquidation.LatestStatus = 13;
                            break;
                        case 13:
                            decidedLiquidation.NextDeciderUserId = null;
                            decidedLiquidation.LatestStatus = 16;
                            break;
                    }

                    result = await _liquidationRepository.UpdateLiquidationAsync(decidedLiquidation);
                    if (!result.Success) return result;

                    StatusHistory decidedAvanceCaisse_SH = new()
                    {
                        LiquidationId = decision.RequestId,
                        DeciderUserId = deciderUserId,
                        DeciderComment = decision.DeciderComment,
                        Total = decidedLiquidation.ActualTotal,
                        Status = decidedLiquidation.LatestStatus,
                        NextDeciderUserId = decidedLiquidation.NextDeciderUserId,
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(decidedAvanceCaisse_SH);
                    if (!result.Success) return result;

                    await transaction.CommitAsync();
                    // send email to the next decider
                    await _miscService.SendMailToDecider("LQ", decidedLiquidation.NextDeciderUserId, decidedLiquidation.Id);
                }
                catch (Exception exception)
                {
                    result.Success = false;
                    result.Message = exception.Message + " ERROR TYPE: " + exception.GetType();
                    return result;
                }
            }

            result.Success = true;
            result.Message = "Liquidation is decided upon successfully";
            return result;
        }

        public async Task<string> GenerateAvanceCaisseLiquidationWordDocument(int liquidationId)
        {
            LiquidationAvanceCaisseDocumentDetailsDTO liquidationDetails = await _liquidationRepository.GetAvanceCaisseLiquidationDocumentDetailsByIdAsync(liquidationId);


            var signaturesDir = Path.Combine(_webHostEnvironment.WebRootPath, "Static-Files\\Signatures");
            var docPath = Path.Combine(_webHostEnvironment.WebRootPath, "Static-Files", "LIQUIDATION_AC_DOCUMENT.docx");

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
                            return ReplaceLiquidationAvanceCaisseDocumentPlaceHolders(match, liquidationDetails);
                        },
                        RegExOptions = RegexOptions.IgnoreCase,
                        NewFormatting = new Formatting() { FontFamily = new Xceed.Document.NET.Font("Arial"), Bold = false }
                    };
                    docx.ReplaceText(replaceTextOptions);
#pragma warning disable CS0618 // func is obsolete

                    // Replace the expenses table
                    docx.ReplaceTextWithObject("expenses", _miscService.GenerateExpesnesTableForLiquidationDocuments(docx, liquidationDetails.Expenses));

                    // Replace result
                    if (liquidationDetails.Result >= 0)
                    {
                        docx.ReplaceText("requester_credit", Math.Abs(liquidationDetails.Result).ToString());
                        docx.ReplaceText("company_credit", "0.00");
                    }
                    else
                    {
                        docx.ReplaceText("company_credit", Math.Abs(liquidationDetails.Result).ToString());
                        docx.ReplaceText("requester_credit", "0.00");
                    }


                    // Replace the signatures
                    if (liquidationDetails.Signers.Any(s => s.Level == "MG"))
                    {
                        docx.ReplaceText("%mg_signature%", liquidationDetails.Signers.Where(s => s.Level == "MG")
                                .Select(s => $"{s.FirstName} {s.LastName}")
                                .First());
                        docx.ReplaceText("%mg_signature_date%", liquidationDetails.Signers.Where(s => s.Level == "MG")
                                .Select(s => s.SignDate.ToString("dd/MM/yyyy hh:mm"))
                                .First());

                        string? imgName = liquidationDetails.Signers.Where(s => s.Level == "MG")
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
                    if (liquidationDetails.Signers.Any(s => s.Level == "FM"))
                    {
                        docx.ReplaceText("%fm_signature%", liquidationDetails.Signers.Where(s => s.Level == "FM")
                                .Select(s => $"{s.FirstName} {s.LastName}")
                                .First());
                        docx.ReplaceText("%fm_signature_date%", liquidationDetails.Signers.Where(s => s.Level == "FM")
                                .Select(s => s.SignDate.ToString("dd/MM/yyyy hh:mm"))
                                .First());

                        string? imgName = liquidationDetails.Signers.Where(s => s.Level == "FM")
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
                    if (liquidationDetails.Signers.Any(s => s.Level == "GD"))
                    {
                        docx.ReplaceText("%gd_signature%", liquidationDetails.Signers.Where(s => s.Level == "GD")
                                .Select(s => $"{s.FirstName} {s.LastName}")
                                .First());
                        docx.ReplaceText("%gd_signature_date%", liquidationDetails.Signers.Where(s => s.Level == "GD")
                                .Select(s => s.SignDate.ToString("dd/MM/yyyy hh:mm"))
                                .First());

                        string? imgName = liquidationDetails.Signers.Where(s => s.Level == "GD")
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

        public async Task<string> GenerateAvanceVoyageLiquidationWordDocument(int liquidationId)
        {
            LiquidationAvanceVoyageDocumentDetailsDTO liquidationDetails = await _liquidationRepository.GetAvanceVoyageLiquidationDocumentDetailsByIdAsync(liquidationId);


            var signaturesDir = Path.Combine(_webHostEnvironment.WebRootPath, "Static-Files\\Signatures");
            var docPath = Path.Combine(_webHostEnvironment.WebRootPath, "Static-Files", "LIQUIDATION_AV_DOCUMENT.docx");

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
                            return ReplaceLiquidationAvanceVoyageDocumentPlaceHolders(match, liquidationDetails);
                        },
                        RegExOptions = RegexOptions.IgnoreCase,
                        NewFormatting = new Formatting() { FontFamily = new Xceed.Document.NET.Font("Arial"), Bold = false }
                    };
                    docx.ReplaceText(replaceTextOptions);
#pragma warning disable CS0618 // func is obsolete

                    // Replace the expenses table
                    if (liquidationDetails.Expenses.Count > 0)
                    {
                        docx.ReplaceTextWithObject("expenses", _miscService.GenerateExpesnesTableForLiquidationDocuments(docx, liquidationDetails.Expenses));
                    }
                    else
                    {
                        docx.ReplaceText("expenses", "(Pas de dépense)");
                    }

                    // Replace the trips table
                    docx.ReplaceTextWithObject("trips", _miscService.GenerateTripsTableForLiquidationDocuments(docx, liquidationDetails.Trips));

                    // Replace result
                    if(liquidationDetails.Result >= 0)
                    {
                        docx.ReplaceText("requester_credit", Math.Abs(liquidationDetails.Result).ToString());
                        docx.ReplaceText("company_credit", "0.00");
                    }
                    else
                    {
                        docx.ReplaceText("company_credit", Math.Abs(liquidationDetails.Result).ToString());
                        docx.ReplaceText("requester_credit", "0.00");
                    }

                    // Replace the signatures
                    if (liquidationDetails.Signers.Any(s => s.Level == "MG"))
                    {
                        docx.ReplaceText("%mg_signature%", liquidationDetails.Signers.Where(s => s.Level == "MG")
                                .Select(s => $"{s.FirstName} {s.LastName}")
                                .First());
                        docx.ReplaceText("%mg_signature_date%", liquidationDetails.Signers.Where(s => s.Level == "MG")
                                .Select(s => s.SignDate.ToString("dd/MM/yyyy hh:mm"))
                                .First());

                        string? imgName = liquidationDetails.Signers.Where(s => s.Level == "MG")
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
                    if (liquidationDetails.Signers.Any(s => s.Level == "FM"))
                    {
                        docx.ReplaceText("%fm_signature%", liquidationDetails.Signers.Where(s => s.Level == "FM")
                                .Select(s => $"{s.FirstName} {s.LastName}")
                                .First());
                        docx.ReplaceText("%fm_signature_date%", liquidationDetails.Signers.Where(s => s.Level == "FM")
                                .Select(s => s.SignDate.ToString("dd/MM/yyyy hh:mm"))
                                .First());

                        string? imgName = liquidationDetails.Signers.Where(s => s.Level == "FM")
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
                    if (liquidationDetails.Signers.Any(s => s.Level == "GD"))
                    {
                        docx.ReplaceText("%gd_signature%", liquidationDetails.Signers.Where(s => s.Level == "GD")
                                .Select(s => $"{s.FirstName} {s.LastName}")
                                .First());
                        docx.ReplaceText("%gd_signature_date%", liquidationDetails.Signers.Where(s => s.Level == "GD")
                                .Select(s => s.SignDate.ToString("dd/MM/yyyy hh:mm"))
                                .First());

                        string? imgName = liquidationDetails.Signers.Where(s => s.Level == "GD")
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


        public string ReplaceLiquidationAvanceCaisseDocumentPlaceHolders(string placeHolder, LiquidationAvanceCaisseDocumentDetailsDTO liquidationDetails)
        {
            Dictionary<string, string> _replacePatterns = new Dictionary<string, string>()
              {
                { "id", liquidationDetails.Id.ToString() },
                { "full_name", liquidationDetails.FirstName + " " + liquidationDetails.LastName },
                { "date", liquidationDetails.SubmitDate.ToString("dd/MM/yyyy") },
                { "actual_amount", liquidationDetails.ActualTotal.ToString().FormatWith(new CultureInfo("fr-FR")) },
                { "estimated_amount", liquidationDetails.EstimatedTotal.ToString().FormatWith(new CultureInfo("fr-FR")) },
                { "worded_amount", ((int)liquidationDetails.ActualTotal).ToWords(new CultureInfo("fr-FR")) },
                { "currency", liquidationDetails.Currency.ToString() },
                { "description", liquidationDetails.Description.ToString() },
              };
            if (_replacePatterns.ContainsKey(placeHolder))
            {
                return _replacePatterns[placeHolder];
            }
            return placeHolder;
        }

        public string ReplaceLiquidationAvanceVoyageDocumentPlaceHolders(string placeHolder, LiquidationAvanceVoyageDocumentDetailsDTO liquidationDetails)
        {
#pragma warning disable CS8604 // null warning.
            Dictionary<string, string> _replacePatterns = new Dictionary<string, string>()
              {
                { "id", liquidationDetails.Id.ToString() },
                { "full_name", liquidationDetails.FirstName + " " + liquidationDetails.LastName },
                { "date", liquidationDetails.SubmitDate.ToString("dd/MM/yyyy") },
                { "actual_amount", liquidationDetails.ActualTotal.ToString().FormatWith(new CultureInfo("fr-FR")) },
                { "estimated_amount", liquidationDetails.EstimatedTotal.ToString().FormatWith(new CultureInfo("fr-FR")) },
                { "worded_amount", ((int)liquidationDetails.ActualTotal).ToWords(new CultureInfo("fr-FR")) },
                { "currency", liquidationDetails.Currency.ToString() },
                { "description", liquidationDetails.Description.ToString() },
              };
#pragma warning restore CS8604 // null warning
            if (_replacePatterns.ContainsKey(placeHolder))
            {
                return _replacePatterns[placeHolder];
            }
            return placeHolder;
        }



    }
}
