using OTAS.Models;

namespace OTAS.Interfaces.IService
{
    public interface IAuthService
    {
        Task<Tokens?> AuthenticateAsync(AuthRequest AuthRequest);
    }
}
