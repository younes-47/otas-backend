using OTAS.Data;
using OTAS.Models;
using Microsoft.EntityFrameworkCore;
using OTAS.Interfaces.IRepository;
using OTAS.Services;
using OTAS.DTO.Get;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OTAS.Interfaces.IService;

namespace OTAS.Repository
{
    public class AvanceVoyageRepository : IAvanceVoyageRepository
    {
        private readonly OtasContext _context;
        private readonly IDeciderRepository _deciderRepository;
        private readonly IMiscService _misc;
        private readonly IMapper _mapper;

        public AvanceVoyageRepository(OtasContext context, IDeciderRepository deciderRepository, IMiscService miscService, IMapper mapper)
        {
            _context = context;
            _deciderRepository = deciderRepository;
            _misc = miscService;
            _mapper = mapper;
        }


        public async Task<int> GetAvanceVoyageNextDeciderUserId(string currentlevel, bool? isLongerThanOneDay = false)
        {
            int deciderUserId;
            switch (currentlevel)
            {
                case "MG":
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("FM");
                    break;

                case "FM":
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("GD");
                    break;

                case "GD":
                    if (isLongerThanOneDay == true)
                    {
                        deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("VP");
                        break;
                    }
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("TR");
                    break;

                case "VP":
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("TR");
                    break;

                case "TR":
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("TR");
                    break;

                default:
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("TR");
                    break;
            }

            return deciderUserId;
        }
   
        public async Task<List<AvanceVoyageTableDTO>> GetAvanceVoyagesForDeciderTable(int deciderUserId)
        {
            /* Get the records that needs to be decided upon now */
            List<AvanceVoyageTableDTO> avanceVoyages = _mapper.Map<List<AvanceVoyageTableDTO>>
                (await _context.AvanceVoyages.Where(om => om.NextDeciderUserId == deciderUserId).ToListAsync());

            /* Get the records that have been already decided upon and add it to the previous list */
            avanceVoyages.AddRange(_mapper.Map<List<AvanceVoyageTableDTO>>
                (await _context.StatusHistories
                .Where(sh => sh.DeciderUserId == deciderUserId)
                .Select(sh => sh.AvanceVoyage)
                .ToListAsync()));

            return avanceVoyages;
        }

        public async Task<List<AvanceVoyage>> GetAvancesVoyageByStatusAsync(int status)
        {
            return await _context.AvanceVoyages.Where(av => av.LatestStatus == status).ToListAsync();
        }

        public async Task<AvanceVoyage> GetAvanceVoyageByIdAsync(int avanceVoyageId)
        {
            return await _context.AvanceVoyages.Where(av => av.Id == avanceVoyageId).FirstAsync();
        }

        public async Task<AvanceVoyage?> FindAvanceVoyageByIdAsync(int avanceVoyageId)
        {
            return await _context.AvanceVoyages.FindAsync(avanceVoyageId);
        }
        public async Task<List<AvanceVoyage>> GetAvancesVoyageByOrdreMissionIdAsync(int ordreMissionId)
        {
            return await _context.AvanceVoyages.Where(av => av.OrdreMissionId == ordreMissionId).ToListAsync();
        }

        public async Task<List<AvanceVoyageTableDTO>> GetAvancesVoyageByUserIdAsync(int userId)
        {
            return await _context.AvanceVoyages.Where(av => av.UserId == userId)
                .Include(av => av.LatestStatusNavigation)
                .Include(av => av.OrdreMission)
                .Select(av => new AvanceVoyageTableDTO
                {
                    Id = av.Id,
                    EstimatedTotal = av.EstimatedTotal,
                    OnBehalf = av.OnBehalf,
                    OrdreMissionId = av.OrdreMission.Id,
                    OrdreMissionDescription = av.OrdreMission.Description,
                    ActualTotal = av.ActualTotal,
                    Currency = av.Currency,
                    ConfirmationNumber = av.ConfirmationNumber,
                    LatestStatus = av.LatestStatusNavigation.StatusString,
                    CreateDate = av.CreateDate,
                })
                .ToListAsync();
        }

        public async Task<AvanceVoyageViewDTO> GetAvancesVoyageViewDetailsByIdAsync(int id)
        {
            return await _context.AvanceVoyages.Where(av => av.Id == id)
                .Include(av => av.LatestStatusNavigation)
                .Include(av => av.OrdreMission)
                .Include(av => av.StatusHistories)
                .Select(av => new AvanceVoyageViewDTO
                {
                    Id = av.Id,
                    EstimatedTotal = av.EstimatedTotal,
                    OrdreMissionId = av.OrdreMission.Id,
                    OrdreMissionDescription = av.OrdreMission.Description,
                    OnBehalf = av.OnBehalf,
                    ActualTotal = av.ActualTotal,
                    Currency = av.Currency,
                    LatestStatus = av.LatestStatusNavigation.StatusString,
                    StatusHistory = av.StatusHistories.Select(sh => new StatusHistoryDTO
                    {
                        Status = sh.StatusNavigation.StatusString,
                        DeciderFirstName = sh.Decider != null ? sh.Decider.FirstName : null,
                        DeciderLastName = sh.Decider != null ?  sh.Decider.LastName : null,
                        DeciderComment = sh.DeciderComment,
                        CreateDate = sh.CreateDate
                    }).ToList()
                })
                .FirstAsync();
        }

        public async Task<ServiceResult> AddAvanceVoyageAsync(AvanceVoyage avanceVoyage)
        {
            ServiceResult result = new();
            await _context.AddAsync(avanceVoyage);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "AvanceVoyage added Successfully" : "Something went wrong while adding AvanceVoyage";
            return result;
        }

        public async Task<ServiceResult> UpdateAvanceVoyageAsync(AvanceVoyage avanceVoyage)
        {
            ServiceResult result = new();
            _context.Update(avanceVoyage);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "AvanceVoyage updated successfully" : "Something went wrong while updating AvanceVoyage";
            return result;
        }

        public async Task <ServiceResult> UpdateAvanceVoyageStatusAsync(int avanceVoyageId, int status, int nextDeciderUserId)
        {
            ServiceResult result = new();

            AvanceVoyage updatedAvanceVoyage  = await GetAvanceVoyageByIdAsync(avanceVoyageId);
            updatedAvanceVoyage.LatestStatus = status;
            updatedAvanceVoyage.NextDeciderUserId = nextDeciderUserId;
            _context.AvanceVoyages.Update(updatedAvanceVoyage);

            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "AvanceVoyage Status Updated Successfully" : "Something went wrong while updating AvanceVoyage Status";

           return result;

        }

        public async Task<ServiceResult> DeleteAvanceVoyagesRangeAsync(List<AvanceVoyage> avanceVoyages)
        {
            ServiceResult result = new();
            _context.RemoveRange(avanceVoyages);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "AvanceVoyages has been deleted successfully" : "Something went wrong while deleting the AvanceVoyages.";
            return result;
        }

        public async Task<bool> SaveAsync()
        {
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }
    }
}
