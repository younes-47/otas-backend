using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface ILiquidationRepository
    {
        Liquidation GetLiquidationById(int id);
        ICollection<Liquidation> GetAllLiquidations();
        ICollection<Liquidation> GetLiquidationsByAvanceVoyageId(int avId);
        ICollection<Liquidation> GetLiquidationsByAvanceCaisseId(int acId);
    }
}
