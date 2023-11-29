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
        Task<ServiceResult> DecideOnAvanceCaisse(int avanceCaisseId, int deciderUserId, string? deciderComment, int decision);
        Task<ServiceResult> SubmitAvanceCaisse(int ordreMissionId);
        Task<ServiceResult> ModifyAvanceCaisse(AvanceCaissePostDTO avanceCaisse, int action);
    }
}
