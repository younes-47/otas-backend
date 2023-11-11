using Microsoft.EntityFrameworkCore.Diagnostics;
using OTAS.Data;
using OTAS.DTO.Post;
using OTAS.Interfaces.IRepository;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Repository
{
    public class ActualRequesterRepository : IActualRequesterRepository
    {
        private readonly OtasContext _context;
        public ActualRequesterRepository(OtasContext context)
        {
            _context = context;
        }

        public async Task<bool> AddActualRequesterInfo(ActualRequester actualRequester)
        {
            await _context.ActualRequesters.AddAsync(actualRequester);
            return await Save();
        }

        public async Task<bool> Save()
        {
            int saved = await _context.SaveChangesAsync();
            return saved > 0;
        }


    }
}
