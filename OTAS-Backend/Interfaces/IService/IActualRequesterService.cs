using OTAS.DTO.Post;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IService
{
    public interface IActualRequesterService
    {
        Task<ServiceResult> AddActualRequesterInfoAsync(ActualRequester actualRequester);
    }
}
