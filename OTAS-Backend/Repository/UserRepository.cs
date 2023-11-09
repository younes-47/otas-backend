using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;

namespace OTAS.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly OtasContext _context;
        public UserRepository(OtasContext context)
        {
            _context = context;
        }



        public ICollection<User> GetAllUsers()
        {
            return _context.Users.OrderBy(user => user.Id).ToList();
        }


        public User GetUserByUserId(int userId)
        {
            return _context.Users.Where(user => user.Id == userId).FirstOrDefault();
        }

        public int GetUserRoleByUserId(int userId)
        {
            return _context.Users.Where(user => user.Id == userId).FirstOrDefault().Role;
        }

        public bool AddUser(User user)
        {
            _context.Add(user);
            return Save();
        }

        public bool Save()
        {
            int saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }

    }
}
