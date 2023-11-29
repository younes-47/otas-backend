using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface IActualRequesterRepository
    {
        Task<ActualRequester?> FindActualrequesterInfoByAvanceCaisseIdAsync(int avanceCaisseId);
        Task<ActualRequester?> FindActualrequesterInfoByOrdreMissionIdAsync(int ordreMissionId);
        Task<ActualRequester?> FindActualrequesterInfoByDepenseCaisseIdAsync(int depenseCaisseId);
        Task<ServiceResult> AddActualRequesterInfoAsync(ActualRequester actualRequester);
        Task<ServiceResult> DeleteActualRequesterInfoAsync(ActualRequester actualRequester);
        Task<bool> SaveAsync();
    }
}
