﻿using OTAS.Data;
using OTAS.Models;
using Microsoft.EntityFrameworkCore;
using OTAS.Interfaces.IRepository;
using OTAS.Services;
using OTAS.DTO.Get;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using OTAS.Interfaces.IService;
using System.Linq;

namespace OTAS.Repository
{
    public class AvanceVoyageRepository : IAvanceVoyageRepository
    {
        private readonly OtasContext _context;
        private readonly IDeciderRepository _deciderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMiscService _miscService;
        private readonly IMapper _mapper;

        public AvanceVoyageRepository(OtasContext context, IDeciderRepository deciderRepository, IUserRepository userRepository, IMiscService miscService, IMapper mapper)
        {
            _context = context;
            _deciderRepository = deciderRepository;
            _userRepository = userRepository;
            _miscService = miscService;
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

        public async Task<List<AvanceVoyageDeciderTableDTO>> GetAvanceVoyagesForDeciderTable(int deciderUserId)
        {
            /* Get the records that needs to be decided upon now */

            bool isDeciderTR = await _context.Deciders.Where(dc => dc.UserId == deciderUserId).Select(dc => dc.Level == "TR").FirstAsync();

            List<AvanceVoyageDeciderTableDTO> avanceVoyages = await _context.AvanceVoyages
                .Include(av => av.LatestStatusNavigation)
                .Include(av => av.OrdreMission)
                .Where(av => (av.NextDeciderUserId == deciderUserId && av.OrdreMission.LatestStatus > av.LatestStatus) || (av.NextDeciderUserId == deciderUserId && isDeciderTR))
                .Select(av => new AvanceVoyageDeciderTableDTO
                {
                    Id = av.Id,
                    EstimatedTotal = av.EstimatedTotal,
                    OnBehalf = av.OnBehalf,
                    OrdreMissionId = av.OrdreMission.Id,
                    OrdreMissionDescription = av.OrdreMission.Description,
                    Currency = av.Currency,
                    IsDecidable = _miscService.IsRequestDecidable(deciderUserId, av.NextDeciderUserId, av.LatestStatusNavigation.StatusString),
                    CreateDate = av.CreateDate,
                })
                .ToListAsync();

            /* Get the records that have been already decided upon and add it to the previous list */
            if (_context.StatusHistories.Where(sh => sh.AvanceVoyageId != null && sh.DeciderUserId == deciderUserId).Any())
            {
                List<AvanceVoyageDeciderTableDTO> avanceVoyages2 = await _context.StatusHistories
                .Where(sh => sh.DeciderUserId == deciderUserId && sh.AvanceVoyageId != null)
                .Include(sh => sh.AvanceVoyage)
                .Include(sh => sh.StatusNavigation)
                .Select(sh => new AvanceVoyageDeciderTableDTO
                {
                    Id = sh.AvanceVoyage.Id,
                    EstimatedTotal = sh.AvanceVoyage.EstimatedTotal,
                    IsDecidable = false,
                    OnBehalf = sh.AvanceVoyage.OnBehalf,
                    OrdreMissionId = sh.AvanceVoyage.OrdreMission.Id,
                    OrdreMissionDescription = sh.AvanceVoyage.OrdreMission.Description,
                    Currency = sh.AvanceVoyage.Currency,
                    CreateDate = sh.AvanceVoyage.CreateDate,
                })
                .ToListAsync();

                avanceVoyages.AddRange(avanceVoyages2);
            }

            return avanceVoyages.Distinct(new AvanceVoyageDeciderTableDTO()).ToList();
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
                .Include(av => av.Trips)
                .Include(av => av.Expenses)
                .Include(av => av.OrdreMission)
                .Include(av => av.StatusHistories)
                .Select(av => new AvanceVoyageViewDTO
                {
                    Id = av.Id,
                    EstimatedTotal = av.EstimatedTotal,
                    DeciderComment = av.DeciderComment,
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
                        DeciderLastName = sh.Decider != null ? sh.Decider.LastName : null,
                        DeciderComment = sh.DeciderComment,
                        Total = sh.Total,
                        CreateDate = sh.CreateDate
                    }).ToList(),
                    Trips = _mapper.Map<List<TripDTO>>(av.Trips),
                    Expenses = _mapper.Map<List<ExpenseDTO>>(av.Expenses)
                })
                .FirstAsync();
        }

