using AutoMapper;
using Humanizer;
using OTAS.Data;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.DTO.Put;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Xceed.Document.NET;
using Xceed.Words.NET;


namespace OTAS.Services
{
    public class AvanceCaisseService : IAvanceCaisseService
    {
        private readonly IAvanceCaisseRepository _avanceCaisseRepository;
        private readonly IStatusHistoryRepository _statusHistoryRepository;
        private readonly IActualRequesterRepository _actualRequesterRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IDeciderRepository _deciderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMiscService _miscService;
        private readonly OtasContext _context;
        private readonly IMapper _mapper;

        public AvanceCaisseService(IAvanceCaisseRepository avanceCaisseRepository,
            IStatusHistoryRepository statusHistoryRepository,
            IActualRequesterRepository actualRequesterRepository,
            IWebHostEnvironment webHostEnvironment,
            IExpenseRepository expenseRepository,
            IDeciderRepository deciderRepository,
            IUserRepository userRepository,
            IMiscService miscService,
            OtasContext context,
            IMapper mapper)
        {
            _avanceCaisseRepository = avanceCaisseRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _actualRequesterRepository = actualRequesterRepository;
            _webHostEnvironment = webHostEnvironment;
            _expenseRepository = expenseRepository;
            _deciderRepository = deciderRepository;
            _userRepository = userRepository;
            _miscService = miscService;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResult> CreateAvanceCaisseAsync(AvanceCaissePostDTO avanceCaisse, int userId)
        {
            var transaction = _context.Database.BeginTransaction();
            ServiceResult result = new();
            try
            {

                var mappedAC = _mapper.Map<AvanceCaisse>(avanceCaisse);
                List<Expense> expenses = _mapper.Map<List<Expense>>(avanceCaisse.Expenses);
                mappedAC.EstimatedTotal = _miscService.CalculateExpensesEstimatedTotal(expenses);
                mappedAC.UserId = userId;
                result = await _avanceCaisseRepository.AddAvanceCaisseAsync(mappedAC);
                if (!result.Success) return result;

                StatusHistory AV_status = new()
                {
                    Total = mappedAC.EstimatedTotal,
                    AvanceCaisseId = mappedAC.Id,
                    Status = mappedAC.LatestStatus
                };
                result = await _statusHistoryRepository.AddStatusAsync(AV_status);
                if (!result.Success)
                {
                    result.Message += " (AvanceCaisse)";
                    return result;
                }

                // Handle Actual Requester Info (if exists)
                if (avanceCaisse.OnBehalf == true)
                {
                    ActualRequester mappedActualRequester = _mapper.Map<ActualRequester>(avanceCaisse.ActualRequester);
                    mappedActualRequester.AvanceCaisseId = mappedAC.Id;
                    mappedActualRequester.OrderingUserId = mappedAC.UserId;
                    mappedActualRequester.ManagerUserId = await _userRepository.GetUserIdByUsernameAsync(avanceCaisse.ActualRequester.ManagerUserName);
                    result = await _actualRequesterRepository.AddActualRequesterInfoAsync(mappedActualRequester);
                    if (!result.Success) return result;
                }

                //Inserting the expenses
                foreach (Expense expense in expenses)
                {
                    expense.AvanceCaisseId = mappedAC.Id;
                    expense.Currency = mappedAC.Currency;
                    result = await _expenseRepository.AddExpenseAsync(expense);
                    if (!result.Success) return result;
                }

                await transaction.CommitAsync();
                result.Id = mappedAC.Id;
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.Message = exception.Message + " ERROR TYPE: " + exception.GetType();
                return result;
            }
            result.Success = true;
            result.Message = "AvanceCaisse created successfully";
            return result;

        }

        public async Task<ServiceResult> SubmitAvanceCaisse(AvanceCaisse avanceCaisse_DB)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();

            try
            {
                var nextDeciderUserId = await _deciderRepository.GetManagerUserIdByUserIdAsync(avanceCaisse_DB.UserId);
                avanceCaisse_DB.NextDeciderUserId = nextDeciderUserId;
                avanceCaisse_DB.LatestStatus = 1;
                result = await _avanceCaisseRepository.UpdateAvanceCaisseAsync(avanceCaisse_DB);
                if (!result.Success) return result;

                StatusHistory newOM_Status = new()
                {
                    Total = avanceCaisse_DB.EstimatedTotal,
                    AvanceCaisseId = avanceCaisse_DB.Id,
                    Status = 1,
                    NextDeciderUserId = nextDeciderUserId,
                };
                result = await _statusHistoryRepository.AddStatusAsync(newOM_Status);
                if (!result.Success)
                {
                    result.Message += " for the newly submitted \"AvanceCaisse\"";
                    return result;
                }

                await transaction.CommitAsync();
                // send email to the next decider
                await _miscService.SendMailToDecider("AC", avanceCaisse_DB.NextDeciderUserId, avanceCaisse_DB.Id);
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.Message = exception.Message + " ERROR TYPE: " + exception.GetType();
                return result;
            }

            result.Success = true;
            result.Message = "AvanceCaisse is Submitted successfully";
            return result;

        }


