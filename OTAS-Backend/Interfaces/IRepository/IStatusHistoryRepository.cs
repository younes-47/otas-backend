using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface IStatusHistoryRepository
    {
        public ICollection<StatusHistory> GetAvanceVoyageStatusHistory(int avId);
        public ICollection<StatusHistory> GetAvanceCaisseStatusHistory(int acId);
        public ICollection<StatusHistory> GetOrdreMissionStatusHistory(int omId);
        public ICollection<StatusHistory> GetDepenseCaisseStatusHistory(int dcId);
        public ICollection<StatusHistory> GetLiquidationStatusHistory(int lqId);

        public bool AddStatus(StatusHistory status);
        bool Save();

    }
}
