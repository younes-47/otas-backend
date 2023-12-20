using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OTAS.Data;
using OTAS.DTO.Get;
using OTAS.DTO.Post;
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
        private readonly IUserRepository _userRepository;
        private readonly OtasContext _context;
        private readonly IMapper _mapper;

        public AvanceCaisseService(IAvanceCaisseRepository avanceCaisseRepository,
            IStatusHistoryRepository statusHistoryRepository,
            IActualRequesterRepository actualRequesterRepository,
            IExpenseRepository expenseRepository,
            IUserRepository userRepository,
            OtasContext context,
            IMapper mapper)
        {
            _avanceCaisseRepository = avanceCaisseRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _actualRequesterRepository = actualRequesterRepository;
            _expenseRepository = expenseRepository;
            _userRepository = userRepository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ServiceResult> CreateAvanceCaisseAsync(AvanceCaissePostDTO avanceCaisse)
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
                var testAC = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync(avanceCaisseId);
                if(testAC.LatestStatus == 1)
                {
                    result.Success = false;
                    result.Message = "You've already submitted this AvanceCaisse!";
                    return result;
                }

                result = await _avanceCaisseRepository.UpdateAvanceCaisseStatusAsync(avanceCaisseId, 1);
                if (!result.Success) return result;

                AvanceCaisse av = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync(avanceCaisseId);
                StatusHistory newOM_Status = new()
                {
                    Total = av.EstimatedTotal,
                    AvanceCaisseId = avanceCaisseId,
                    Status = 1,
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

        public async Task<ServiceResult> DecideOnAvanceCaisse(int avanceCaisseId, string? advanceOption, int deciderUserId, string? deciderComment, int decision)
        {

            ServiceResult result = new();
            AvanceCaisse decidedAvanceCaisse = await _avanceCaisseRepository.GetAvanceCaisseByIdAsync(avanceCaisseId);

            //Test if the request is not on a state where you can't decide upon (99, 98, 97)
            if (decidedAvanceCaisse.LatestStatus == 99 || decidedAvanceCaisse.LatestStatus == 98 || decidedAvanceCaisse.LatestStatus == 97)
            {
                result.Success = false;
                result.Message = "You can't decide on this request in this state! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks";
                return result;
            }

            // Test if the decider is allowed to decide on the request on this level
            if (await _userRepository.GetUserRoleByUserIdAsync(deciderUserId) != (decidedAvanceCaisse.LatestStatus + 2))
            // Here the role of the decider should always be equal to the latest status of the request + 2 (refer to one note). If not, he is trying to approve on a different level from his.
            {
                result.Success = false;
                result.Message = "You are not allowed to approve on this level! Either the request is still not forwarded to you yet, or you have already decided upon it. If you think this error is not supposed to occur, report the IT department with the issue. Thanks";
                return result;
            }



            //Test If it has been already decided upon it by the decider or it has been forwarded to the next decider
            int deciderRole_Request = await _userRepository.GetUserRoleByUserIdAsync(deciderUserId);
            int deciderRole_DB = 0;
            if (decidedAvanceCaisse.DeciderUserId != null)
            {
                deciderRole_DB = await _userRepository.GetUserRoleByUserIdAsync((int)decidedAvanceCaisse.DeciderUserId);
            }

                 //If the decider userid is already assigned to the request || or the decider role is smaller than the role of the decider that has decided on the request
            if (decidedAvanceCaisse.DeciderUserId == deciderUserId || deciderRole_Request < deciderRole_DB)
            {
                result.Success = false;
                result.Message = "AvanceCaisse is already decided upon! If you think this error is not supposed to occur, report the IT department with the issue. If not, please don't attempt to manipulate the system. Thanks";
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
                    decidedAvanceCaisse.DeciderComment = deciderComment;
                    decidedAvanceCaisse.DeciderUserId = deciderUserId;
                    decidedAvanceCaisse.LatestStatus = decision;
                    result = await _avanceCaisseRepository.UpdateAvanceCaisseAsync(decidedAvanceCaisse);
                    if (!result.Success) return result;

                    StatusHistory decidedAvanceCaisse_SH = new()
                    {
                        AvanceCaisseId = avanceCaisseId,
                        DeciderUserId = deciderUserId,
                        DeciderComment = deciderComment,
                        Total = decidedAvanceCaisse.EstimatedTotal,
                        Status = decision,
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
                    decidedAvanceCaisse.DeciderUserId = deciderUserId;
                    if (decidedAvanceCaisse.LatestStatus == 5) decidedAvanceCaisse.LatestStatus = 7; //Preparing funds in case of TR Approval (6 is approved, and it will be assigned only for OM not AV)
                    decidedAvanceCaisse.LatestStatus++; // Otherwise next step of approval process.
                    decidedAvanceCaisse.AdvanceOption = advanceOption;
                    result = await _avanceCaisseRepository.UpdateAvanceCaisseAsync(decidedAvanceCaisse);
                    if (!result.Success) return result;

                    StatusHistory decidedAvanceCaisse_SH = new()
                    {
                        AvanceCaisseId = avanceCaisseId,
                        DeciderUserId = deciderUserId,
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

        public async Task<ServiceResult> ModifyAvanceCaisse(AvanceCaissePostDTO avanceCaisse, int action) // action is either 99 OR 1 ; 99 -> save as draft again, 1 -> submit
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

                if(action == 99 && avanceCaisse_DB.LatestStatus == 98)
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
                    mappedActualRequester.OrderingUserId = avanceCaisse.UserId;
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
                avanceCaisse_DB.LatestStatus = action;
                avanceCaisse_DB.Description = avanceCaisse.Description;
                avanceCaisse_DB.Currency = avanceCaisse.Currency;
                avanceCaisse_DB.OnBehalf = avanceCaisse.OnBehalf;
                avanceCaisse_DB.EstimatedTotal = CalculateExpensesEstimatedTotal(mappedExpenses);
                result = await _avanceCaisseRepository.UpdateAvanceCaisseAsync(avanceCaisse_DB);
                if (!result.Success) return result;


                //Insert new status history in case of a submit or re submit action
                if (action == 1)
                {
                    StatusHistory OM_statusHistory = new()
                    {
                        Total = avanceCaisse_DB.EstimatedTotal,
                        AvanceCaisseId = avanceCaisse_DB.Id,
                        Status = 1
                    };
                    result = await _statusHistoryRepository.AddStatusAsync(OM_statusHistory);
                    if (!result.Success) return result;
                }
                else
                {
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

            if (action == 1)
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
