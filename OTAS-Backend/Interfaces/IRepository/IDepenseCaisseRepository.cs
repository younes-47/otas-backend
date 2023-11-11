using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface IDepenseCaisseRepository
    {
        DepenseCaisse GetDepenseCaisseById(int id);
        ICollection<DepenseCaisse> GetDepensesCaisseByStatus(int status);
        ICollection<DepenseCaisse> GetDepensesCaisseByRequesterUserId(int requesterUserId);
    }
}
