﻿using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using OTAS.Data;
using OTAS.DTO.Get;
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
            //var decider = await _context.Deciders.Where(d => d.Level == level).FirstAsync();
            List<Decider> gfd = await _context.Deciders.ToListAsync();
            try
            {
                var decider = await _context.Deciders.Where(d => d.Level == level).FirstAsync();
                return decider.UserId;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<List<string>?> GetDeciderLevelsByUserId(int deciderUserId)
        {
            return await _context.Deciders.Where(d => d.UserId == deciderUserId).Select(d => d.Level).ToListAsync();
        }


        public async Task<string?> GetDeciderLevelByUserId(int deciderUserId)
        {
            var decider = await _context.Deciders.Where(d => d.UserId == deciderUserId).FirstOrDefaultAsync();
            if (decider == null)
            {
                return null;
            }
            return decider.Level;
        }

        public async Task<Decider?> FindDeciderByUserIdAsync(int userId)
        {
            return await _context.Deciders.Where(d => d.UserId == userId).FirstOrDefaultAsync();
        }

        public async Task<List<ManagerInfoDTO>> GetManagersInfo()
        {
            return await _context.Deciders.Where(d => d.Level == "MG")
                .Include(d => d.User)
                .Select(d => new ManagerInfoDTO 
                { 
                    FirstName = d.User.FirstName,
                    LastName = d.User.LastName,
                    Username = d.User.Username,
                }).ToListAsync();
        }

        public async Task<UserDTO> GetDeciderInfoForEmailNotificationAsync(int deciderUserId)
        {
            UserDTO decider = await _context.Deciders
                .Where(d => d.UserId == deciderUserId)
                .Include(d => d.User)
                .Select(d => new UserDTO
                { 
                    FirstName = d.User.FirstName,
                    LastName = d.User.LastName,
                    PreferredLanguage = d.User.PreferredLanguage
                })
                .FirstAsync();
            return decider;
        }
    }
}
