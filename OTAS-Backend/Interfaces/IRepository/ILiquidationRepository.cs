using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface ILiquidationRepository
    {
        public Liquidation GetLiquidationById(int id);
        public ICollection<Liquidation> GetAllLiquidations();
        public ICollection<Liquidation> GetLiquidationsByAvanceVoyageId(int avId);
        public ICollection<Liquidation> GetLiquidationsByAvanceCaisseId(int acId);
    }
}
