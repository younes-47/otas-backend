using OTAS.DTO.Post;
using OTAS.Services;

namespace OTAS.Interfaces.IService
{
    public interface IDepenseCaisseService
    {
        Task<ServiceResult> AddDepenseCaisse(DepenseCaissePostDTO depenseCaisse, string filePath);
        Task<ServiceResult> ModifyDepenseCaisse(DepenseCaissePostDTO depenseCaisse, int action);
        Task<ServiceResult> SubmitDepenseCaisse(int depenseCaisseId);
    }
}
