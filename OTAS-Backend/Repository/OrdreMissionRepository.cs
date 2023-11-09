using Microsoft.EntityFrameworkCore;
using OTAS.Data;
using OTAS.Interfaces.IRepository;
using OTAS.Models;

namespace OTAS.Repository
{
    public class OrdreMissionRepository : IOrdreMissionRepository
    {
        private readonly OtasContext _context;
        public OrdreMissionRepository(OtasContext context)
        {
            _context = context;
        }


        
        public ICollection<OrdreMission> GetOrdresMissionByUserId(int userid)
        {
            return _context.OrdreMissions.Where(om => om.UserId == userid).ToList();
        }

        public OrdreMission GetOrdreMissionById(int Id)
        {
            return _context.OrdreMissions.Where(om => om.Id == Id).FirstOrDefault();
        }

        public ICollection<OrdreMission> GetOrdresMissionByStatus(int status)
        {
            return _context.OrdreMissions.Where(om => om.LatestStatus == status).ToList();
        }

        public string DecodeStatus(int statusCode)
        {
            return _context.StatusCodes.Where(sc => sc.StatusInt == statusCode).FirstOrDefault().StatusString;
        }

        public bool AddOrdreMission(OrdreMission ordreMission)
        {
            _context.OrdreMissions.Add(ordreMission);
            return Save();
        }

        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
    }
}
