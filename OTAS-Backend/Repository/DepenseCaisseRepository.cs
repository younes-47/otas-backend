using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;
using Microsoft.EntityFrameworkCore;
using OTAS.Services;
using Azure.Core;
using OTAS.DTO.Get;
using AutoMapper;

namespace OTAS.Repository
{
    public class DepenseCaisseRepository : IDepenseCaisseRepository
    {
        //Inject datacontext in the constructor
        private readonly OtasContext _context;
        private readonly IMapper _mapper;
        private readonly IDeciderRepository _deciderRepository;

        public DepenseCaisseRepository(OtasContext context, IMapper mapper, IDeciderRepository deciderRepository)
        {
            _context = context;
            _mapper = mapper;
            _deciderRepository = deciderRepository;
        }

        public async Task<DepenseCaisse?> FindDepenseCaisseAsync(int depenseCaisseId)
        {
            return await _context.DepenseCaisses.FindAsync(depenseCaisseId);
        }

        public async Task<ServiceResult> AddDepenseCaisseAsync(DepenseCaisse depenseCaisse)
        {
            ServiceResult result = new();
            await _context.AddAsync(depenseCaisse);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "DepenseCaisse has been added successfully" : "Something went wrong while adding the DepenseCaisse";
            return result;
        }

        public async Task<ServiceResult> UpdateDepenseCaisseAsync(DepenseCaisse depenseCaisse)
        {
            ServiceResult result = new();
            _context.Update(depenseCaisse);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "DepenseCaisse has been updated successfully" : "Something went wrong while updating the DepenseCaisse";
            return result;
        }
        
        public async Task<ServiceResult> UpdateDepenseCaisseStatusAsync(int depesneCaisseId, int status)
        {
            ServiceResult result = new();

            DepenseCaisse updatedDepenseCaisse = await GetDepenseCaisseByIdAsync(depesneCaisseId);
            updatedDepenseCaisse.LatestStatus = status;
            _context.DepenseCaisses.Update(updatedDepenseCaisse);

            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "\"DepenseCaisse\" Status Updated Successfully" : "Something went wrong while updating \"DepenseCaisse\" Status";

            return result;
        }

        public async Task<ServiceResult> DeleteDepenseCaisseAync(DepenseCaisse depenseCaisse)
        {
            ServiceResult result = new();
            _context.Remove(depenseCaisse);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "DepenseCaisse has been deleted successfully" : "Something went wrong while deleting the DepenseCaisse.";
            return result;
        }

        public async Task<DepenseCaisse> GetDepenseCaisseByIdAsync(int depenseCaisseId)
        {
            return await _context.DepenseCaisses.Where(dc => dc.Id == depenseCaisseId).FirstAsync();
        }

        public async Task<List<DepenseCaisseDTO>> GetDepensesCaisseByUserIdAsync(int userId)
        {
            return await _context.DepenseCaisses.Where(dc => dc.UserId == userId)
                .Include(dc => dc.LatestStatusNavigation)
                .Select(dc => new DepenseCaisseDTO
                {
                    Id = dc.Id,
                    OnBehalf = dc.OnBehalf,
                    Description = dc.Description,
                    Currency = dc.Currency,
                    Total = dc.Total,
                    ReceiptsFileName = dc.ReceiptsFileName,
                    LatestStatus = dc.LatestStatusNavigation.StatusString,
                    CreateDate = dc.CreateDate,
                })
                .ToListAsync();
        }

        public async Task<List<DepenseCaisse>> GetDepensesCaisseByStatus(int status)
        {
            return await _context.DepenseCaisses.Where(dc => dc.LatestStatus == status).ToListAsync();
        }

        public async Task<List<DepenseCaisseDTO>> GetDepenseCaissesForDeciderTable(int deciderUserId)
        {
            /* Get the records that needs to be decided upon now */
            List<DepenseCaisseDTO> depenseCaisses = _mapper.Map<List<DepenseCaisseDTO>>
                (await _context.DepenseCaisses.Where(om => om.NextDeciderUserId == deciderUserId).ToListAsync());

            /* Get the records that have been already decided upon and add it to the previous list */
            depenseCaisses.AddRange(_mapper.Map<List<DepenseCaisseDTO>>
                (await _context.StatusHistories
                .Where(sh => sh.DeciderUserId == deciderUserId)
                .Select(sh => sh.DepenseCaisse)
                .ToListAsync()));

            return depenseCaisses;

        }

        public async Task<int> GetDepenseCaisseNextDeciderUserId(string currentlevel, bool? isReturnedToFMByTR = false, bool? isReturnedToTRbyFM = false)
        {
            int deciderUserId; 
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
                default:
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("TR");
                    break;
            }

            return deciderUserId;
        }

        public async Task<bool> SaveAsync()
        {
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}
