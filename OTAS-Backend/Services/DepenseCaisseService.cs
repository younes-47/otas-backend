using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using OTAS.Data;
using OTAS.DTO.Post;
using OTAS.DTO.Put;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using OTAS.Repository;


namespace OTAS.Services
{
    public class DepenseCaisseService : IDepenseCaisseService
    {
        private readonly IDepenseCaisseRepository _depenseCaisseRepository;
        private readonly IActualRequesterRepository _actualRequesterRepository;
        private readonly IStatusHistoryRepository _statusHistoryRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IDeciderRepository _deciderRepository;
        private readonly IMapper _mapper;
        private readonly OtasContext _context;

        public DepenseCaisseService(IDepenseCaisseRepository depenseCaisseRepository,
            IActualRequesterRepository actualRequesterRepository,
            IStatusHistoryRepository statusHistoryRepository,
            IExpenseRepository expenseRepository,
            IWebHostEnvironment webHostEnvironment,
            IDeciderRepository deciderRepository,
            IMapper mapper,
            OtasContext context)
        {
            _depenseCaisseRepository = depenseCaisseRepository;
            _actualRequesterRepository = actualRequesterRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _expenseRepository = expenseRepository;
            _webHostEnvironment = webHostEnvironment;
            _deciderRepository = deciderRepository;
            _mapper = mapper;
            _context = context;
        }


        public async Task<ServiceResult> AddDepenseCaisse(DepenseCaissePostDTO depenseCaisse, int userId)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();
            try
            {
                if (depenseCaisse.Expenses.Count < 1)
                {
                    result.Success = false;
                    result.Message = "DepenseCaisse must contain at least one expense! You can't request a DepenseCaisse with no expense.";
                    return result;
                }

                if (depenseCaisse.Currency != "MAD" && depenseCaisse.Currency != "EUR")
                {
                    result.Success = false;
                    result.Message = "Invalid currency! Please choose suitable currency from the dropdownmenu!";
                    return result;
                }

                //Automatic mapping
                List<Expense> mappedExpenses = _mapper.Map<List<Expense>>(depenseCaisse.Expenses);
                DepenseCaisse mappedDepenseCaisse = _mapper.Map<DepenseCaisse>(depenseCaisse);


                //Manual mapping

                /* _webHostEnvironment.WebRootPath == wwwroot\ (the default folder to store files) */
                var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Depense-Caisse-Receipts");
                if (!Directory.Exists(uploadsFolderPath))
                {
                    Directory.CreateDirectory(uploadsFolderPath);
                }
                /* Create file name by concatenating a uuid + DC + username + .pdf extension */
                string uniqueReceiptsFileName = Guid.NewGuid() + "_DC_" + userId + ".pdf";
                /* Combine the folder path with the file name to create full path */
                var filePath = Path.Combine(uploadsFolderPath, uniqueReceiptsFileName);
                /* Create the file */
                await System.IO.File.WriteAllBytesAsync(filePath, depenseCaisse.ReceiptsFile);


                mappedDepenseCaisse.Total = CalculateExpensesEstimatedTotal(mappedExpenses);
                mappedDepenseCaisse.ReceiptsFilePath = uniqueReceiptsFileName;
                mappedDepenseCaisse.UserId = userId;

                result = await _depenseCaisseRepository.AddDepenseCaisseAsync(mappedDepenseCaisse);
                if (!result.Success) return result;

                // Status History
                StatusHistory DC_status = new()
                {
                    Total = mappedDepenseCaisse.Total,
                    DepenseCaisseId = mappedDepenseCaisse.Id,
                    Status = mappedDepenseCaisse.LatestStatus,
                };
                result = await _statusHistoryRepository.AddStatusAsync(DC_status);

                if (!result.Success)
                {
                    result.Message += " (depenseCaisse)";
                    return result;
                }

                // Handle Actual Requester Info (if exists)
                if (depenseCaisse.OnBehalf == true)
                {
                    if (depenseCaisse.ActualRequester == null)
                    {
                        result.Success = false;
                        result.Message = "You must fill actual requester's info in case you are filing this request on behalf of someone";
                        return result;
                    }

                    ActualRequester mappedActualRequester = _mapper.Map<ActualRequester>(depenseCaisse.ActualRequester);
                    mappedActualRequester.DepenseCaisseId = mappedDepenseCaisse.Id;
                    mappedActualRequester.OrderingUserId = mappedDepenseCaisse.UserId;
                    result = await _actualRequesterRepository.AddActualRequesterInfoAsync(mappedActualRequester);
                    if (!result.Success) return result;
                }

                //Inserting the expenses
                foreach (Expense expense in mappedExpenses)
                {
                    expense.DepenseCaisseId = mappedDepenseCaisse.Id;
                    expense.Currency = mappedDepenseCaisse.Currency;
                    result = await _expenseRepository.AddExpenseAsync(expense);
                    if (!result.Success) return result;
                }

                await transaction.CommitAsync();

            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"ERROR: {ex.Message} ||| {ex.InnerException} ||| {ex.StackTrace}";
                return result;
            }