        public async Task<AvanceVoyageDocumentDetailsDTO> GetAvanceVoyageDocumentDetailsByIdAsync(int avanceVoyageId)
        {
            int satatusHistoryBreakRowId = _context.StatusHistories
                                                .Where(lq => lq.Status == 1 && lq.AvanceVoyageId == avanceVoyageId)
                                                .OrderByDescending(lq => lq.Id)
                                                .Select(lq => lq.Id)
                                                .First();

            return await _context.AvanceVoyages.Where(ac => ac.Id == avanceVoyageId)
                        .Include(ac => ac.Expenses)
                        .Include(ac => ac.StatusHistories)
                        .Include(ac => ac.User)
                        .Select(ac => new AvanceVoyageDocumentDetailsDTO
                        {
                            Id = ac.Id,
                            FirstName = ac.User.FirstName,
                            LastName = ac.User.LastName,
                            Description = ac.OrdreMission.Description,
                            Currency = ac.Currency,
                            EstimatedTotal = ac.EstimatedTotal,
                            ConfirmationNumber = ac.ConfirmationNumber,
                            SubmitDate = _context.StatusHistories
                                        .Where(lq => lq.AvanceVoyageId == avanceVoyageId && lq.Status == 1)
                                        .OrderByDescending(lq => lq.CreateDate)
                                        .Select(lq => lq.CreateDate)
                                        .First(),
                            Expenses = _mapper.Map<List<ExpenseDTO>>(ac.Expenses),
                            Trips = _mapper.Map<List<TripDTO>>(ac.Trips),
                            Signers = ac.StatusHistories
                                        .Where(lq => lq.AvanceVoyageId == avanceVoyageId)
                                        .Where(lq => lq.DeciderUserId != null)
                                        .Where(lq => lq.Id > satatusHistoryBreakRowId)
                                        .Select(sh => new Signatory
                                        {
                                            FirstName = sh.Decider.FirstName,
                                            LastName = sh.Decider.LastName,
                                            SignatureImageName = _context.Deciders.Where(d => d.UserId == sh.Decider.Id).Select(d => d.SignatureImageName).FirstOrDefault(),
                                            Level = _miscService.GetDeciderLevelByStatus(sh.Status, false),
                                            SignDate = sh.CreateDate
                                        })
                                        .ToList()
                        }).FirstAsync();
        }


