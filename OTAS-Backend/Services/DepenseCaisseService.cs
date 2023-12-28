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
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly OtasContext _context;

        public DepenseCaisseService(IDepenseCaisseRepository depenseCaisseRepository,
            IActualRequesterRepository actualRequesterRepository,
            IStatusHistoryRepository statusHistoryRepository,
            IExpenseRepository expenseRepository,
            IWebHostEnvironment webHostEnvironment,
            IDeciderRepository deciderRepository,
            IUserRepository userRepository,
            IMapper mapper,
            OtasContext context)
        {
            _depenseCaisseRepository = depenseCaisseRepository;
            _actualRequesterRepository = actualRequesterRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _expenseRepository = expenseRepository;
            _webHostEnvironment = webHostEnvironment;
            _deciderRepository = deciderRepository;
            _userRepository = userRepository;
            _mapper = mapper;
            _context = context;
        }


        public async Task<ServiceResult> AddDepenseCaisse(DepenseCaissePostDTO depenseCaisse, int userId)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();
            try
            {
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
                var user = await _userRepository.GetUserByUserIdAsync(userId);
                string uniqueReceiptsFileName = Guid.NewGuid() + "_DC_" + user.Username + ".pdf";
                /* Combine the folder path with the file name to create full path */
                var filePath = Path.Combine(uploadsFolderPath, uniqueReceiptsFileName);
                /* The creation of the file occurs after the commitment of DB changes */
               

                mappedDepenseCaisse.Total = CalculateExpensesEstimatedTotal(mappedExpenses);
                mappedDepenseCaisse.ReceiptsFileName = uniqueReceiptsFileName;
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

                /* Create the file if everything went as expected*/
                await System.IO.File.WriteAllBytesAsync(filePath, depenseCaisse.ReceiptsFile);
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


                // Handle Onbehalf case
                if (depenseCaisse.OnBehalf == true)
                {
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

                //CHECK IF THE MODIFICATION REQUEST HAS A NEW FILE UPLOADED => DELETE OLD ONE AND CREATE NEW ONE
                String newFilePath = "";
                String oldFilePath = "";
                if (depenseCaisse.ReceiptsFile != null)
                {
                    /* _webHostEnvironment.WebRootPath == wwwroot\ (the default folder to store files) */
                    var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Depense-Caisse-Receipts");
                    if (!Directory.Exists(uploadsFolderPath))
                    {
                        Directory.CreateDirectory(uploadsFolderPath);
                    }
                    /* concatenate a uuid + DC + username + .pdf extension */
                    var user = await _userRepository.GetUserByUserIdAsync(depenseCaisse_DB.UserId);
                    string uniqueReceiptsFileName = Guid.NewGuid() + "_DC_" + user.Username + ".pdf";
                    /* combine the folder path with the file name */
                    newFilePath = Path.Combine(uploadsFolderPath, uniqueReceiptsFileName);
                    /* Find the old file */
                    oldFilePath = Path.Combine(uploadsFolderPath, depenseCaisse_DB.ReceiptsFileName);

                    /* Deletion and creation of the file occur after the commitment of DB changes */

                    depenseCaisse_DB.ReceiptsFileName = uniqueReceiptsFileName;
                }

                depenseCaisse_DB.Total = CalculateExpensesEstimatedTotal(mappedExpenses);


                //Insert new status history in case of a submit action
                if (depenseCaisse.Action.ToLower() == "submit")
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

                if(depenseCaisse.ReceiptsFile != null)
                {
                    /* Delete old file */
                    System.IO.File.Delete(oldFilePath);
                    /* Create the new file */
                    await System.IO.File.WriteAllBytesAsync(newFilePath, depenseCaisse.ReceiptsFile);
                }
           
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
                result.Message = "DepenseCaisse is resubmitted successfully";
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
                DepenseCaisse depenseCaisse_DB = await _depenseCaisseRepository.GetDepenseCaisseByIdAsync(depenseCaisseId);
                var nextDeciderUserId = await _deciderRepository.GetManagerUserIdByUserIdAsync(depenseCaisse_DB.UserId);
                depenseCaisse_DB.NextDeciderUserId = nextDeciderUserId;
                depenseCaisse_DB.LatestStatus = 1;
                result = await _depenseCaisseRepository.UpdateDepenseCaisseAsync(depenseCaisse_DB);
                StatusHistory newOM_Status = new()
                {
                    Total = depenseCaisse_DB.Total,
                    DepenseCaisseId = depenseCaisseId,
                    Status = 1,
                    NextDeciderUserId = nextDeciderUserId,
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

        public async Task<ServiceResult> DeleteDraftedDepenseCaisse(DepenseCaisse depenseCaisse)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();
            try
            {

                // Delete expenses & status History
                var expenses = await _expenseRepository.GetDepenseCaisseExpensesByAvIdAsync(depenseCaisse.Id);
                var statusHistories = await _statusHistoryRepository.GetDepenseCaisseStatusHistory(depenseCaisse.Id);

                result = await _expenseRepository.DeleteExpenses(expenses);
                if (!result.Success) return result;

                result = await _statusHistoryRepository.DeleteStatusHistories(statusHistories);
                if (!result.Success) return result;


                if (depenseCaisse.OnBehalf == true)
                {
                    ActualRequester actualRequester = await _actualRequesterRepository.FindActualrequesterInfoByDepenseCaisseIdAsync(depenseCaisse.Id);
                    result = await _actualRequesterRepository.DeleteActualRequesterInfoAsync(actualRequester);
                    if (!result.Success) return result;
                }

                // Delete the file from the system
                /* _webHostEnvironment.WebRootPath == wwwroot\ (the default folder to store files) */
                var uploadsFolderPath = Path.Combine(_webHostEnvironment.WebRootPath, "Depense-Caisse-Receipts");
                /* Find the file */
                var filePath = Path.Combine(uploadsFolderPath, depenseCaisse.ReceiptsFileName);
                
                // delete DC
                result = await _depenseCaisseRepository.DeleteDepenseCaisseAync(depenseCaisse);
                if (!result.Success) return result;

                await transaction.CommitAsync();

                /* Delete old file */
                System.IO.File.Delete(filePath);
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                result.Success = false;
                result.Message = ex.Message;
                return result;
            }
            result.Success = true;
            result.Message = "\"DepenseCaisse\" has been deleted successfully";
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
