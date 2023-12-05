using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
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

        public OrdreMissionRepository(OtasContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        public async Task<OrdreMissionFullDetailsDTO> GetOrdreMissionFullDetailsById(int ordreMissionId)
        {
            //var ordreMission = await _context.OrdreMissions
            //    .Where(om => om.Id == ordreMissionId)
            //    .Select(om => new OrdreMissionFullDetailsDTO
            //    {
            //        Id = om.Id,
            //        Description = om.Description,
            //        Abroad = om.Abroad,
            //        DepartureDate = om.DepartureDate,
            //        ReturnDate = om.ReturnDate,
            //        LatestStatus = om.LatestStatus,
            //        CreateDate = om.CreateDate,
            //        User = _mapper.Map<UserDTO>(om.User),
            //        ActualRequester = _mapper.Map<ActualRequesterDTO>(om.ActualRequester),
            //        AvanceVoyages = om.AvanceVoyages.Select(av => new AvanceVoyageDTO
            //        {
            //            Expenses = _mapper.Map<ICollection<ExpenseDTO>>(om.AvanceVoyages.)
            //        }).ToList(),
            //        StatusHistories = _mapper.Map<ICollection<StatusHistoryDTO>>(om.StatusHistories)
            //    })
            //    .FirstOrDefaultAsync();

            var ordreMission = await _context.OrdreMissions
                .Where(om => om.Id == ordreMissionId)
                .ProjectTo<OrdreMissionFullDetailsDTO>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync();

            //For some reason Automapper doesn't map the user object
            ordreMission.User = _mapper.Map<UserDTO>(await _context.Users.Where(user => user.Id == ordreMission.UserId).FirstAsync());

            return ordreMission;
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

        public async Task<ServiceResult> UpdateOrdreMissionStatusAsync(int ordreMissionId, int status)
        {
            ServiceResult result = new();

            OrdreMission updatedOrdreMission = await GetOrdreMissionByIdAsync(ordreMissionId);
            updatedOrdreMission.LatestStatus = status;
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


        public async Task<bool> SaveAsync()
        {
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

    }
}
