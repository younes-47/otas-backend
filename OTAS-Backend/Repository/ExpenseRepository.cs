using OTAS.Data;
using OTAS.Models;
using Microsoft.Identity.Client;
using OTAS.Interfaces.IRepository;
using Microsoft.EntityFrameworkCore;
using OTAS.Services;

namespace OTAS.Repository
{
    public class ExpenseRepository : IExpenseRepository
    {
        private readonly OtasContext _context;
        public ExpenseRepository(OtasContext context)
        {
            _context = context;
        }

        //Get Methods

        public async Task<List<Expense>> GetAvanceCaisseExpensesByAcIdAsync(int acId)
        {
            return await _context.Expenses.Where(expenses => expenses.AvanceCaisseId == acId).ToListAsync();
        }

        public async Task<List<Expense>> GetAvanceVoyageExpensesByAvIdAsync(int avId)
        {
            return await _context.Expenses.Where(expenses => expenses.AvanceVoyageId == avId).ToListAsync();
        }

        public async Task<List<Expense>> GetDepenseCaisseExpensesByDcIdAsync(int dcId)
        {
            return await _context.Expenses.Where(expenses => expenses.DepenseCaisseId == dcId).ToListAsync();
        }

        public async Task<Expense?> FindExpenseAsync(int expenseId)
        {
            return await _context.Expenses.FindAsync(expenseId);
        }

        public async Task<Expense> GetExpenseAsync(int expenseId)
        {
            return await _context.Expenses.Where(expense => expense.Id == expenseId).FirstAsync();
        }

        //Post Methods
        public async Task<ServiceResult> AddExpensesAsync(List<Expense> expenses)
        {
            ServiceResult result = new();
            await _context.AddRangeAsync(expenses);
            result.Success= await SaveAsync();
            result.Message = result.Success == true ? "Expenses added successfully" : "Something went wrong while saving the expenses.";
            return result;
        }

        public async Task<ServiceResult> AddExpenseAsync(Expense expense)
        {
            ServiceResult result = new();
            await _context.AddAsync(expense);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "Expense added successfully" : "Something went wrong while saving an expense.";
            return result;
        }

        public async Task<ServiceResult> UpdateExpense(Expense expense)
        {
            ServiceResult result = new();
            _context.Update(expense);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "Expense updated successfully" : "Something went wrong while updating an expense.";
            return result;
        }

        public async Task<ServiceResult> DeleteExpenses(List<Expense> expenses)
        {
            ServiceResult result = new();
            _context.RemoveRange(expenses);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "Expenses deleted successfully" : "Something went wrong while deleting the expenses.";
            return result;
        }

        public async Task<ServiceResult> DeleteExpense(Expense expense)
        {
            ServiceResult result = new();
            _context.Remove(expense);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "Expense deleted successfully" : "Something went wrong while deleting the expense.";
            return result;
        }

        // Save context state
        public async Task<bool> SaveAsync()
        {
            int saved = await _context.SaveChangesAsync();
            return saved > 0;
        }
    }
}
