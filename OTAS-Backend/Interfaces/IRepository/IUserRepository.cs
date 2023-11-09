using OTAS.Models;

namespace OTAS.Interfaces.IRepository
{
    public interface IUserRepository
    {
        public ICollection<User> GetAllUsers();
        public int GetUserRoleByUserId(int userId);
        public User GetUserByUserId(int userId);
        bool AddUser(User user);
        bool Save();
    }
}
