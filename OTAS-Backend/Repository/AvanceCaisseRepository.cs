using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;
using OTAS.Services;
using Microsoft.EntityFrameworkCore;
using AutoMapper.QueryableExtensions;
using OTAS.DTO.Get;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;

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
        public async Task<List<AvanceCaisseDeciderTableDTO>> GetAvanceCaissesForDeciderTable(int deciderUserId)
        {
            /* Get the records that needs to be decided upon now */
            List<AvanceCaisseDeciderTableDTO> avanceCaisses = await _context.AvanceCaisses
                .Include(ac => ac.LatestStatusNavigation)
                .Where(ac => ac.NextDeciderUserId == deciderUserId)
                .Select(ac => new AvanceCaisseDeciderTableDTO
                {
                    Id = ac.Id,
                    EstimatedTotal = ac.EstimatedTotal,
                    NextDeciderUserName = ac.NextDecider != null ? ac.NextDecider.Username : null,
                    OnBehalf = ac.OnBehalf,
                    Description = ac.Description,
                    Currency = ac.Currency,
                    CreateDate = ac.CreateDate,
                })
                .ToListAsync();


                /* Get the records that have been already decided upon and add it to the previous list */
                if( _context.StatusHistories.Where(sh => sh.AvanceCaisseId != null && sh.DeciderUserId == deciderUserId).Any())
                {
                    List<AvanceCaisseDeciderTableDTO> avanceCaisses2 = await _context.StatusHistories
                            .Where(sh => sh.DeciderUserId == deciderUserId && sh.AvanceCaisseId != null)
                            .Include(sh => sh.AvanceCaisse)
                            .Select(sh => new AvanceCaisseDeciderTableDTO
                            {
                                Id = sh.AvanceCaisse.Id,
                                EstimatedTotal = sh.AvanceCaisse.EstimatedTotal,
                                NextDeciderUserName = sh.AvanceCaisse.NextDecider != null ? sh.AvanceCaisse.NextDecider.Username : null,
                                OnBehalf = sh.AvanceCaisse.OnBehalf,
                                Description = sh.AvanceCaisse.Description,
                                Currency = sh.AvanceCaisse.Currency,
                                CreateDate = sh.AvanceCaisse.CreateDate,
                            })
                            .ToListAsync();
                    avanceCaisses.AddRange(avanceCaisses2);
                }

            return avanceCaisses.Distinct(new AvanceCaisseDeciderTableDTO()).ToList();
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

        public async Task<AvanceCaisseViewDTO> GetAvanceCaisseFullDetailsById(int avanceCaisseId)
        {
            return await _context.AvanceCaisses
                .Where(ac => ac.Id == avanceCaisseId)
                .Include(ac => ac.LatestStatusNavigation)
                .Include(ac => ac.StatusHistories)
                .Select(ac => new AvanceCaisseViewDTO
                {
                    Id = ac.Id,
                    Description = ac.Description,
                    DeciderComment = ac.DeciderComment,
                    OnBehalf = ac.OnBehalf,
                    Currency = ac.Currency,
                    EstimatedTotal = ac.EstimatedTotal,
                    ActualTotal = ac.ActualTotal,
                    CreateDate = ac.CreateDate,
                    LatestStatus = ac.LatestStatusNavigation.StatusString,
                    StatusHistory = ac.StatusHistories.Select(sh => new StatusHistoryDTO
                    {
                        Status = sh.StatusNavigation.StatusString,
                        DeciderFirstName = sh.Decider != null ? sh.Decider.FirstName : null,
                        DeciderLastName = sh.Decider != null ? sh.Decider.LastName : null,
                        DeciderComment = sh.DeciderComment,
                        CreateDate = sh.CreateDate
                    }).ToList(),
                    Expenses = _mapper.Map<List<ExpenseDTO>>(ac.Expenses)
                }).FirstAsync();
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

        public async Task<AvanceCaisseDeciderViewDTO> GetAvanceCaisseFullDetailsByIdForDecider(int avanceCaisseId)
        {
            return await _context.AvanceCaisses
                .Where(ac => ac.Id == avanceCaisseId)
                .Include(ac => ac.LatestStatusNavigation)
                .Include(ac => ac.StatusHistories)
                .Select(ac => new AvanceCaisseDeciderViewDTO
                {
                    Id = ac.Id,
                    UserId = ac.UserId,
                    Description = ac.Description,
                    EstimatedTotal = ac.EstimatedTotal,
                    ActualTotal = ac.ActualTotal,
                    Currency = ac.Currency,
                    OnBehalf = ac.OnBehalf,
                    LatestStatus = ac.LatestStatusNavigation.StatusString,
                    CreateDate = ac.CreateDate,
                    StatusHistory = ac.StatusHistories.Select(sh => new StatusHistoryDTO
                    {
                        Status = sh.StatusNavigation.StatusString,
                        DeciderFirstName = sh.Decider != null ? sh.Decider.FirstName : null,
                        DeciderLastName = sh.Decider != null ? sh.Decider.LastName : null,
                        DeciderComment = sh.DeciderComment,
                        CreateDate = sh.CreateDate
                    }).ToList(),
                    Expenses = _mapper.Map<List<ExpenseDTO>>(ac.Expenses),
                })
                .FirstAsync();
        }

        public async Task<int> GetAvanceCaisseNextDeciderUserId(string currentlevel, bool? isReturnedToFMByTR = false, bool? isReturnedToTRbyFM = false)
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
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("TR");
                    break;

                default:
                    deciderUserId = await _deciderRepository.GetDeciderUserIdByDeciderLevel("TR");
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

        public async Task<decimal[]> GetRequesterAllTimeRequestedAmountsByUserIdAsync(int userId)
        {
            decimal[] totalAmounts = new decimal[2];
            decimal AllTimeTotalMAD = 0.0m;
            decimal AllTimeTotalEUR = 0.0m;

            List<decimal> resultMAD = await _context.AvanceCaisses.Where(av => av.UserId == userId).Where(av => av.Currency == "MAD").Select(av => av.EstimatedTotal).ToListAsync();
            List<decimal> resultEUR = await _context.AvanceCaisses.Where(av => av.UserId == userId).Where(av => av.Currency == "EUR").Select(av => av.EstimatedTotal).ToListAsync();

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

            var latestAvanceCaisse = await _context.AvanceCaisses
                .Where(av => av.UserId == userId)
                .OrderByDescending(av => av.CreateDate)
                .FirstOrDefaultAsync();

            if (latestAvanceCaisse != null)
            {
                lastCreated = latestAvanceCaisse.CreateDate;
            }

            return lastCreated;
        }


        public async Task<string?> DecodeStatusAsync(int statusCode)
        {
            var statusCodeEntity = await _context.StatusCodes.Where(sc => sc.StatusInt == statusCode).FirstOrDefaultAsync();

            return statusCodeEntity?.StatusString;
        }
    }
}
