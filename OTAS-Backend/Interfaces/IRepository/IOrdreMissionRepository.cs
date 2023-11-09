using OTAS.Models;


namespace OTAS.Interfaces.IRepository
{
    public interface IOrdreMissionRepository
    {
        OrdreMission GetOrdreMissionById(int id);
        ICollection<OrdreMission> GetOrdresMissionByUserId(int userid);
        ICollection<OrdreMission> GetOrdresMissionByStatus(int status);
        string DecodeStatus(int statusCode);
        bool AddOrdreMission(OrdreMission ordreMission);
        bool Save();
    }
}
