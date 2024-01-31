using OTAS.Models;
using OTAS.Services;
using System.Runtime.CompilerServices;

namespace OTAS.Interfaces.IRepository
{
    public interface IUserRepository
    {
        Task<int> GetUserRoleByUserIdAsync(int userId);
        Task<User?> GetUserByHttpContextAsync(HttpContext httpContext);
        Task<int> GetUserRoleByUsernameAsync(string username);
        Task<int> GetUserIdByUsernameAsync(string username);
        Task<string> GetPreferredLanguageByUserIdAsync(int userId);
        Task<User> GetUserByUserIdAsync(int userId);
        Task<string> GetUsernameByUserIdAsync(int userId);
        Task<string> GetDeciderUsernameByDeciderLevel(string level);
        Task<User?> FindUserByUserIdAsync(int userId);
        Task<User?> FindUserByUsernameAsync(string username);
        Task<ServiceResult> AddUserAsync(User user);
        Task<User> CreateUserAsync(User user);
        Task<bool> SaveAsync();
    }
}
