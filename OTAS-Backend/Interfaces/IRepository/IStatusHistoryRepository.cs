using Microsoft.EntityFrameworkCore;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface IStatusHistoryRepository
    {
        Task<List<StatusHistory>> GetAvanceCaisseStatusHistory(int acId);

        Task<List<StatusHistory>> GetAvanceVoyageStatusHistory(int avId);

        Task<List<StatusHistory>> GetDepenseCaisseStatusHistory(int dcId);

        Task<List<StatusHistory>> GetLiquidationStatusHistory(int lqId);

        Task<List<StatusHistory>> GetOrdreMissionStatusHistory(int omId);

        Task<StatusHistory> GetLatestAvanceCaisseStatusHistroyById(int acId);

        Task<StatusHistory> GetLatestAvanceVoyageStatusHistroyById(int avId);

        Task<StatusHistory> GetLatestDepenseCaisseStatusHistroyById(int dcId);

        Task<StatusHistory> GetLatestLiquidationStatusHistroyById(int lqId);

        Task<ServiceResult> UpdateStatusHistoryTotal(int requestId, string requestType, decimal total);

        Task<ServiceResult> DeleteStatusHistories(List<StatusHistory> statusHistories);

        Task<ServiceResult> AddStatusAsync(StatusHistory status);
        Task<bool> SaveAsync();

    }
}
