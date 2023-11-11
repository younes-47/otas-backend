using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;

namespace OTAS.Repository
{
    public class DelegationRepository : IDelegationRepository
    {
        private readonly OtasContext _Context;
        public DelegationRepository(OtasContext context)
        {
            _Context = context;
        }

        public Delegation GetDelegation(int deciderUserId)
        {
            return _Context.Delegations.Where(d => d.DeciderUserId == deciderUserId).FirstOrDefault();
        }

        public int[] GetDelegationPeriod(int deciderUserId)
        {
            int[] daysAndHours = new int[2];

            DateTime startDate = _Context.Delegations.Where(d => d.DeciderUserId == deciderUserId).FirstOrDefault().StartDate;
            DateTime endDate = _Context.Delegations.Where(d => d.DeciderUserId == deciderUserId).FirstOrDefault().EndDate;

            

            TimeSpan difference = endDate - startDate;
            daysAndHours[0] = difference.Days;
            daysAndHours[1] = difference.Hours;

            return daysAndHours;
        }

        public bool HaveDelegate(int deciderUserId)
        {
            return _Context.Delegations.Any(d => d.DeciderUserId == deciderUserId);
        }


    }
}
