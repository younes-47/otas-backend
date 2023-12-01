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




        public async Task<User> GetUserByUserIdAsync(int userId)
        {
            return await _context.Users.Where(user => user.Id == userId).FirstAsync();
        }
        public async Task<User> GetUserByOrdreMissionId(int ordreMissionId)
        {
            OrdreMission om = await _context.OrdreMissions.Where(om => om.Id == ordreMissionId).FirstAsync();
            int userId = om.UserId;

            return await GetUserByUserIdAsync(userId);
        }

        public async Task<User?> FindUserByUserIdAsync(int userId)
        {
            return await _context.Users.Where(user => user.Id == userId).FirstOrDefaultAsync();
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

        public async Task<ServiceResult> AddUserAsync(User user)
        {
            ServiceResult result = new();
            await _context.AddAsync(user);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "User added Successfully." : "Something went wrong while adding the user";
            return result;
        }

        public async Task<bool> SaveAsync()
        {
            int saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

    }
}
