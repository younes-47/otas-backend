using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface IAvanceVoyageRepository
    {
        //Get Methods
        public AvanceVoyage GetAvanceVoyageById(int id);
        public ICollection<AvanceVoyage> GetAvancesVoyageByRequesterUsername(string requesterUsername);
        public ICollection<AvanceVoyage> GetAvancesVoyageByStatus(int status);
        public ICollection<AvanceVoyage> GetAvancesVoyageByOrdreMissionId(int ordreMissionId);

        //Post Methods
        bool AddAvanceVoyage(AvanceVoyage voyage);
        bool Save();
    }
}
