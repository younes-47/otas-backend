using OTAS.DTO.Get;
using OTAS.DTO.Post;
using OTAS.DTO.Put;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IService
{
    public interface IAvanceCaisseService
    {
        Task<ServiceResult> CreateAvanceCaisseAsync(AvanceCaissePostDTO avanceCaisse, int userId);
        Task<AvanceCaisseFullDetailsDTO> GetAvanceCaisseFullDetailsById(int avanceCaisseId);
        Task<ServiceResult> DecideOnAvanceCaisse(DecisionOnRequestPostDTO decision);
        Task<ServiceResult> SubmitAvanceCaisse(AvanceCaisse avanceCaisse);
        Task<ServiceResult> ModifyAvanceCaisse(AvanceCaissePutDTO avanceCaisse);
        Task<ServiceResult> DeleteDraftedAvanceCaisse(AvanceCaisse avanceCaisse);
    }
}
