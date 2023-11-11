using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface IActualRequesterRepository
    {
        Task<bool> AddActualRequesterInfo(ActualRequester actualRequester);

        Task<bool> Save();

    }
}
