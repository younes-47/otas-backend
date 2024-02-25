using OTAS.DTO.Post;
using OTAS.DTO.Put;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IService
{
    public interface IDepenseCaisseService
    {
        Task<ServiceResult> AddDepenseCaisse(DepenseCaissePostDTO depenseCaisse, int userId);
        Task<ServiceResult> ModifyDepenseCaisse(DepenseCaissePutDTO depenseCaisse);
        Task<ServiceResult> DeleteDraftedDepenseCaisse(DepenseCaisse depenseCaisse);
        Task<ServiceResult> SubmitDepenseCaisse(int depenseCaisseId);
        Task<ServiceResult> DecideOnDepenseCaisse(DecisionOnRequestPostDTO decision, int deciderUserId);
        Task<string> GenerateDepenseCaisseWordDocument(int depenseCaisseId);
    }
}