        public async Task<AvanceVoyageDeciderViewDTO> GetAvanceVoyageFullDetailsByIdForDecider(int avanceVoyageId)
        {
            return await _context.AvanceVoyages
                .Where(av => av.Id == avanceVoyageId)
                .Include(av => av.LatestStatusNavigation)
                .Include(av => av.OrdreMission)
                .Include(av => av.StatusHistories)
                .Select(av => new AvanceVoyageDeciderViewDTO
                {
                    Id = av.Id,
                    UserId = av.UserId,
                    OrdreMissionDescription = av.OrdreMission.Description,
                    OrdreMissionId = av.OrdreMissionId,
                    EstimatedTotal = av.EstimatedTotal,
                    ActualTotal = av.ActualTotal,
                    Currency = av.Currency,
                    OnBehalf = av.OnBehalf,
                    LatestStatus = av.LatestStatusNavigation.StatusString,
                    CreateDate = av.CreateDate,
                    StatusHistory = av.StatusHistories.Select(sh => new StatusHistoryDTO
                    {
                        Status = sh.StatusNavigation.StatusString,
                        DeciderFirstName = sh.Decider != null ? sh.Decider.FirstName : null,
                        DeciderLastName = sh.Decider != null ? sh.Decider.LastName : null,
                        DeciderComment = sh.DeciderComment,
                        Total = sh.Total,
                        CreateDate = sh.CreateDate
                    }).ToList(),
                    Trips = _mapper.Map<List<TripDTO>>(av.Trips),
                    Expenses = _mapper.Map<List<ExpenseDTO>>(av.Expenses),
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

        public async Task<ServiceResult> UpdateAvanceVoyageStatusAsync(int avanceVoyageId, int status, int nextDeciderUserId)
        {
            ServiceResult result = new();

            AvanceVoyage updatedAvanceVoyage = await GetAvanceVoyageByIdAsync(avanceVoyageId);
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

        public async Task<decimal[]> GetRequesterAllTimeRequestedAmountsByUserIdAsync(int userId)
        {
            decimal[] totalAmounts = new decimal[2];
            decimal AllTimeTotalMAD = 0.0m;
            decimal AllTimeTotalEUR = 0.0m;

            List<decimal> resultMAD = await _context.AvanceVoyages.Where(av => av.UserId == userId).Where(av => av.Currency == "MAD").Select(av => av.EstimatedTotal).ToListAsync();
            List<decimal> resultEUR = await _context.AvanceVoyages.Where(av => av.UserId == userId).Where(av => av.Currency == "EUR").Select(av => av.EstimatedTotal).ToListAsync();

            if (resultMAD.Count > 0)
            {
                foreach (var item in resultMAD)
                {
                    AllTimeTotalMAD += item;
                }
            }
            if (resultEUR.Count > 0)
            {
                foreach (var item in resultMAD)
                {
                    AllTimeTotalEUR += item;
                }
            }

            totalAmounts[0] = AllTimeTotalMAD;
            totalAmounts[1] = AllTimeTotalEUR;

            return totalAmounts;
        }

        public async Task<decimal[]> GetDeciderAllTimeDecidedUponAmountsByUserIdAsync(int userId)
        {
            decimal[] totalAmounts = new decimal[2];
            decimal AllTimeTotalMAD = 0.0m;
            decimal AllTimeTotalEUR = 0.0m;

            List<decimal> resultMAD = await _context.StatusHistories
                                        .Include(sh => sh.AvanceVoyage)
                                        .Where(sh => sh.AvanceVoyageId != null && sh.DeciderUserId == userId)
                                        .Where(sh => sh.AvanceVoyage.Currency == "MAD")
                                        .Select(sh => sh.AvanceVoyage.EstimatedTotal)
                                        .ToListAsync();
            List<decimal> resultEUR = await _context.StatusHistories
                                        .Include(sh => sh.AvanceVoyage)
                                        .Where(sh => sh.AvanceVoyageId != null && sh.DeciderUserId == userId)
                                        .Where(sh => sh.AvanceVoyage.Currency == "EUR")
                                        .Select(sh => sh.AvanceVoyage.EstimatedTotal)
                                        .ToListAsync();

            if (resultMAD.Count > 0)
            {
                foreach (var item in resultMAD)
                {
                    AllTimeTotalMAD += item;
                }
            }
            if (resultEUR.Count > 0)
            {
                foreach (var item in resultMAD)
                {
                    AllTimeTotalEUR += item;
                }
            }

            totalAmounts[0] = AllTimeTotalMAD;
            totalAmounts[1] = AllTimeTotalEUR;

            return totalAmounts;
        }

        public async Task<DateTime?> GetTheLatestCreatedRequestByUserIdAsync(int userId)
        {
            DateTime? lastCreated = null;

            var latestAvanceVoyage = await _context.AvanceVoyages
                .Where(av => av.UserId == userId)
                .OrderByDescending(av => av.CreateDate)
                .FirstOrDefaultAsync();

            if (latestAvanceVoyage != null)
            {
                lastCreated = latestAvanceVoyage.CreateDate;
            }

            return lastCreated;
        }
        public async Task<bool> SaveAsync()
        {
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }

        public async Task<LiquidationRequestDetailsDTO> GetAvanceVoyageDetailsForLiquidationAsync(int requestId)
        {
            return await _context.AvanceVoyages.Where(av => av.Id == requestId).Select(av => new LiquidationRequestDetailsDTO
            {
                Id = av.Id,
                OnBehalf = av.OnBehalf,
                Description = av.OrdreMission.Description,
                Currency = av.Currency,
                EstimatedTotal = av.EstimatedTotal,
                CreateDate = av.CreateDate,
                Expenses = _mapper.Map<List<ExpenseDTO>>(av.Expenses),
                Trips = _mapper.Map<List<TripDTO>>(av.Trips),
            }).FirstAsync();
        }

        public async Task<List<AvanceVoyageViewDTO>> GetFinalizedAvanceVoyagesForDeciderStats(int deciderUserId)
        {
            var temp =  await _context.StatusHistories
                .Where(sh => sh.DeciderUserId == deciderUserId)
                .Where(sh => sh.AvanceVoyageId != null)
                .Where(sh => sh.AvanceVoyage.LatestStatus == 10)
                .Include(sh => sh.AvanceVoyage)
                .Select(sh => new AvanceVoyageViewDTO
                {
                    Id = sh.AvanceVoyage.Id,
                    Currency = sh.AvanceVoyage.Currency,
                    EstimatedTotal = sh.AvanceVoyage.EstimatedTotal,
                }).ToListAsync();

            return temp.Distinct(new AvanceVoyageViewDTO()).ToList();
        }

        public async Task<List<AvanceVoyageViewDTO>> GetFinalizedAvanceVoyagesForRequesterStats(int userId)
        {
            return await _context.AvanceVoyages
                .Where(sh => sh.UserId == userId)
                .Where(sh => sh.LatestStatus == 10)
                .Select(sh => new AvanceVoyageViewDTO
                {
                    Currency = sh.Currency,
                    EstimatedTotal = sh.EstimatedTotal,
                }).ToListAsync();
        }



    }
}
