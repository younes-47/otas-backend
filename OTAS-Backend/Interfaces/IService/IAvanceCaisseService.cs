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
        Task<ServiceResult> DecideOnAvanceCaisse(DecisionOnRequestPostDTO decision, int deciderUserId);
        Task<ServiceResult> MarkFundsAsPrepared(int avanceCaisseId, string advanceOption, int deciderUserId);
        Task<ServiceResult> ConfirmFundsDelivery(int avanceCaisseId, int confirmationNumber);
        Task<ServiceResult> SubmitAvanceCaisse(AvanceCaisse avanceCaisse);
        Task<ServiceResult> ModifyAvanceCaisse(AvanceCaissePutDTO avanceCaisse);
        Task<ServiceResult> DeleteDraftedAvanceCaisse(AvanceCaisse avanceCaisse);

        Task<string> GenerateAvanceCaisseWordDocument(int avanceCaisseId);
    }
}
