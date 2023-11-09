using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface IDelegationRepository
    {
        public bool haveDelegate(int deciderUserId);
        int[] GetDelegationPeriod(int deciderUserId);
        Delegation GetDelegation(int deciderUserId);

    }
}
