﻿using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface IExpenseRepository
    {
        Task<List<Expense>> GetAvanceCaisseExpensesByAcIdAsync(int acId);
        Task<List<Expense>> GetAvanceVoyageExpensesByAvIdAsync(int avId);
        Task<List<Expense>> GetDepenseCaisseExpensesByDcIdAsync(int dcId);
        Task<Expense?> FindExpenseAsync(int expenseId);
        Task<Expense> GetExpenseAsync(int expenseId);
        Task<ServiceResult> AddExpenseAsync(Expense expense);
        Task<ServiceResult> AddExpensesAsync(List<Expense> expenses);
        Task<ServiceResult> UpdateExpense(Expense expense);
        Task<ServiceResult> DeleteExpenses(List<Expense> expenses);
        Task<ServiceResult> DeleteExpense(Expense expense);
        Task<bool> SaveAsync();

    }
}
