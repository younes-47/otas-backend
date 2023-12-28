using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;
using OTAS.Services;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using OTAS.DTO.Get;
using AutoMapper;

namespace OTAS.Repository
{
    public class AvanceCaisseRepository : IAvanceCaisseRepository
    {
        private readonly OtasContext _context;
        private readonly IDeciderRepository _deciderRepository;
        private readonly IMapper _mapper;

        public AvanceCaisseRepository(OtasContext context, IDeciderRepository deciderRepository, IMapper mapper)
        {
            _context = context;
            _deciderRepository = deciderRepository;
            _mapper = mapper;
        }

        public async Task<ServiceResult> AddAvanceCaisseAsync(AvanceCaisse avanceCaisse)
        {
            ServiceResult result = new();
            await _context.AddAsync(avanceCaisse);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "\"AvanceCaisse\" added successfully" : "Something went wrong while adding \"AvanceCaisse\"";
            return result;
        }

        public async Task<AvanceCaisse?> FindAvanceCaisseAsync(int avanceCaisseId)
        {
            return await _context.AvanceCaisses.FindAsync(avanceCaisseId);
        }

        public async Task<AvanceCaisse> GetAvanceCaisseByIdAsync(int avanceCaisseId)
        {
            return await _context.AvanceCaisses.Where(ac => ac.Id == avanceCaisseId).FirstAsync();
        }
        public async Task<List<AvanceCaisseDTO>> GetAvanceCaissesForDeciderTable(int deciderUserId)
        {
            return _mapper.Map<List<AvanceCaisseDTO>>(await _context.AvanceCaisses.Where(om => om.NextDeciderUserId == deciderUserId).ToListAsync());
        }

        public async Task<List<AvanceCaisse>> GetAvancesCaisseByStatusAsync(int status)
        {
            return await _context.AvanceCaisses.Where(ac => ac.LatestStatus == status).ToListAsync();
        }

        public async Task<List<AvanceCaisseDTO>> GetAvancesCaisseByUserIdAsync(int userId)
        {
            return await _context.AvanceCaisses.Where(ac => ac.UserId == userId)
                .Include(ac => ac.LatestStatusNavigation)
                .Select(ac => new AvanceCaisseDTO
                {
                    Id = ac.Id,
                    OnBehalf = ac.OnBehalf,
                    Description = ac.Description,
                    Currency = ac.Currency,
                    EstimatedTotal = ac.EstimatedTotal,
                    ActualTotal = ac.ActualTotal,
                    ConfirmationNumber = ac.ConfirmationNumber,
                    LatestStatus = ac.LatestStatusNavigation.StatusString,
                    CreateDate = ac.CreateDate,
                })
                .ToListAsync();
        }

        public async Task<bool> SaveAsync()
        {
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

        public async Task<ServiceResult> UpdateAvanceCaisseAsync(AvanceCaisse avanceCaisse)
        {
            ServiceResult result = new();
            _context.Update(avanceCaisse);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "\"AvanceCaisse\" updated successfully" : "Something went wrong while updating \"AvanceCaisse\"";
            return result;
        }

        public async Task<ServiceResult> UpdateAvanceCaisseStatusAsync(int avanceCaisseId, int status)
        {
            ServiceResult result = new();

            AvanceCaisse updatedAvanceCaisse = await GetAvanceCaisseByIdAsync(avanceCaisseId);
            updatedAvanceCaisse.LatestStatus = status;
            _context.AvanceCaisses.Update(updatedAvanceCaisse);

            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "\"AvanceCaisse\" Status Updated Successfully" : "Something went wrong while updating \"AvanceCaisse\" Status";

            return result;
        }

        public async Task<int> GetAvanceCaisseNextDeciderUserId(string currentlevel, bool? isReturnedToFMByTR = false, bool? isReturnedToTRbyFM = false)
        {
            int deciderUserId = 0;
            switch (currentlevel)
            {
                case "MG":
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("FM");
                    break;

                case "FM":
                    if (isReturnedToTRbyFM == true)
                    {
                        deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("TR"); /* in case FM returns it to TR */
                        break;
                    }
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("GD");
                    break;

                case "GD":
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("TR");
                    break;

                case "TR":
                    if (isReturnedToFMByTR == true)
                    {
                        deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("FM"); /* In case TR returns it to FM */
                        break;
                    }
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("TR"); /* In case TR aprroves it, the next decider is still TR*/
                    break;
            }

            return deciderUserId;
        }

        public async Task<ServiceResult> DeleteAvanceCaisseAync(AvanceCaisse avanceCaisse)
        {
            ServiceResult result = new();
            _context.Remove(avanceCaisse);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "AvanceCaisse has been deleted successfully" : "Something went wrong while deleting the AvanceCaisse.";
            return result;
        }

        public async Task<string?> DecodeStatusAsync(int statusCode)
        {
            var statusCodeEntity = await _context.StatusCodes.Where(sc => sc.StatusInt == statusCode).FirstOrDefaultAsync();

            return statusCodeEntity?.StatusString;
        }
    }
}
