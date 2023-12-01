using OTAS.Models;
using OTAS.Services;
using System.Runtime.CompilerServices;

namespace OTAS.Interfaces.IRepository
{
    public interface IUserRepository
    {
        Task<int> GetUserRoleByUserIdAsync(int userId);
        Task<int> GetUserRoleByUsernameAsync(string username);
        Task<User> GetUserByUserIdAsync(int userId);
        Task<User> GetUserByOrdreMissionId(int ordreMissionId);
        Task<User?> FindUserByUserIdAsync(int userId);
        Task<ServiceResult> AddUserAsync(User user);
        Task<bool> SaveAsync();
    }
}
