using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface IExpenseRepository
    {
        public ICollection<Expense> GetAvanceVoyageExpensesByAvId(int avId);
        public ICollection<Expense> GetAvanceCaisseExpensesByAvId(int acId);
        public ICollection<Expense> GetDepenseCaisseExpensesByAvId(int dcId);
        bool AddExpenses(ICollection<Expense> expenses);
        bool Save();
    }
}
