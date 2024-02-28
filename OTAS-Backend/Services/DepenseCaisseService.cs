using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using OTAS.Data;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.DTO.Put;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using Humanizer;
using System.Globalization;
using System.Text.RegularExpressions;
using Xceed.Document.NET;
using Xceed.Words.NET;


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
        private readonly IMiscService _miscService;
        private readonly IMapper _mapper;
        private readonly OtasContext _context;

        public DepenseCaisseService(IDepenseCaisseRepository depenseCaisseRepository,
            IActualRequesterRepository actualRequesterRepository,
            IStatusHistoryRepository statusHistoryRepository,
            IExpenseRepository expenseRepository,
            IWebHostEnvironment webHostEnvironment,
            IDeciderRepository deciderRepository,
            IUserRepository userRepository,
            IMiscService miscService,
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
            _miscService = miscService;
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
                /* Create file name by concatenating a random string + DC + username + .pdf extension */
                var user = await _userRepository.GetUserByUserIdAsync(userId);
                string uniqueReceiptsFileName = _miscService.GenerateRandomString(10) + "_DC_" + user.Username + ".pdf";
                /* Combine the folder path with the file name to create full path */
                var filePath = Path.Combine(uploadsFolderPath, uniqueReceiptsFileName);
                /* The creation of the file occurs after the commitment of DB changes */
               

                mappedDepenseCaisse.Total = _miscService.CalculateExpensesEstimatedTotal(mappedExpenses);
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
                    mappedActualRequester.ManagerUserId = await _userRepository.GetUserIdByUsernameAsync(depenseCaisse.ActualRequester.ManagerUserName);
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

                result.Id = mappedDepenseCaisse.Id;
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
                    mappedActualRequester.ManagerUserId = await _userRepository.GetUserIdByUsernameAsync(depenseCaisse.ActualRequester.ManagerUserName);
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
                var expenses_DB = await _expenseRepository.GetDepenseCaisseExpensesByDcIdAsync(depenseCaisse.Id);
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
                if(depenseCaisse_DB.LatestStatus != 15)
                {
                    depenseCaisse_DB.LatestStatus = depenseCaisse.Action.ToLower() == "save" ? 99 : 1;
                }
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
                    /* concatenate a random string + DC + username + .pdf extension */
                    var user = await _userRepository.GetUserByUserIdAsync(depenseCaisse_DB.UserId);
                    string uniqueReceiptsFileName = _miscService.GenerateRandomString(10) + "_DC_" + user.Username + ".pdf";
                    /* combine the folder path with the file name */
                    newFilePath = Path.Combine(uploadsFolderPath, uniqueReceiptsFileName);
                    /* Find the old file */
                    oldFilePath = Path.Combine(uploadsFolderPath, depenseCaisse_DB.ReceiptsFileName);

                    /* Deletion and creation of the file occur after the commitment of DB changes (you don't want to create a file if the req hasnt been saved) */

                    depenseCaisse_DB.ReceiptsFileName = uniqueReceiptsFileName;
                }

                depenseCaisse_DB.Total = _miscService.CalculateExpensesEstimatedTotal(mappedExpenses);


                //Insert new status history in case of a submit action
                if (depenseCaisse.Action.ToLower() == "submit")
                {
                    if(depenseCaisse_DB.LatestStatus != 15)
                    {
                        var managerUserId = await _deciderRepository.GetManagerUserIdByUserIdAsync(depenseCaisse_DB.UserId);
                        depenseCaisse_DB.NextDeciderUserId = managerUserId;
                        result = await _depenseCaisseRepository.UpdateDepenseCaisseAsync(depenseCaisse_DB);
                        if (!result.Success) return result;
                        StatusHistory OM_statusHistory = new()
                        {
                            NextDeciderUserId = managerUserId,
                            Total = depenseCaisse_DB.Total,
                            DepenseCaisseId = depenseCaisse_DB.Id,
                            Status = 1
                        };
                        result = await _statusHistoryRepository.AddStatusAsync(OM_statusHistory);
                        if (!result.Success) return result;
                    }
                    else
                    {
                        depenseCaisse_DB.LatestStatus = 13;
                        depenseCaisse_DB.NextDeciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("TR");
                        result = await _depenseCaisseRepository.UpdateDepenseCaisseAsync(depenseCaisse_DB);
                        if (!result.Success) return result;
                        StatusHistory OM_statusHistory = new()
                        {
                            NextDeciderUserId = depenseCaisse_DB.NextDeciderUserId,
                            Total = depenseCaisse_DB.Total,
                            DepenseCaisseId = depenseCaisse_DB.Id,
                            Status = 13
                        };
                        result = await _statusHistoryRepository.AddStatusAsync(OM_statusHistory);
                        if (!result.Success) return result;
                    }
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
                result.Id = depenseCaisse_DB.Id;
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
                // send email to the next decider
                await _miscService.SendMailToDecider("DC", depenseCaisse_DB.NextDeciderUserId, depenseCaisse_DB.Id);
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
                var expenses = await _expenseRepository.GetDepenseCaisseExpensesByDcIdAsync(depenseCaisse.Id);
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

        public async Task<ServiceResult> DecideOnDepenseCaisse(DecisionOnRequestPostDTO decision, int deciderUserId)
        {
            ServiceResult result = new();
            DepenseCaisse decidedDepenseCaisse = await _depenseCaisseRepository.GetDepenseCaisseByIdAsync(decision.RequestId);

            // test if the decider is the one who is supposed to decide upon it
            if (decidedDepenseCaisse.NextDeciderUserId != deciderUserId)
            {
                result.Success = false;
                result.Message = "You can't decide on this request in this state! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks";
                return result;
            }

            // CASE: REJECTION / RETURN
            if (decision.DecisionString == "return" || decision.DecisionString == "reject")
            {
                var transaction = _context.Database.BeginTransaction();
                try
                {
                    decidedDepenseCaisse.DeciderComment = decision.DeciderComment;
                    decidedDepenseCaisse.DeciderUserId = deciderUserId;
                    if (decision.ReturnedToFMByTR)
                    {
                        decidedDepenseCaisse.NextDeciderUserId = await _depenseCaisseRepository.GetDepenseCaisseNextDeciderUserId("TR", decision.ReturnedToFMByTR, null);
                        decidedDepenseCaisse.LatestStatus = 14; /* Returned to finance dept for missing info */
                    }
                    else if (decision.ReturnedToTRByFM)
                    {
                        decidedDepenseCaisse.NextDeciderUserId = await _depenseCaisseRepository.GetDepenseCaisseNextDeciderUserId("FM", null, decision.ReturnedToTRByFM);
                        decidedDepenseCaisse.LatestStatus = 13; /* pending TR validation */
                    }
                    else if (decision.ReturnedToRequesterByTR)
                    {
                        decidedDepenseCaisse.NextDeciderUserId = null;
                        decidedDepenseCaisse.LatestStatus = 15; /* returned for missing evidences + next decider is still TR */
                    }
                    else
                    {
                        decidedDepenseCaisse.NextDeciderUserId = null; /* next decider is set to null if returned or rejected normally */
                        decidedDepenseCaisse.LatestStatus = decision.DecisionString.ToLower() == "return" ? 98 : 97; /* returned normaly or rejected */
                    }
                    result = await _depenseCaisseRepository.UpdateDepenseCaisseAsync(decidedDepenseCaisse);
                    if (!result.Success) return result;

                    StatusHistory decidedDepenseCaisse_SH = new()
                    {
                        DepenseCaisseId = decision.RequestId,
                        DeciderUserId = deciderUserId,
                        DeciderComment = decision.DeciderComment,
                        Total = decidedDepenseCaisse.Total,
                        Status = decision.DecisionString.ToLower() == "return" ? 98 : 97,
                        NextDeciderUserId = decidedDepenseCaisse.NextDeciderUserId,
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(decidedDepenseCaisse_SH);
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
            // CASE: APPROVE
            else
            {
                var transaction = _context.Database.BeginTransaction();
                try
                {
                    decidedDepenseCaisse.DeciderUserId = deciderUserId;
                    switch (decidedDepenseCaisse.LatestStatus)
                    {
                        case 1:
                            decidedDepenseCaisse.NextDeciderUserId = await _depenseCaisseRepository.GetDepenseCaisseNextDeciderUserId("MG");
                            decidedDepenseCaisse.LatestStatus = 3;
                            break;
                        case 3:
                            decidedDepenseCaisse.NextDeciderUserId = await _depenseCaisseRepository.GetDepenseCaisseNextDeciderUserId("FM", null, decision.ReturnedToTRByFM);
                            decidedDepenseCaisse.LatestStatus = 4; 
                            break;
                        case 4:
                            decidedDepenseCaisse.NextDeciderUserId = await _depenseCaisseRepository.GetDepenseCaisseNextDeciderUserId("GD");
                            decidedDepenseCaisse.LatestStatus = 13;
                            break;
                        case 13:
                            decidedDepenseCaisse.NextDeciderUserId = null;
                            decidedDepenseCaisse.LatestStatus = 16;
                            break;
                    }

                    result = await _depenseCaisseRepository.UpdateDepenseCaisseAsync(decidedDepenseCaisse);
                    if (!result.Success) return result;

                    StatusHistory decidedDepenseCaisse_SH = new()
                    {
                        DepenseCaisseId = decision.RequestId,
                        DeciderUserId = deciderUserId,
                        DeciderComment = decision.DeciderComment,
                        Total = decidedDepenseCaisse.Total,
                        Status = decidedDepenseCaisse.LatestStatus,
                        NextDeciderUserId = decidedDepenseCaisse.NextDeciderUserId,
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(decidedDepenseCaisse_SH);
                    if (!result.Success) return result;

                    await transaction.CommitAsync();
                    // send email to the next decider
                    await _miscService.SendMailToDecider("DC", decidedDepenseCaisse.NextDeciderUserId, decidedDepenseCaisse.Id);
                }
                catch (Exception exception)
                {
                    result.Success = false;
                    result.Message = exception.Message;
                    return result;
                }
            }

            result.Success = true;
            result.Message = "DepenseCaisse is decided upon successfully";
            return result;
        }

        public async Task<string> GenerateDepenseCaisseWordDocument(int depenseCaisseId)
        {
            DepenseCaisseDocumentDetailsDTO depenseCaisseDetails = await _depenseCaisseRepository.GetDepenseCaisseDocumentDetailsByIdAsync(depenseCaisseId);


            var signaturesDir = Path.Combine(_webHostEnvironment.WebRootPath, "Static-Files\\Signatures");
            var docPath = Path.Combine(_webHostEnvironment.WebRootPath, "Static-Files", "DEPENSE_CAISSE_DOCUMENT.docx");

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
                            return ReplaceDepenseCaisseDocumentPlaceHolders(match, depenseCaisseDetails);
                        },
                        RegExOptions = RegexOptions.IgnoreCase,
                        NewFormatting = new Formatting() { FontFamily = new Xceed.Document.NET.Font("Arial"), Bold = false }
                    };
                    docx.ReplaceText(replaceTextOptions);
#pragma warning disable CS0618 // func is obsolete

                    // Replace the expenses table
                    docx.ReplaceTextWithObject("expenses", _miscService.GenerateExpesnesTableForDocuments(docx, depenseCaisseDetails.Expenses));

                    // Replace the signatures
                    if (depenseCaisseDetails.Signers.Any(s => s.Level == "MG"))
                    {
                        docx.ReplaceText("%mg_signature%", depenseCaisseDetails.Signers.Where(s => s.Level == "MG")
                                .Select(s => $"{s.FirstName} {s.LastName}")
                                .First());
                        docx.ReplaceText("%mg_signature_date%", depenseCaisseDetails.Signers.Where(s => s.Level == "MG")
                                .Select(s => s.SignDate.ToString("dd/MM/yyyy hh:mm"))
                                .First());

                        string? imgName = depenseCaisseDetails.Signers.Where(s => s.Level == "MG")
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
                    if (depenseCaisseDetails.Signers.Any(s => s.Level == "FM"))
                    {
                        docx.ReplaceText("%fm_signature%", depenseCaisseDetails.Signers.Where(s => s.Level == "FM")
                                .Select(s => $"{s.FirstName} {s.LastName}")
                                .First());
                        docx.ReplaceText("%fm_signature_date%", depenseCaisseDetails.Signers.Where(s => s.Level == "FM")
                                .Select(s => s.SignDate.ToString("dd/MM/yyyy hh:mm"))
                                .First());

                        string? imgName = depenseCaisseDetails.Signers.Where(s => s.Level == "FM")
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
                    if (depenseCaisseDetails.Signers.Any(s => s.Level == "GD"))
                    {
                        docx.ReplaceText("%gd_signature%", depenseCaisseDetails.Signers.Where(s => s.Level == "GD")
                                .Select(s => $"{s.FirstName} {s.LastName}")
                                .First());
                        docx.ReplaceText("%gd_signature_date%", depenseCaisseDetails.Signers.Where(s => s.Level == "GD")
                                .Select(s => s.SignDate.ToString("dd/MM/yyyy hh:mm"))
                                .First());

                        string? imgName = depenseCaisseDetails.Signers.Where(s => s.Level == "GD")
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

        public string ReplaceDepenseCaisseDocumentPlaceHolders(string placeHolder, DepenseCaisseDocumentDetailsDTO depenseCaisseDetails)
        {
#pragma warning disable CS8604 // null warning.
            Dictionary<string, string> _replacePatterns = new Dictionary<string, string>()
              {
                { "id", depenseCaisseDetails.Id.ToString() },
                { "full_name", depenseCaisseDetails.FirstName + " " + depenseCaisseDetails.LastName },
                { "date", depenseCaisseDetails.SubmitDate.ToString("dd/MM/yyyy") },
                { "amount", depenseCaisseDetails.Total.ToString().FormatWith(new CultureInfo("fr-FR")) },
                { "worded_amount", ((int)depenseCaisseDetails.Total).ToWords(new CultureInfo("fr-FR")) },
                { "currency", depenseCaisseDetails.Currency.ToString() },
                { "description", depenseCaisseDetails.Description.ToString() },
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