        public async Task<ServiceResult> DecideOnAvanceCaisse(DecisionOnRequestPostDTO decision, int deciderUserId)
        {

            ServiceResult result = new();
            AvanceCaisse decidedAvanceCaisse = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync(decision.RequestId);

            // test if the decider is the one who is supposed to decide upon it
            if (decidedAvanceCaisse.NextDeciderUserId != deciderUserId)
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
                    decidedAvanceCaisse.DeciderComment = decision.DeciderComment;
                    decidedAvanceCaisse.DeciderUserId = deciderUserId;
                    decidedAvanceCaisse.NextDeciderUserId = null;
                    decidedAvanceCaisse.LatestStatus = decision.DecisionString.ToLower() == "return" ? 98 : 97;
                    result = await _avanceCaisseRepository.UpdateAvanceCaisseAsync(decidedAvanceCaisse);
                    if (!result.Success) return result;

                    StatusHistory decidedAvanceCaisse_SH = new()
                    {
                        AvanceCaisseId = decision.RequestId,
                        DeciderUserId = deciderUserId,
                        DeciderComment = decision.DeciderComment,
                        Total = decidedAvanceCaisse.EstimatedTotal,
                        NextDeciderUserId = decidedAvanceCaisse.NextDeciderUserId,
                        Status = decision.DecisionString.ToLower() == "return" ? 98 : 97,
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
            // CASE: Approval
            else
            {
                var transaction = _context.Database.BeginTransaction();
                try
                {
                    decidedAvanceCaisse.DeciderUserId = deciderUserId;

                    switch (decidedAvanceCaisse.LatestStatus)
                    {
                        case 1:
                            decidedAvanceCaisse.NextDeciderUserId = await _avanceCaisseRepository.GetAvanceCaisseNextDeciderUserId("MG");
                            decidedAvanceCaisse.LatestStatus = 3;
                            break;
                        case 3:
                            decidedAvanceCaisse.NextDeciderUserId = await _avanceCaisseRepository.GetAvanceCaisseNextDeciderUserId("FM");
                            decidedAvanceCaisse.LatestStatus = 4;
                            break;
                        case 4:
                            decidedAvanceCaisse.NextDeciderUserId = await _avanceCaisseRepository.GetAvanceCaisseNextDeciderUserId("GD");
                            decidedAvanceCaisse.LatestStatus = 8;
                            break;
                    }

                    result = await _avanceCaisseRepository.UpdateAvanceCaisseAsync(decidedAvanceCaisse);
                    if (!result.Success) return result;

                    StatusHistory decidedAvanceCaisse_SH = new()
                    {
                        AvanceCaisseId = decidedAvanceCaisse.Id,
                        DeciderUserId = decidedAvanceCaisse.DeciderUserId,
                        NextDeciderUserId = decidedAvanceCaisse.NextDeciderUserId,
                        Status = decidedAvanceCaisse.LatestStatus,
                        Total = decidedAvanceCaisse.EstimatedTotal,
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(decidedAvanceCaisse_SH);
                    if (!result.Success) return result;


                    await transaction.CommitAsync();

                    // send email to the next decider
                    await _miscService.SendMailToDecider("AC", decidedAvanceCaisse.NextDeciderUserId, decidedAvanceCaisse.Id);
                }
                catch (Exception exception)
                {
                    result.Success = false;
                    result.Message = exception.Message + " ERROR TYPE: " + exception.GetType();
                    return result;
                }
            }

            result.Success = true;
            result.Message = "AvanceCaisse has been decided upon successfully";
            return result;

        }

        public async Task<ServiceResult> MarkFundsAsPrepared(int avanceCaisseId, string advanceOption, int deciderUserId)
        {
            ServiceResult result = new();
            AvanceCaisse decidedAvanceCaisse = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync(avanceCaisseId);

            var transaction = _context.Database.BeginTransaction();
            try
            {
                decidedAvanceCaisse.AdvanceOption = advanceOption;
                decidedAvanceCaisse.DeciderUserId = deciderUserId;
                decidedAvanceCaisse.LatestStatus = 9;

                /*
                    Generates a random number of 8
                    CAUTION: DO NOT EXCEED 10 IN LENGTH AS THIS WILL CAUSE AN OVERFLOW EXCEPTION
                 */
                decidedAvanceCaisse.ConfirmationNumber = _miscService.GenerateRandomNumber(8); //  
                result = await _avanceCaisseRepository.UpdateAvanceCaisseAsync(decidedAvanceCaisse);
                if (!result.Success) return result;

                StatusHistory decidedAvanceCaisse_SH = new()
                {
                    AvanceCaisseId = decidedAvanceCaisse.Id,
                    DeciderUserId = decidedAvanceCaisse.DeciderUserId,
                    Total = decidedAvanceCaisse.EstimatedTotal,
                    NextDeciderUserId = decidedAvanceCaisse.NextDeciderUserId,
                    Status = decidedAvanceCaisse.LatestStatus,
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


            result.Success = true;
            result.Message = "AvanceCaisse has been decided upon successfully";
            return result;
        }

        public async Task<ServiceResult> ConfirmFundsDelivery(int avanceCaisseId, int confirmationNumber)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();
            try
            {
                AvanceCaisse avanceCaisse_DB = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync(avanceCaisseId);

                if (avanceCaisse_DB.ConfirmationNumber != confirmationNumber)
                {
                    result.Success = false;
                    result.Message = "Wrong number!";
                    return result;
                }

                avanceCaisse_DB.NextDeciderUserId = null;
                avanceCaisse_DB.DeciderUserId = null;
                avanceCaisse_DB.LatestStatus = 10;

                result = await _avanceCaisseRepository.UpdateAvanceCaisseAsync(avanceCaisse_DB);
                if (!result.Success) return result;

                StatusHistory decidedAvanceCaisse_SH = new()
                {
                    AvanceCaisseId = avanceCaisse_DB.Id,
                    DeciderUserId = avanceCaisse_DB.DeciderUserId,
                    Total = avanceCaisse_DB.EstimatedTotal,
                    NextDeciderUserId = avanceCaisse_DB.NextDeciderUserId,
                    Status = avanceCaisse_DB.LatestStatus,
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

            result.Success = true;
            result.Message = "AvanceCaisse funds has been confirmed as collected!";
            return result;
        }

        public async Task<ServiceResult> ModifyAvanceCaisse(AvanceCaissePutDTO avanceCaisse)
        {
            var transaction = _context.Database.BeginTransaction();
            ServiceResult result = new();
            try
            {
                AvanceCaisse avanceCaisse_DB = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync(avanceCaisse.Id);

                // Handle Onbehalf case
                if (avanceCaisse.OnBehalf == true)
                {
                    //Delete actualrequester info first regardless
                    ActualRequester? actualRequester = await _actualRequesterRepository.FindActualrequesterInfoByAvanceCaisseIdAsync(avanceCaisse.Id);
                    if (actualRequester != null)
                    {
                        result = await _actualRequesterRepository.DeleteActualRequesterInfoAsync(actualRequester);
                        if (!result.Success) return result;
                    }

                    //Insert the new one coming from request
                    ActualRequester mappedActualRequester = _mapper.Map<ActualRequester>(avanceCaisse.ActualRequester);
                    mappedActualRequester.AvanceCaisseId = avanceCaisse.Id;
                    mappedActualRequester.OrderingUserId = avanceCaisse_DB.UserId;
                    mappedActualRequester.ManagerUserId = await _userRepository.GetUserIdByUsernameAsync(avanceCaisse.ActualRequester.ManagerUserName);
                    result = await _actualRequesterRepository.AddActualRequesterInfoAsync(mappedActualRequester);
                    if (!result.Success) return result;
                }
                else
                {
                    ActualRequester? actualRequesterInfo = await _actualRequesterRepository.FindActualrequesterInfoByAvanceCaisseIdAsync(avanceCaisse.Id);
                    if (actualRequesterInfo != null)
                    {
                        //Make sure to delete actualrequester in case the user modifies "OnBehalf" Prop to false and an actual requester exists in database
                        result = await _actualRequesterRepository.DeleteActualRequesterInfoAsync(actualRequesterInfo);
                        if (!result.Success) return result;
                    }
                }

                //Delete all expenses from DB (related to this ac!)
                var expenses_DB = await _expenseRepository.GetAvanceCaisseExpensesByAcIdAsync(avanceCaisse.Id);
                await _expenseRepository.DeleteExpenses(expenses_DB);

                //Map each expense coming from the request and insert them
                var mappedExpenses = _mapper.Map<List<Expense>>(avanceCaisse.Expenses);
                foreach (Expense expense in mappedExpenses)
                {
                    expense.AvanceCaisseId = avanceCaisse_DB.Id;
                    expense.Currency = avanceCaisse.Currency;
                }
                result = await _expenseRepository.AddExpensesAsync(mappedExpenses);
                if (!result.Success) return result;

                // Map the fetched AC from the DB with the new values and update it
                avanceCaisse_DB.DeciderComment = null;
                avanceCaisse_DB.DeciderUserId = null;
                avanceCaisse_DB.LatestStatus = avanceCaisse.Action.ToLower() == "save" ? 99 : 1;
                avanceCaisse_DB.Description = avanceCaisse.Description;
                avanceCaisse_DB.Currency = avanceCaisse.Currency;
                avanceCaisse_DB.OnBehalf = avanceCaisse.OnBehalf;
                avanceCaisse_DB.EstimatedTotal = _miscService.CalculateExpensesEstimatedTotal(mappedExpenses);

                //Insert new status history in case of a submit or re submit action
                if (avanceCaisse.Action.ToLower() == "submit")
                {
                    var managerUserId = await _deciderRepository.GetManagerUserIdByUserIdAsync(avanceCaisse_DB.UserId);
                    avanceCaisse_DB.NextDeciderUserId = managerUserId;
                    result = await _avanceCaisseRepository.UpdateAvanceCaisseAsync(avanceCaisse_DB);
                    if (!result.Success) return result;

                    StatusHistory OM_statusHistory = new()
                    {
                        NextDeciderUserId = managerUserId,
                        Total = avanceCaisse_DB.EstimatedTotal,
                        AvanceCaisseId = avanceCaisse_DB.Id,
                        Status = 1
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(OM_statusHistory);
                    if (!result.Success) return result;
                }
                else
                {
                    result = await _avanceCaisseRepository.UpdateAvanceCaisseAsync(avanceCaisse_DB);
                    if (!result.Success) return result;

                    // Just Update Status History Total in case of saving
                    result = await _statusHistoryRepository.UpdateStatusHistoryTotal(avanceCaisse_DB.Id, "AC", avanceCaisse_DB.EstimatedTotal);
                    if (!result.Success) return result;
                }


                await transaction.CommitAsync();
                result.Id = avanceCaisse_DB.Id;
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.Message = exception.Message + " ERROR TYPE: " + exception.GetType();
                return result;
            }

            if (avanceCaisse.Action.ToLower() == "submit")
            {
                result.Success = true;
                result.Message = "AvanceCaisse is resubmitted successfully";
                return result;
            }

            result.Success = true;
            result.Message = "Changes made to AvanceCaisse are saved successfully";
            return result;

        }

        public async Task<ServiceResult> DeleteDraftedAvanceCaisse(AvanceCaisse avanceCaisse)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();
            try
            {

                // Delete expenses & status History
                var expenses = await _expenseRepository.GetAvanceCaisseExpensesByAcIdAsync(avanceCaisse.Id);
                var statusHistories = await _statusHistoryRepository.GetAvanceCaisseStatusHistory(avanceCaisse.Id);

                result = await _expenseRepository.DeleteExpenses(expenses);
                if (!result.Success) return result;

                result = await _statusHistoryRepository.DeleteStatusHistories(statusHistories);
                if (!result.Success) return result;


                if (avanceCaisse.OnBehalf == true)
                {
                    ActualRequester actualRequester = await _actualRequesterRepository.FindActualrequesterInfoByAvanceCaisseIdAsync(avanceCaisse.Id);
                    result = await _actualRequesterRepository.DeleteActualRequesterInfoAsync(actualRequester);
                    if (!result.Success) return result;
                }

                // delete AC
                result = await _avanceCaisseRepository.DeleteAvanceCaisseAync(avanceCaisse);
                if (!result.Success) return result;

                await transaction.CommitAsync();

            }
            catch (Exception exception)
            {
                transaction.Rollback();
                result.Success = false;
                result.Message = exception.Message + " ERROR TYPE: " + exception.GetType();
                return result;
            }
            result.Success = true;
            result.Message = "\"AvanceCaisse\" has been deleted successfully";
            return result;
        }

        public async Task<string> GenerateAvanceCaisseWordDocument(int avanceCaisseId)
        {
            AvanceCaisseDocumentDetailsDTO avanceCaisseDetails = await _avanceCaisseRepository.GetAvanceCaisseDocumentDetailsByIdAsync(avanceCaisseId);
            

            var signaturesDir = Path.Combine(_webHostEnvironment.WebRootPath, "Static-Files\\Signatures");
            var docPath = Path.Combine(_webHostEnvironment.WebRootPath, "Static-Files", "AVANCE_CAISSE_DOCUMENT.docx");

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
                            return ReplaceAvanceCaisseDocumentPlaceHolders(match, avanceCaisseDetails);
                        },
                        RegExOptions = RegexOptions.IgnoreCase,
                        NewFormatting = new Formatting() { FontFamily = new Xceed.Document.NET.Font("Arial"), Bold = false }
                    };
                    docx.ReplaceText(replaceTextOptions);
#pragma warning disable CS0618 // func is obsolete

                    // Replace the expenses table
                    docx.ReplaceTextWithObject("expenses", _miscService.GenerateExpesnesTableForDocuments(docx, avanceCaisseDetails.Expenses));

                    // Replace the signatures
                    if (avanceCaisseDetails.Signers.Any(s => s.Level == "MG"))
                    {
                        docx.ReplaceText("%mg_signature%", avanceCaisseDetails.Signers.Where(s => s.Level == "MG")
                                .Select(s => $"{s.FirstName} {s.LastName}")
                                .First());
                        docx.ReplaceText("%mg_signature_date%", avanceCaisseDetails.Signers.Where(s => s.Level == "MG")
                                .Select(s => s.SignDate.ToString("dd/MM/yyyy hh:mm"))
                                .First());

                        string? imgName = avanceCaisseDetails.Signers.Where(s => s.Level == "MG")
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
                    if (avanceCaisseDetails.Signers.Any(s => s.Level == "FM"))
                    {
                        docx.ReplaceText("%fm_signature%", avanceCaisseDetails.Signers.Where(s => s.Level == "FM")
                                .Select(s => $"{s.FirstName} {s.LastName}")
                                .First());
                        docx.ReplaceText("%fm_signature_date%", avanceCaisseDetails.Signers.Where(s => s.Level == "FM")
                                .Select(s => s.SignDate.ToString("dd/MM/yyyy hh:mm"))
                                .First());

                        string? imgName = avanceCaisseDetails.Signers.Where(s => s.Level == "FM")
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
                    if (avanceCaisseDetails.Signers.Any(s => s.Level == "GD"))
                    {
                        docx.ReplaceText("%gd_signature%", avanceCaisseDetails.Signers.Where(s => s.Level == "GD")
                                .Select(s => $"{s.FirstName} {s.LastName}")
                                .First());
                        docx.ReplaceText("%gd_signature_date%", avanceCaisseDetails.Signers.Where(s => s.Level == "GD")
                                .Select(s => s.SignDate.ToString("dd/MM/yyyy hh:mm"))
                                .First());

                        string? imgName = avanceCaisseDetails.Signers.Where(s => s.Level == "GD")
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

        public string ReplaceAvanceCaisseDocumentPlaceHolders(string placeHolder, AvanceCaisseDocumentDetailsDTO avanceCaisseDetails)
        {
            #pragma warning disable CS8604 // null warning.
            Dictionary<string, string> _replacePatterns = new Dictionary<string, string>()
              {
                { "id", avanceCaisseDetails.Id.ToString() },
                { "full_name", avanceCaisseDetails.FirstName + " " + avanceCaisseDetails.LastName },
                { "confirmation_number", avanceCaisseDetails.ConfirmationNumber != null ?
                                                avanceCaisseDetails.ConfirmationNumber.ToString()
                                                : "N/A"},
                { "date", avanceCaisseDetails.SubmitDate.ToString("dd/MM/yyyy") },
                { "amount", avanceCaisseDetails.EstimatedTotal.ToString().FormatWith(new CultureInfo("fr-FR")) },
                { "worded_amount", ((int)avanceCaisseDetails.EstimatedTotal).ToWords(new CultureInfo("fr-FR")) },
                { "currency", avanceCaisseDetails.Currency.ToString() },
                { "description", avanceCaisseDetails.Description.ToString() },
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
