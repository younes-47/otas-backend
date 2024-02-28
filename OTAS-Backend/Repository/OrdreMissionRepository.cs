using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using OTAS.Data;
using OTAS.DTO.Get;
using OTAS.Interfaces.IRepository;
using OTAS.Interfaces.IService;
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
        private readonly IAvanceVoyageRepository _avanceVoyageRepository;
        private readonly IMiscService _miscService;
        private readonly IUserRepository _userRepository;

        public OrdreMissionRepository(OtasContext context, IMapper mapper, IDeciderRepository deciderRepository, 
            IUserRepository userRepository, 
            IAvanceVoyageRepository avanceVoyageRepository,
            IMiscService miscService)
        {
            _context = context;
            _mapper = mapper;
            _deciderRepository = deciderRepository;
            _avanceVoyageRepository = avanceVoyageRepository;
            _miscService = miscService;
            _userRepository = userRepository;
        }

        public async Task<List<OrdreMissionDeciderTableDTO>> GetOrdreMissionsForDecider(int deciderUserId)
        {
            /* Get the records that needs to be decided upon now */
            //List<OrdreMissionDTO> ordreMissions = _mapper.Map<List<OrdreMissionDTO>>
            //    (await _context.OrdreMissions.Where(om => om.NextDeciderUserId == deciderUserId).Include(om => om.NextDeciderUser).Select().ToListAsync());

            bool isDeciderHR = await _context.Deciders.Where(dc => dc.UserId == deciderUserId).Select(dc => "HR" == dc.Level).FirstAsync();

            List<OrdreMissionDeciderTableDTO> ordreMissions = await _context.OrdreMissions
                .Where(om => om.NextDeciderUserId == deciderUserId)
                .Include(om => om.AvanceVoyages)
                .Where(om => (om.AvanceVoyages.Where(av => av.LatestStatus == om.LatestStatus).Count() == om.AvanceVoyages.Count()) || isDeciderHR)
                .Include(om => om.NextDecider)
                .Include(om => om.LatestStatusString)
                .Select(om => new OrdreMissionDeciderTableDTO
                {
                    Id = om.Id,
                    Description = om.Description,
                    Abroad = om.Abroad,
                    OnBehalf = om.OnBehalf,
                    IsDecidable = _miscService.IsRequestDecidable(deciderUserId, om.NextDeciderUserId, om.LatestStatusString.StatusString),
                    DepartureDate = om.DepartureDate,
                    ReturnDate = om.ReturnDate,
                    CreateDate = om.CreateDate,
                    RequestedAmountMAD = om.AvanceVoyages.Where(av => av.Currency == "MAD").Select(av => av.EstimatedTotal).FirstOrDefault(),
                    RequestedAmountEUR = om.AvanceVoyages.Where(av => av.Currency == "EUR").Select(av => av.EstimatedTotal).FirstOrDefault(),
                })
                .ToListAsync();

            /* Get the records that have been already decided upon and add it to the previous list */
            if (_context.StatusHistories.Where(sh => sh.OrdreMissionId != null && sh.DeciderUserId == deciderUserId).Any())
            {
                List<OrdreMissionDeciderTableDTO> ordreMissions2 = await _context.StatusHistories
                   .Where(sh => sh.DeciderUserId == deciderUserId && sh.OrdreMissionId != null)
                   .Include(sh => sh.OrdreMission)
                   .Include(sh => sh.StatusNavigation)
                   .Select(sh => new OrdreMissionDeciderTableDTO
                   {
                       Id = sh.OrdreMission.Id,
                       Description = sh.OrdreMission.Description,
                       Abroad = sh.OrdreMission.Abroad,
                       OnBehalf = sh.OrdreMission.OnBehalf,
                       IsDecidable = false,
                       DepartureDate = sh.OrdreMission.DepartureDate,
                       ReturnDate = sh.OrdreMission.ReturnDate,
                       CreateDate = sh.OrdreMission.CreateDate,
                       RequestedAmountMAD = sh.OrdreMission.AvanceVoyages.Where(av => av.Currency == "MAD").Select(av => av.EstimatedTotal).FirstOrDefault(),
                       RequestedAmountEUR = sh.OrdreMission.AvanceVoyages.Where(av => av.Currency == "EUR").Select(av => av.EstimatedTotal).FirstOrDefault(),
                   })
                   .ToListAsync();

                ordreMissions.AddRange(ordreMissions2);
            }


            return ordreMissions.Distinct(new OrdreMissionDeciderTableDTO()).ToList();
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
                    DeciderComment = om.DeciderComment,
                    Abroad = om.Abroad,
                    OnBehalf = om.OnBehalf,
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
                        Total = sh.Total,
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
        }

        public async Task<OrdreMissionDeciderViewDTO> GetOrdreMissionFullDetailsByIdForDecider(int ordreMissionId)
        {
            return await _context.OrdreMissions
                .Where(om => om.Id == ordreMissionId)
                .Include(om => om.LatestStatusString)
                .Include(om => om.StatusHistories)
                .Select(om => new OrdreMissionDeciderViewDTO
                {
                    Id = om.Id,
                    UserId = om.UserId,
                    Description = om.Description,
                    Abroad = om.Abroad,
                    OnBehalf = om.OnBehalf,
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
                        Total = sh.Total,
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
        }

        public async Task<OrdreMissionDocumentDetailsDTO> GetOrdreMissionDocumentDetailsByIdAsync(int ordreMissionId)
        {
            int satatusHistoryBreakRowId = _context.StatusHistories
                                                .Where(lq => lq.Status == 1 && lq.OrdreMissionId == ordreMissionId)
                                                .OrderByDescending(lq => lq.Id)
                                                .Select(lq => lq.Id)
                                                .First();

            return await _context.OrdreMissions.Where(ac => ac.Id == ordreMissionId)
                        .Include(ac => ac.AvanceVoyages)
                        .Include(ac => ac.StatusHistories)
                        .Include(ac => ac.User)
                        .Select(ac => new OrdreMissionDocumentDetailsDTO
                        {
                            Id = ac.Id,
                            FirstName = ac.User.FirstName,
                            LastName = ac.User.LastName,
                            Description = ac.Description,
                            SubmitDate = _context.StatusHistories
                                        .Where(lq => lq.OrdreMissionId == ordreMissionId && lq.Status == 1)
                                        .OrderByDescending(lq => lq.CreateDate)
                                        .Select(lq => lq.CreateDate)
                                        .First(),
                            AvanceVoyagesDetails = ac.AvanceVoyages.Select(av => new OrdreMissionAvanceDetailsDTO
                            {
                                Id = av.Id,
                                EstimatedTotal = av.EstimatedTotal,
                                ActualTotal = av.ActualTotal,
                                Currency = av.Currency,
                                LatestStatus = av.LatestStatusNavigation.StatusString,
                                Trips = _mapper.Map<List<TripDTO>>(av.Trips),
                                Expenses = _mapper.Map<List<ExpenseDTO>>(av.Expenses),
                            }).ToList(),
                            Signers = ac.StatusHistories
                                        .Where(lq => lq.OrdreMissionId == ordreMissionId)
                                        .Where(lq => lq.DeciderUserId != null)
                                        .Where(lq => lq.Id > satatusHistoryBreakRowId)
                                        .Select(sh => new Signatory
                                        {
                                            FirstName = sh.Decider.FirstName,
                                            LastName = sh.Decider.LastName,
                                            SignatureImageName = _context.Deciders.Where(d => d.UserId == sh.Decider.Id).Select(d => d.SignatureImageName).FirstOrDefault(),
                                            Level = _miscService.GetDeciderLevelByStatus(sh.Status, true),
                                            SignDate = sh.CreateDate
                                        })
                                        .ToList()
                        }).FirstAsync();
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
