using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;
using Microsoft.EntityFrameworkCore;
using OTAS.Services;

namespace OTAS.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly OtasContext _context;
        public UserRepository(OtasContext context)
        {
            _context = context;
        }


        public async Task<User?> GetUserByHttpContextAsync(HttpContext httpContext)
        {
            var userIdentity = httpContext.User.Identity;
            if(userIdentity == null || userIdentity.Name == null)
            {
                return null;
            }
            User? user = await FindUserByUsernameAsync(userIdentity.Name);
            return user;
        }

        public async Task<User> GetUserByUserIdAsync(int userId)
        {
            return await _context.Users.Where(user => user.Id == userId).FirstAsync();
        }

        public async Task<User?> FindUserByUserIdAsync(int userId)
        {
            return await _context.Users.Where(user => user.Id == userId).FirstOrDefaultAsync();
        }
        public async Task<User?> FindUserByUsernameAsync(string username)
        {
            return await _context.Users.Where(user => user.Username == username).FirstOrDefaultAsync();
        }
        public async Task<int> GetUserRoleByUserIdAsync(int userId)
        {
            var user = await _context.Users.Where(user => user.Id == userId).FirstAsync();
            return user.Role;
        }

        public async Task<int> GetUserRoleByUsernameAsync(string username)
        {
            var user = await _context.Users.Where(user => user.Username == username).FirstAsync();
            return user.Role;
        }

        public async Task<int> GetUserIdByUsernameAsync(string username)
        {
            var user = await _context.Users.Where(user => user.Username == username).FirstAsync();
            return user.Id;
        }

        public async Task<string> GetUsernameByUserIdAsync(int userId)
        {
            var user = await _context.Users.Where(user => user.Id == userId).FirstAsync();
            return user.Username;
        }

        public async Task<ServiceResult> AddUserAsync(User user)
        {
            ServiceResult result = new();
            await _context.AddAsync(user);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "User added Successfully." : "Something went wrong while adding the user";
            return result;
        }

        public async Task<string> GetDeciderUsernameByDeciderLevel(string level)
        {
            int userId =  _context.Deciders.Where(dc => dc.Level == level).Select(dc => dc.UserId).FirstOrDefault();
            return await _context.Users.Where(user => user.Id == userId).Select(user => user.Username).FirstAsync();
        }

        public async Task<User> CreateUserAsync(User user)
        {
            await _context.AddAsync(user);
            await SaveAsync();
            return user;
        }

        public async Task<bool> SaveAsync()
        {
            int saved = await _context.SaveChangesAsync();
            return saved > 0;
        }


    }
}
