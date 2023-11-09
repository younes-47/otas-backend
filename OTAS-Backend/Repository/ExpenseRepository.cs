using OTAS.Data;
using OTAS.Models;
using Microsoft.Identity.Client;
using OTAS.Interfaces.IRepository;

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

        public ICollection<Expense> GetAvanceCaisseExpensesByAvId(int acId)
        {
            return _context.Expenses.Where(expenses => expenses.AvanceCaisseId == acId).ToList();
        }

        public ICollection<Expense> GetAvanceVoyageExpensesByAvId(int avId)
        {
            return _context.Expenses.Where(expenses => expenses.AvanceVoyageId == avId).ToList();
        }

        public ICollection<Expense> GetDepenseCaisseExpensesByAvId(int dcId)
        {
            return _context.Expenses.Where(expenses => expenses.DepenseCaisseId == dcId).ToList();
        }

        //Post Methods
        public bool AddExpenses(ICollection<Expense> expenses)
        {
            _context.AddRange(expenses);
            return Save();
        }



        // Save context state
        public bool Save()
        {
            int saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
    }
}
