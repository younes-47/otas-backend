using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OTAS.Data;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.DTO.Put;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;
using OTAS.Repository;
using System;


namespace OTAS.Services
{
    public class AvanceCaisseService : IAvanceCaisseService
    {
        private readonly IAvanceCaisseRepository _avanceCaisseRepository;
        private readonly IStatusHistoryRepository _statusHistoryRepository;
        private readonly IActualRequesterRepository _actualRequesterRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IDeciderRepository _deciderRepository;
        private readonly IUserRepository _userRepository;
        private readonly OtasContext _context;
        private readonly IMapper _mapper;

        public AvanceCaisseService(IAvanceCaisseRepository avanceCaisseRepository,
            IStatusHistoryRepository statusHistoryRepository,
            IActualRequesterRepository actualRequesterRepository,
            IExpenseRepository expenseRepository,
            IDeciderRepository deciderRepository,
            IUserRepository userRepository,
            OtasContext context,
            IMapper mapper)
        {
            _avanceCaisseRepository = avanceCaisseRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _actualRequesterRepository = actualRequesterRepository;
            _expenseRepository = expenseRepository;
            _deciderRepository = deciderRepository;
            _userRepository = userRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResult> CreateAvanceCaisseAsync(AvanceCaissePostDTO avanceCaisse, int userId)
        {
            var transaction = _context.Database.BeginTransaction();
            ServiceResult result = new();
            try
            {
                if (avanceCaisse.Expenses.Count < 1)
                {
                    result.Success = false;
                    result.Message = "AvanceCaisse must have at least one expense! You can't request an AvanceCaisse with no expense.";
                    return result;
                }

                if (avanceCaisse.Currency != "MAD" && avanceCaisse.Currency != "EUR")
                {
                    result.Success = false;
                    result.Message = "Invalid currency! Please choose suitable currency from the dropdownmenu!";
                    return result;
                }

                var mappedAC = _mapper.Map<AvanceCaisse>(avanceCaisse);
                List<Expense> expenses = _mapper.Map<List<Expense>>(avanceCaisse.Expenses);
                mappedAC.EstimatedTotal = CalculateExpensesEstimatedTotal(expenses);
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
                    if (avanceCaisse.ActualRequester == null)
                    {
                        result.Success = false;
                        result.Message = "You must fill actual requester's info in case you are filing this request on behalf of someone";
                        return result;
                    }

                    ActualRequester mappedActualRequester = _mapper.Map<ActualRequester>(avanceCaisse.ActualRequester);
                    mappedActualRequester.AvanceCaisseId = mappedAC.Id;
                    mappedActualRequester.OrderingUserId = mappedAC.UserId;
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
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.Message = exception.Message + exception.GetType() + exception.StackTrace;
                return result;
            }
            result.Success = true;
            result.Message = "AvanceCaisse created successfully";
            return result;

        }

        public async Task<ServiceResult> SubmitAvanceCaisse(int avanceCaisseId)
        {
            ServiceResult result = new();
            var transaction = _context.Database.BeginTransaction();

            try
            {
                AvanceCaisse avanceCaisse_DB = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync(avanceCaisseId);
                if(avanceCaisse_DB.LatestStatus == 1)
                {
                    result.Success = false;
                    result.Message = "You've already submitted this AvanceCaisse!";
                    return result;
                }

                var nextDeciderUserId = await _deciderRepository.GetManagerUserIdByUserIdAsync(avanceCaisse_DB.UserId);
                avanceCaisse_DB.NextDeciderUserId = nextDeciderUserId;
                avanceCaisse_DB.LatestStatus = 1;
                result = await _avanceCaisseRepository.UpdateAvanceCaisseAsync(avanceCaisse_DB);
                if (!result.Success) return result;

                StatusHistory newOM_Status = new()
                {
                    Total = avanceCaisse_DB.EstimatedTotal,
                    AvanceCaisseId = avanceCaisseId,
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
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.Message = exception.Message;
                return result;
            }

            result.Success = true;
            result.Message = "AvanceCaisse is Submitted successfully";
            return result;

        }

        public async Task<AvanceCaisseFullDetailsDTO> GetAvanceCaisseFullDetailsById(int avanceCaisseId)
        {
            var avanceCaisse = await _context.AvanceCaisses
                .Where(ac => ac.Id == avanceCaisseId)
                .ProjectTo<AvanceCaisseFullDetailsDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            //For some reason Automapper doesn't map the user object
            avanceCaisse.User = _mapper.Map<UserDTO>(await _context.Users.Where(user => user.Id == avanceCaisse.UserId).FirstAsync());

            return avanceCaisse;
        }

        public async Task<List<AvanceCaisseDTO>> GetAvanceCaissesForDeciderTable(int deciderRole)
        {
            List<AvanceCaisse> avanceCaisses = await _avanceCaisseRepository.GetAvancesCaisseByStatusAsync(deciderRole - 1); //See oneNote sketches to understand why it is role-1
            List<AvanceCaisseDTO> mappedAvanceCaisses = _mapper.Map<List<AvanceCaisseDTO>>(avanceCaisses);
            return mappedAvanceCaisses;
        }

        public async Task<ServiceResult> DecideOnAvanceCaisse(DecisionOnRequestPostDTO decision)
        {

            ServiceResult result = new();
            AvanceCaisse decidedAvanceCaisse = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync(decision.RequestId);

            //Test if the request is not on a state where you can't decide upon (99, 98, 97)
            if (decidedAvanceCaisse.NextDeciderUserId != decision.DeciderUserId)
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
                    decidedAvanceCaisse.DeciderUserId = decision.DeciderUserId;
                    decidedAvanceCaisse.LatestStatus = decision.DecisionString.ToLower() == "return" ? 98 : 97;
                    result = await _avanceCaisseRepository.UpdateAvanceCaisseAsync(decidedAvanceCaisse);
                    if (!result.Success) return result;

                    StatusHistory decidedAvanceCaisse_SH = new()
                    {
                        AvanceCaisseId = decision.RequestId,
                        DeciderUserId = decision.DeciderUserId,
                        DeciderComment = decision.DeciderComment,
                        Total = decidedAvanceCaisse.EstimatedTotal,
                        Status = decision.DecisionString.ToLower() == "return" ? 98 : 97,
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(decidedAvanceCaisse_SH);
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
            // CASE: Approval
            else
            {
                var transaction = _context.Database.BeginTransaction();
                try
                {
                    decidedAvanceCaisse.DeciderUserId = decision.DeciderUserId;

                    var deciderLevel = await _deciderRepository.GetDeciderLevelByUserId(decision.DeciderUserId);
                    switch (deciderLevel)
                    {
                        case "MG":
                            decidedAvanceCaisse.NextDeciderUserId = await _avanceCaisseRepository.GetAvanceCaisseNextDeciderUserId("MG");
                            decidedAvanceCaisse.LatestStatus = 3;
                            break;
                        case "FM":
                            decidedAvanceCaisse.NextDeciderUserId = await _avanceCaisseRepository.GetAvanceCaisseNextDeciderUserId("FM");
                            decidedAvanceCaisse.LatestStatus = 4;
                            break;
                        case "GD":
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
                }
                catch (Exception exception)
                {
                    result.Success = false;
                    result.Message = exception.Message;
                    return result;
                }
            }

            result.Success = true;
            result.Message = "AvanceCaisse has been decided upon successfully";
            return result;

        }

        public async Task<ServiceResult> ModifyAvanceCaisse(AvanceCaissePutDTO avanceCaisse)
        {
            var transaction = _context.Database.BeginTransaction();
            ServiceResult result = new();
            try
            {
                AvanceCaisse avanceCaisse_DB = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync(avanceCaisse.Id);

                if (avanceCaisse_DB.LatestStatus == 97)
                {
                    result.Success = false;
                    result.Message = "You can't modify or resubmit a rejected request!";
                    return result;
                }

                if (avanceCaisse_DB.LatestStatus != 98 && avanceCaisse_DB.LatestStatus != 99)
                {
                    result.Success = false;
                    result.Message = "The AvanceCaisse you are trying to modify is not a draft nor returned! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks";
                    return result;
                }

                if(avanceCaisse.Action.ToLower() == "save" && avanceCaisse_DB.LatestStatus == 98)
                {
                    result.Success = false;
                    result.Message = "You can't modify and save a returned request as a draft again. You may want to apply your modifications to you request and resubmit it directly";
                    return result;
                }

                if (avanceCaisse.Expenses.Count < 1)
                {
                    result.Success = false;
                    result.Message = "AvanceCaisse must have at least one expense! You can't request an AvanceCaisse with no expense.";
                    return result;
                }


                // Handle Onbehalf case
                if (avanceCaisse.OnBehalf == true)
                {
                    if (avanceCaisse.ActualRequester == null)
                    {
                        result.Success = false;
                        result.Message = "You must fill actual requester's info in case you are filing this request on behalf of someone";
                        return result;
                    }

                    //Delete actualrequester info first regardless
                    ActualRequester? actualRequester = await _actualRequesterRepository.FindActualrequesterInfoByAvanceCaisseIdAsync(avanceCaisse.Id);
                    if(actualRequester != null)
                    {
                        result = await _actualRequesterRepository.DeleteActualRequesterInfoAsync(actualRequester);
                        if (!result.Success) return result;
                    }

                    //Insert the new one coming from request
                    ActualRequester mappedActualRequester = _mapper.Map<ActualRequester>(avanceCaisse.ActualRequester);
                    mappedActualRequester.AvanceCaisseId = avanceCaisse.Id;
                    mappedActualRequester.OrderingUserId = avanceCaisse_DB.UserId;
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
                var expenses_DB = await _expenseRepository.GetAvanceCaisseExpensesByAvIdAsync(avanceCaisse.Id);
                await _expenseRepository.DeleteExpenses(expenses_DB);

                //Map each expense coming from the request and insert them
                var mappedExpenses = _mapper.Map<List<Expense>>(avanceCaisse.Expenses);
                foreach (Expense expense in mappedExpenses)
                {
                    expense.AvanceCaisseId = avanceCaisse_DB.Id;
                    expense.Currency = avanceCaisse.Currency;
                }
                result = await _expenseRepository.AddExpensesAsync(mappedExpenses);
                if(!result.Success) return result;

                // Map the fetched AC from the DB with the new values and update it
                avanceCaisse_DB.DeciderComment = null;
                avanceCaisse_DB.DeciderUserId = null;
                avanceCaisse_DB.LatestStatus = avanceCaisse.Action.ToLower() == "save" ? 99 : 1;
                avanceCaisse_DB.Description = avanceCaisse.Description;
                avanceCaisse_DB.Currency = avanceCaisse.Currency;
                avanceCaisse_DB.OnBehalf = avanceCaisse.OnBehalf;
                avanceCaisse_DB.EstimatedTotal = CalculateExpensesEstimatedTotal(mappedExpenses);

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
            }
            catch (Exception exception)
            {
                result.Success = false;
                result.Message = exception.Message + exception.GetType() + exception.StackTrace;
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
