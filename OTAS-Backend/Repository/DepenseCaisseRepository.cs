using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;

namespace OTAS.Repository
{
    public class DepenseCaisseRepository : IDepenseCaisseRepository
    {
        //Inject datacontext in the constructor
        private readonly OtasContext _context;
        public DepenseCaisseRepository(OtasContext context)
        {
            _context = context;
        }


        public DepenseCaisse GetDepenseCaisseById(int id)
        {
            return _context.DepenseCaisses.Where(dc => dc.Id == id).FirstOrDefault();
        }

        public ICollection<DepenseCaisse> GetDepensesCaisseByRequesterUserId(int requesterUserId)
        {
            return _context.DepenseCaisses.Where(dc => dc.UserId == requesterUserId).ToList();
        }

        public ICollection<DepenseCaisse> GetDepensesCaisseByStatus(int status)
        {
            return _context.DepenseCaisses.Where(dc => dc.LatestStatus == status).ToList();
        }
    }
}
