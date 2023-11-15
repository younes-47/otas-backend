using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface IUserRepository
    {
        Task<ICollection<User>> GetAllUsersAsync();
        Task<int> GetUserRoleByUserIdAsync(int userId);
        Task<User> GetUserByUserIdAsync(int userId);
        Task<User?> FindUserByUserIdAsync(int userId);
        Task<ServiceResult> AddUserAsync(User user);
        Task<bool> SaveAsync();
    }
}
