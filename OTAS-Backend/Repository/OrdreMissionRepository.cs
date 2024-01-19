using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using OTAS.Data;
using OTAS.DTO.Get;
using OTAS.Interfaces.IRepository;
using OTAS.Models;
using OTAS.Services;
using System.Collections.Generic;

namespace OTAS.Repository
{
    public class OrdreMissionRepository : IOrdreMissionRepository
    {
        private readonly OtasContext _context;
        private readonly IMapper _mapper;
        private readonly IDeciderRepository _deciderRepository;
        private readonly IUserRepository _userRepository;

        public OrdreMissionRepository(OtasContext context, IMapper mapper, IDeciderRepository deciderRepository, IUserRepository userRepository)
        {
            _context = context;
            _mapper = mapper;
            _deciderRepository = deciderRepository;
            _userRepository = userRepository;
        }

        public async Task<List<OrdreMissionDTO>> GetOrdreMissionsForDecider(int deciderUserId)
        {
            /* Get the records that needs to be decided upon now */
            List<OrdreMissionDTO> ordreMissions = _mapper.Map<List<OrdreMissionDTO>>
                (await _context.OrdreMissions.Where(om => om.NextDeciderUserId == deciderUserId).ToListAsync());

            /* Get the records that have been already decided upon and add it to the previous list */
            ordreMissions.AddRange(_mapper.Map<List<OrdreMissionDTO>>
                (await _context.StatusHistories
                .Where(sh => sh.DeciderUserId == deciderUserId)
                .Select(sh => sh.OrdreMission)
                .ToListAsync()));

            return ordreMissions;
        }

        public async Task<OrdreMissionViewDTO> GetOrdreMissionFullDetailsById(int ordreMissionId)
        {
            return await _context.OrdreMissions
                .Where(om => om.Id == ordreMissionId)
                .Include(om => om.LatestStatusString)
                .Include(om => om.StatusHistories)
                .Select(om => new OrdreMissionViewDTO
                {
                    Id = om.Id,
                    Description = om.Description,
                    Abroad = om.Abroad,
                    DepartureDate = om.DepartureDate,
                    ReturnDate = om.ReturnDate,
                    LatestStatus = om.LatestStatusString.StatusString,
                    CreateDate = om.CreateDate,
                    StatusHistory = om.StatusHistories.Select(sh => new StatusHistoryDTO
                    {
                        Status = sh.StatusNavigation.StatusString,
                        DeciderFirstName = sh.Decider != null ? sh.Decider.FirstName : null,
                        DeciderLastName = sh.Decider != null ? sh.Decider.LastName : null,
                        DeciderComment = sh.DeciderComment,
                        CreateDate = sh.CreateDate
                    }).ToList(),
                    AvanceVoyagesDetails = om.AvanceVoyages.Select(av => new OrdreMissionAvanceDetailsDTO
                    {
                        Id = av.Id,
                        EstimatedTotal = av.EstimatedTotal,
                        ActualTotal = av.ActualTotal,
                        Currency = av.Currency,
                        LatestStatus = av.LatestStatusNavigation.StatusString,
                        Trips = _mapper.Map<List<TripDTO>>(av.Trips),
                        Expenses = _mapper.Map<List<ExpenseDTO>>(av.Expenses),
                    }).ToList(),
                })
                .FirstAsync();

            //var ordreMission = await _context.OrdreMissions
            //    .Where(om => om.Id == ordreMissionId)
            //    .ProjectTo<OrdreMissionViewDTO>(_mapper.ConfigurationProvider)
            //    .FirstOrDefaultAsync();

            //return ordreMission;
        }

        public async Task<List<OrdreMissionDTO>?> GetOrdresMissionByUserIdAsync(int userid)
        {
            List<OrdreMissionDTO> ordreMissionDTO = await _context.OrdreMissions
                .Where(om => om.UserId == userid)
                .Include(om => om.LatestStatusString)
                .Select(OM => new OrdreMissionDTO
                {
                    Id = OM.Id,
                    OnBehalf = OM.OnBehalf,
                    Description = OM.Description,
                    DepartureDate = OM.DepartureDate,
                    ReturnDate = OM.ReturnDate,
                    LatestStatus = OM.LatestStatusString.StatusString,
                    CreateDate = OM.CreateDate,
                    Abroad = OM.Abroad,
                }).ToListAsync();
            return ordreMissionDTO;
        }

        public async Task<int> GetOrdreMissionNextDeciderUserId(string currentlevel, bool? isLongerThanOneDay = false)
        {
            int deciderUserId;
            switch(currentlevel)
            {
                case "MG":
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("HR"); 
                    break;

                case "HR":
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("FM");
                    break;

                case "FM":
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("GD");
                    break;

                case "GD":
                    if(isLongerThanOneDay == true)
                    {
                        deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("VP");
                        break;
                    }
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("GD");
                    break;

                case "VP":
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("VP");
                    break;

                default:
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("VP");
                    break;
            }

            return deciderUserId;
        }

        public async Task<OrdreMission> GetOrdreMissionByIdAsync(int ordreMissionId)
        {
            return await _context.OrdreMissions.Where(om => om.Id == ordreMissionId).FirstAsync();
        }

        public async Task<OrdreMission?> FindOrdreMissionByIdAsync(int ordreMissionId)
        {
            return await _context.OrdreMissions.FindAsync(ordreMissionId);
        }

        public async Task<List<OrdreMission>> GetOrdresMissionByStatusAsync(int status)
        {
            return await _context.OrdreMissions.Where(om => om.LatestStatus == status).ToListAsync();
        }

        public async Task<string?> DecodeStatusAsync(int statusCode)
        {
            var statusCodeEntity = await _context.StatusCodes.Where(sc => sc.StatusInt == statusCode).FirstOrDefaultAsync();

            return statusCodeEntity?.StatusString;
        }

        public async Task<ServiceResult> AddOrdreMissionAsync(OrdreMission ordreMission)
        {
            ServiceResult result = new();
            await _context.OrdreMissions.AddAsync(ordreMission);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "OrdreMission added Successfully!" : "Something went wrong while adding OrdreMission.";

            return result;
        }

        public async Task<ServiceResult> UpdateOrdreMissionStatusAsync(int ordreMissionId, int status, int nextDeciderUserId)
        {
            ServiceResult result = new();

            OrdreMission updatedOrdreMission = await GetOrdreMissionByIdAsync(ordreMissionId);
            updatedOrdreMission.LatestStatus = status;
            updatedOrdreMission.NextDeciderUserId = nextDeciderUserId;
            _context.OrdreMissions.Update(updatedOrdreMission);

            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "OrdreMission Status Updated Successfully!" : "Something went wrong while updating OrdreMission Status";

            return result;
        }

        public async Task<ServiceResult> UpdateOrdreMission(OrdreMission ordreMission)
        {
            ServiceResult result = new();
            _context.OrdreMissions.Update(ordreMission);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "OrdreMission Updated Successfully!" : "Something went wrong while updating OrdreMission";

            return result;
        }

        public async Task<ServiceResult> DeleteOrdreMissionAsync(OrdreMission ordreMission)
        {
            ServiceResult result = new();
            _context.Remove(ordreMission);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "OrdreMission has been deleted successfully" : "Something went wrong while deleting the OrdreMission.";
            return result;
        }

        public async Task<bool> SaveAsync()
        {
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

    }
}
