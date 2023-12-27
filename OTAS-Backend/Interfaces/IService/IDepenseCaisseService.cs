using OTAS.DTO.Post;
using OTAS.Services;

namespace OTAS.Interfaces.IService
{
    public interface IDepenseCaisseService
    {
        Task<ServiceResult> AddDepenseCaisse(DepenseCaissePostDTO depenseCaisse, int userId);
        Task<ServiceResult> ModifyDepenseCaisse(DepenseCaissePostDTO depenseCaisse, string action);
        Task<ServiceResult> SubmitDepenseCaisse(int depenseCaisseId);
    }
}
