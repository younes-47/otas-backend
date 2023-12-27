using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using OTAS.Data;
using OTAS.Interfaces.IRepository;
using OTAS.Models;

namespace OTAS.Repository
{
   
    public class DeciderRepository : IDeciderRepository
    {
        private readonly OtasContext _context;
        private readonly IMapper _mapper;
        public DeciderRepository(OtasContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        public async Task<int> GetManagerUserIdByUserIdAsync(int userId)
        {
            // Get the user's superior
            var user =  await _context.Users.Where(u => u.Id == userId).FirstAsync();
            int superiorUserId = user.SuperiorUserId;

            // Check if the superior is actually a decider
            var decider = await FindDeciderByUserIdAsync(superiorUserId);
            // if the decider with the current value is not null then return its id
            if (decider != null)
            {
                return decider.UserId;
            }
            //otherwise see if the superior of the current superior is actually a decider (process repeats until you find a decider)
            return await GetManagerUserIdByUserIdAsync(superiorUserId);
        }

        public async Task<int> GetDeciderUserIdByDeciderLevel(string level)
        {
            var decider = await _context.Deciders.Where(d => d.Level == level).FirstAsync();
            return decider.UserId;
        }

        public async Task<string> GetDeciderLevelByUserId(int deciderUserId)
        {
            var decider = await _context.Deciders.Where(d => d.UserId == deciderUserId).FirstAsync();
            return decider.Level;
        }

        public async Task<Decider?> FindDeciderByUserIdAsync(int userId)
        {
            return await _context.Deciders.Where(d => d.UserId == userId).FirstOrDefaultAsync();
        }
    }
}
