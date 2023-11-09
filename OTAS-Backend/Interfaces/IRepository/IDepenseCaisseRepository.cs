using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface IDepenseCaisseRepository
    {
        public DepenseCaisse GetDepenseCaisseById(int id);
        public ICollection<DepenseCaisse> GetDepensesCaisseByStatus(int status);
        public ICollection<DepenseCaisse> GetDepensesCaisseByRequesterUserId(int requesterUserId);
    }
}
