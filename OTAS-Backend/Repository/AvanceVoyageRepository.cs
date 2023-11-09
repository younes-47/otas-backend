using OTAS.Data;
using OTAS.Models;
using Microsoft.EntityFrameworkCore;
using OTAS.Interfaces.IRepository;

namespace OTAS.Repository
{
    public class AvanceVoyageRepository : IAvanceVoyageRepository
    {
        private readonly OtasContext _Context;
        public AvanceVoyageRepository(OtasContext context)
        {
            _Context = context;
        }

        

        public ICollection<AvanceVoyage> GetAvancesVoyageByStatus(int status)
        {
            return _Context.AvanceVoyages.Where(av => av.LatestStatus == status).ToList();
        }

        public AvanceVoyage GetAvanceVoyageById(int id)
        {
            return _Context.AvanceVoyages.Where(av => av.Id == id).FirstOrDefault();
        }
        public ICollection<AvanceVoyage> GetAvancesVoyageByOrdreMissionId(int ordreMissionId)
        {
            return _Context.AvanceVoyages.Where(av => av.OrdreMissionId == ordreMissionId).ToList();
        }

        /* ??????????????????????????*/
        public ICollection<AvanceVoyage> GetAvancesVoyageByRequesterUsername(string requesterUsername)
        {
            throw new NotImplementedException();
        }

        public bool AddAvanceVoyage(AvanceVoyage voyage)
        {
            _Context.Add(voyage);
            return Save();
        }

        public bool Save()
        {
            var saved = _Context.SaveChanges();
            return saved > 0 ? true : false;
        }
    }
}
