using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface IAvanceVoyageRepository
    {
        //Get Methods
        Task<AvanceVoyage> GetAvanceVoyageById(int id);
        Task<List<AvanceVoyage>> GetAvancesVoyageByRequesterUserId(int requesterUserId);
        Task<List<AvanceVoyage>> GetAvancesVoyageByStatus(int status);
        Task<List<AvanceVoyage>> GetAvancesVoyageByOrdreMissionId(int ordreMissionId);

        //Post Methods
        Task<bool> AddAvanceVoyage(AvanceVoyage voyage);
        Task<bool> Save();
    }
}
