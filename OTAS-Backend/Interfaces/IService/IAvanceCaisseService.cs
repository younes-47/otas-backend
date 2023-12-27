using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.Services;

namespace OTAS.Interfaces.IService
{
    public interface IAvanceCaisseService
    {
        Task<ServiceResult> CreateAvanceCaisseAsync(AvanceCaissePostDTO avanceCaisse);
        Task<List<AvanceCaisseDTO>> GetAvanceCaissesForDeciderTable(int userId);
        Task<AvanceCaisseFullDetailsDTO> GetAvanceCaisseFullDetailsById(int avanceCaisseId);
        Task<ServiceResult> DecideOnAvanceCaisse(DecisionOnRequestPostDTO decision);
        Task<ServiceResult> SubmitAvanceCaisse(int ordreMissionId);
        Task<ServiceResult> ModifyAvanceCaisse(AvanceCaissePostDTO avanceCaisse, string action);
    }
}
