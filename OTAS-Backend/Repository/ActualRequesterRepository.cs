using Microsoft.EntityFrameworkCore;
using OTAS.Data;
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


        public async Task<ServiceResult> AddActualRequesterInfoAsync(ActualRequester actualRequester)
        {
            ServiceResult result = new();
            await _context.AddAsync(actualRequester);
            result.Success = await SaveAsync();
            if (!result.Success) result.Message = "Something went wrong while Saving actual requester info";

            return result;
        }

        public async Task<ServiceResult> DeleteActualRequesterInfoAsync(ActualRequester actualRequester)
        {
            ServiceResult result = new();
            _context.Remove(actualRequester);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "ActualRequester's info deleted successfully" : "Something went wrong while deleting the actualrequester.";
            return result;

        }

        public async Task<ActualRequester?> FindActualrequesterInfoByAvanceCaisseIdAsync(int avanceCaisseId)
        {
            return await _context.ActualRequesters.Where(ar => ar.AvanceCaisseId == avanceCaisseId).FirstOrDefaultAsync();
        }

        public async Task<ActualRequester?> FindActualrequesterInfoByDepenseCaisseIdAsync(int depenseCaisseId)
        {
            return await _context.ActualRequesters.Where(ar => ar.DepenseCaisseId ==  depenseCaisseId).FirstOrDefaultAsync();
        }

        public async Task<ActualRequester?> FindActualrequesterInfoByOrdreMissionIdAsync(int ordreMissionId)
        {
            return await _context.ActualRequesters.Where(ar => ar.OrdreMissionId == ordreMissionId).FirstOrDefaultAsync();
        }

        public async Task<bool> SaveAsync()
        {
            int saved = await _context.SaveChangesAsync();
            return saved > 0;
        }


    }
}
