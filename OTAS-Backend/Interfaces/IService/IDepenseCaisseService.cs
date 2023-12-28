using OTAS.DTO.Post;
using OTAS.DTO.Put;
using OTAS.Services;

namespace OTAS.Interfaces.IService
{
    public interface IDepenseCaisseService
    {
        Task<ServiceResult> AddDepenseCaisse(DepenseCaissePostDTO depenseCaisse, int userId);
        Task<ServiceResult> ModifyDepenseCaisse(DepenseCaissePutDTO depenseCaisse);
        Task<ServiceResult> SubmitDepenseCaisse(int depenseCaisseId);
    }
}
