using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface IUserRepository
    {
        ICollection<User> GetAllUsers();
        int GetUserRoleByUserId(int userId);
        User GetUserByUserId(int userId);
        bool AddUser(User user);
        bool Save();
    }
}
