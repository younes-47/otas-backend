using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface IStatusHistoryRepository
    {
        ICollection<StatusHistory> GetAvanceVoyageStatusHistory(int avId);
        ICollection<StatusHistory> GetAvanceCaisseStatusHistory(int acId);
        ICollection<StatusHistory> GetOrdreMissionStatusHistory(int omId);
        ICollection<StatusHistory> GetDepenseCaisseStatusHistory(int dcId);
        ICollection<StatusHistory> GetLiquidationStatusHistory(int lqId);

        Task<ServiceResult> AddStatusAsync(StatusHistory status);
        Task<bool> SaveAsync();

    }
}
