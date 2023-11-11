using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface IExpenseRepository
    {
        ICollection<Expense> GetAvanceVoyageExpensesByAvId(int avId);
        ICollection<Expense> GetAvanceCaisseExpensesByAvId(int acId);
        ICollection<Expense> GetDepenseCaisseExpensesByAvId(int dcId);
        bool AddExpenses(ICollection<Expense> expenses);
        bool Save();
    }
}
