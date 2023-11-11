using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface IDelegationRepository
    {
        bool HaveDelegate(int deciderUserId);
        int[] GetDelegationPeriod(int deciderUserId);
        Delegation GetDelegation(int deciderUserId);

    }
}
