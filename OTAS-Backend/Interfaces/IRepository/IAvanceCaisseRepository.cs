using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface IAvanceCaisseRepository
    {
        // Show a specific "Avance de caisse" details (page)
        AvanceCaisse GetAvanceCaisseById(int id);
        // Show relevant "Avances de caisse" to a decider based on its current status
        ICollection<AvanceCaisse> GetAvancesCaisseByStatus(int status);
        // Show requested "Avances de caisse" for the relevant requester
        ICollection<AvanceCaisse> GetAvancesCaisseByRequesterUserId(int requesterUserId);
    }
}
