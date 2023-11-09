using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;

namespace OTAS.Repository
{
    public class AvanceCaisseRepository : IAvanceCaisseRepository
    {
        private readonly OtasContext _Context;
        public AvanceCaisseRepository(OtasContext context)
        {
            _Context = context;
        }


        public AvanceCaisse GetAvanceCaisseById(int id)
        {
            return _Context.AvanceCaisses.Where(av => av.Id == id).FirstOrDefault();
        }

        public ICollection<AvanceCaisse> GetAvancesCaisseByStatus(int status)
        {
            return _Context.AvanceCaisses.Where(av => av.LatestStatus == status).ToList();
        }

        public ICollection<AvanceCaisse> GetAvancesCaisseByRequesterUserId(int requesterUserId)
        {
            return _Context.AvanceCaisses.Where(om => om.UserId == requesterUserId).ToList();
        }

    }
}
