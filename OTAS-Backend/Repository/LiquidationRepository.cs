using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;

namespace OTAS.Repository
{
    public class LiquidationRepository : ILiquidationRepository
    {
        private readonly OtasContext _context;
        public LiquidationRepository(OtasContext context)
        {
            _context = context;
        }

        public ICollection<Liquidation> GetAllLiquidations()
        {
            return _context.Liquidations.ToList();
        }

        public Liquidation GetLiquidationById(int id)
        {
            return _context.Liquidations.Where(lq => lq.Id == id).FirstOrDefault();
        }

        public ICollection<Liquidation> GetLiquidationsByAvanceCaisseId(int acId)
        {
            return _context.Liquidations.Where(lq => lq.AvanceCaisseId == acId).ToList();
        }

        public ICollection<Liquidation> GetLiquidationsByAvanceVoyageId(int avId)
        {
            return _context.Liquidations.Where(lq => lq.AvanceVoyageId == avId).ToList();
        }

        public ICollection<Liquidation> GetLiquidationsByRequesterUsername(int requesterUserId)
        {
            return _context.Liquidations.Where(lq => lq.UserId == requesterUserId).ToList();
        }
    }
}
