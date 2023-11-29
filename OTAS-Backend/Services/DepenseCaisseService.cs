using AutoMapper;
using OTAS.Data;
using OTAS.DTO.Post;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
using OTAS.Models;


namespace OTAS.Services
{
    public class DepenseCaisseService : IDepenseCaisseService
    {
        private readonly IDepenseCaisseRepository _depenseCaisseRepository;
        private readonly IActualRequesterRepository _actualRequesterRepository;
        private readonly IStatusHistoryRepository _statusHistoryRepository;
        private readonly IExpenseRepository _expenseRepository;
        private readonly IMapper _mapper;
        private readonly OtasContext _context;

        public DepenseCaisseService(IDepenseCaisseRepository depenseCaisseRepository,
            IActualRequesterRepository actualRequesterRepository,
            IStatusHistoryRepository statusHistoryRepository,
            IExpenseRepository expenseRepository,
            IMapper mapper,
            OtasContext context)
        {
            _depenseCaisseRepository = depenseCaisseRepository;
            _actualRequesterRepository = actualRequesterRepository;
            _statusHistoryRepository = statusHistoryRepository;
            _expenseRepository = expenseRepository;
            _mapper = mapper;
            _context = context;
        }


        public async Task<ServiceResult> AddDepenseCaisse(DepenseCaissePostDTO depenseCaisse, string filePath)
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

                //MAP
                List<Expense> mappedExpenses = _mapper.Map<List<Expense>>(depenseCaisse.Expenses);

                DepenseCaisse mappedDepenseCaisse = _mapper.Map<DepenseCaisse>(depenseCaisse);




                //Manually map
                mappedDepenseCaisse.Total = CalculateExpensesEstimatedTotal(mappedExpenses);
                mappedDepenseCaisse.ReceiptsFilePath = filePath;

                result = await _depenseCaisseRepository.AddDepenseCaisseAsync(mappedDepenseCaisse);
                if(!result.Success) return result;

                // Status History
                StatusHistory DC_status = new()
                {
                    DepenseCaisseId = mappedDepenseCaisse.Id,
                };
                result = await _statusHistoryRepository.AddStatusAsync(DC_status);

                if (!result.Success)
                {
                    result.Message += " (AvanceCaisse)";
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