            result.Success = true;
            result.Message = "DepenseCaisse & Receipts have been sent successfully";
            return result;
        }

        public async Task<ServiceResult> ModifyDepenseCaisse(DepenseCaissePutDTO depenseCaisse)
        {
            var transaction = _context.Database.BeginTransaction();
            ServiceResult result = new();
            try
            {
                DepenseCaisse depenseCaisse_DB = await _depenseCaisseRepository.GetDepenseCaisseByIdAsync(depenseCaisse.Id);

                if (depenseCaisse_DB.LatestStatus == 97)
                {
                    result.Success = false;
                    result.Message = "You can't modify or resubmit a rejected request!";
                    return result;
                }

                if (depenseCaisse_DB.LatestStatus != 98 && depenseCaisse_DB.LatestStatus != 99)
                {
                    result.Success = false;
                    result.Message = "The DP you are trying to modify is not a draft nor returned! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks";
                    return result;
                }

                if (depenseCaisse.Action.ToLower() == "save" && depenseCaisse_DB.LatestStatus == 98)
                {
                    result.Success = false;
                    result.Message = "You can't modify and save a returned request as a draft again. You may want to apply your modifications to you request and resubmit it directly";
                    return result;
                }

                if (depenseCaisse.Expenses.Count < 1)
                {
                    result.Success = false;
                    result.Message = "DP must have at least one expense! You can't request a DP with no expense.";
                    return result;
                }

                // Handle Onbehalf case
                if (depenseCaisse.OnBehalf == true)
                {
                    if (depenseCaisse.ActualRequester == null)
                    {
                        result.Success = false;
                        result.Message = "You must fill actual requester's info in case you are filing this request on behalf of someone";
                        return result;
                    }

                    //Delete actualrequester info first regardless
                    ActualRequester? actualRequester = await _actualRequesterRepository.FindActualrequesterInfoByDepenseCaisseIdAsync(depenseCaisse.Id);
                    if (actualRequester != null)
                    {
                        result = await _actualRequesterRepository.DeleteActualRequesterInfoAsync(actualRequester);
                        if (!result.Success) return result;
                    }

                    //Insert the new one coming from request
                    ActualRequester mappedActualRequester = _mapper.Map<ActualRequester>(depenseCaisse.ActualRequester);
                    mappedActualRequester.DepenseCaisseId = depenseCaisse.Id;
                    mappedActualRequester.OrderingUserId = depenseCaisse_DB.UserId;
                    result = await _actualRequesterRepository.AddActualRequesterInfoAsync(mappedActualRequester);
                    if (!result.Success) return result;
                }
                else
                {
                    ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByDepenseCaisseIdAsync(depenseCaisse.Id);
                    if (actualRequesterInfo != null)
                    {
                        //Make sure to delete actualrequester in case the user modifies "OnBehalf" Prop to false and an actual requester exists in database
                        result = await _actualRequesterRepository.DeleteActualRequesterInfoAsync(actualRequesterInfo);
                        if (!result.Success) return result;
                    }
                }

                //Delete all expenses from DB (related to this dc!)
                var expenses_DB = await _expenseRepository.GetDepenseCaisseExpensesByAvIdAsync(depenseCaisse.Id);
                await _expenseRepository.DeleteExpenses(expenses_DB);

                //Map each expense coming from the request and insert them
                var mappedExpenses = _mapper.Map<List<Expense>>(depenseCaisse.Expenses);
                foreach (Expense expense in mappedExpenses)
                {
                    expense.DepenseCaisseId = depenseCaisse_DB.Id;
                    expense.Currency = depenseCaisse.Currency;
                }
                result = await _expenseRepository.AddExpensesAsync(mappedExpenses);
                if (!result.Success) return result;

                //Map the fetched DP from the DB with the new values and update it
                depenseCaisse_DB.DeciderComment = null;
                depenseCaisse_DB.DeciderUserId = null;
                depenseCaisse_DB.LatestStatus = depenseCaisse.Action.ToLower() == "save" ? 99 : 1;
                depenseCaisse_DB.Description = depenseCaisse.Description;
                depenseCaisse_DB.Currency = depenseCaisse.Currency;
                depenseCaisse_DB.OnBehalf = depenseCaisse.OnBehalf;

                
                /* _webHostEnvironment.WebRootPath == wwwroot\ (the default folder to store files) */
                var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Depense-Caisse-Receipts");
                if (!Directory.Exists(uploadsFolderPath))
                {
                    Directory.CreateDirectory(uploadsFolderPath);
                }
                /* concatenate a uuid + DC + username + .pdf extension */
                string uniqueReceiptsFileName = Guid.NewGuid() + "_DC_" + depenseCaisse_DB.UserId + ".pdf";
                /* combine the folder path with the file name */
                var filePath = Path.Combine(uploadsFolderPath, uniqueReceiptsFileName);
                /* Create the file */
                await System.IO.File.WriteAllBytesAsync(filePath, depenseCaisse.ReceiptsFile);

                depenseCaisse_DB.ReceiptsFilePath = uniqueReceiptsFileName;
                depenseCaisse_DB.Total = CalculateExpensesEstimatedTotal(mappedExpenses);


                //Insert new status history in case of a submit action
                if (depenseCaisse.Action.ToLower() == "save")
                {
                    var managerUserId = await _deciderRepository.GetManagerUserIdByUserIdAsync(depenseCaisse_DB.UserId);
                    depenseCaisse_DB.NextDeciderUserId = managerUserId;
                    result = await _depenseCaisseRepository.UpdateDepenseCaisseAsync(depenseCaisse_DB);
                    if (!result.Success) return result;
                    StatusHistory OM_statusHistory = new()
                    {
                        Total = depenseCaisse_DB.Total,
                        DepenseCaisseId = depenseCaisse_DB.Id,
                        Status = 1
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(OM_statusHistory);
                    if (!result.Success) return result;
                }
                else
                {
                    result = await _depenseCaisseRepository.UpdateDepenseCaisseAsync(depenseCaisse_DB);
                    if (!result.Success) return result;
                    // Just Update Status History Total in case of saving
                    result = await _statusHistoryRepository.UpdateStatusHistoryTotal(depenseCaisse_DB.Id, "DC", depenseCaisse_DB.Total);
                    if (!result.Success) return result;
                }

                await transaction.CommitAsync();
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.Message = exception.Message + exception.GetType() + exception.StackTrace;
                return result;
            }

            if (depenseCaisse.Action.ToLower() == "save")
            {
                result.Success = true;
                result.Message = "depenseCaisse is resubmitted successfully";
                return result;
            }

            result.Success = true;
            result.Message = "Changes made to depenseCaisse are saved successfully";
            return result;
        }

        public async Task<ServiceResult> SubmitDepenseCaisse(int depenseCaisseId)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();

            try
            {
                var testAC = await _depenseCaisseRepository.GetDepenseCaisseByIdAsync(depenseCaisseId);
                if (testAC.LatestStatus == 1)
                {
                    result.Success = false;
                    result.Message = "You've already submitted this DepenseCaisse!";
                    return result;
                }

                result = await _depenseCaisseRepository.UpdateDepenseCaisseStatusAsync(depenseCaisseId, 1);
                if (!result.Success) return result;

                DepenseCaisse dc = await _depenseCaisseRepository.GetDepenseCaisseByIdAsync(depenseCaisseId);

                StatusHistory newOM_Status = new()
                {
                    Total = dc.Total,
                    DepenseCaisseId = depenseCaisseId,
                    Status = 1,
                };
                result = await _statusHistoryRepository.AddStatusAsync(newOM_Status);
                if (!result.Success)
                {
                    result.Message += " for the newly submitted \"DepenseCaisse\"";
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
            result.Message = "DepenseCaisse is Submitted successfully";
            return result;

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
