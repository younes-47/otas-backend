using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;
using Microsoft.EntityFrameworkCore;
using OTAS.Services;
using Azure.Core;
using OTAS.DTO.Get;
using AutoMapper;
using System.Linq;
using OTAS.Interfaces.IService;

namespace OTAS.Repository
{
    public class DepenseCaisseRepository : IDepenseCaisseRepository
    {
        //Inject datacontext in the constructor
        private readonly OtasContext _context;
        private readonly IMapper _mapper;
        private readonly IDeciderRepository _deciderRepository;
        private readonly IMiscService _miscService;
        private readonly IUserRepository _userRepository;

        public DepenseCaisseRepository(OtasContext context, IMapper mapper, IDeciderRepository deciderRepository, 
            IMiscService miscService, IUserRepository userRepository)
        {
            _context = context;
            _mapper = mapper;
            _deciderRepository = deciderRepository;
            _miscService = miscService;
            _userRepository = userRepository;
        }

        public async Task<DepenseCaisse?> FindDepenseCaisseAsync(int depenseCaisseId)
        {
            return await _context.DepenseCaisses.FindAsync(depenseCaisseId);
        }

        public async Task<DepenseCaisseViewDTO> GetDepenseCaisseFullDetailsById(int depenseCaisseId)
        {
            return await _context.DepenseCaisses
                .Where(dc => dc.Id == depenseCaisseId)
                .Include(dc => dc.LatestStatusNavigation)
                .Include(dc => dc.StatusHistories)
                .Select(dc => new DepenseCaisseViewDTO
                {
                    Id = dc.Id,
                    Description = dc.Description,
                    OnBehalf = dc.OnBehalf,
                    DeciderComment = dc.DeciderComment,
                    Currency = dc.Currency,
                    Total = dc.Total,
                    ReceiptsFileName = dc.ReceiptsFileName,
                    CreateDate = dc.CreateDate,
                    LatestStatus = dc.LatestStatusNavigation.StatusString,
                    StatusHistory = dc.StatusHistories.Select(sh => new StatusHistoryDTO
                    {
                        Status = sh.StatusNavigation.StatusString,
                        DeciderFirstName = sh.Decider != null ? sh.Decider.FirstName : null,
                        DeciderLastName = sh.Decider != null ? sh.Decider.LastName : null,
                        DeciderComment = sh.DeciderComment,
                        Total = sh.Total,
                        CreateDate = sh.CreateDate
                    }).ToList(),
                    Expenses = _mapper.Map<List<ExpenseDTO>>(dc.Expenses)
                }).FirstAsync();
        }

        public async Task<DepenseCaisseDeciderViewDTO> GetDepenseCaisseFullDetailsByIdForDecider(int depenseCaisseId)
        {
            return await _context.DepenseCaisses
                .Where(ac => ac.Id == depenseCaisseId)
                .Include(ac => ac.LatestStatusNavigation)
                .Include(ac => ac.StatusHistories)
                .Select(ac => new DepenseCaisseDeciderViewDTO
                {
                    Id = ac.Id,
                    UserId = ac.UserId,
                    Description = ac.Description,
                    DeciderComment = ac.DeciderComment != null ? ac.DeciderComment : null,
                    Total = ac.Total,
                    Currency = ac.Currency,
                    OnBehalf = ac.OnBehalf,
                    LatestStatus = ac.LatestStatusNavigation.StatusString,
                    CreateDate = ac.CreateDate,
                    ReceiptsFileName = ac.ReceiptsFileName,
                    StatusHistory = ac.StatusHistories.Select(sh => new StatusHistoryDTO
                    {
                        Status = sh.StatusNavigation.StatusString,
                        DeciderFirstName = sh.Decider != null ? sh.Decider.FirstName : null,
                        DeciderLastName = sh.Decider != null ? sh.Decider.LastName : null,
                        DeciderComment = sh.DeciderComment,
                        Total = sh.Total,
                        CreateDate = sh.CreateDate
                    }).ToList(),
                    Expenses = _mapper.Map<List<ExpenseDTO>>(ac.Expenses),
                })
                .FirstAsync();
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

        public async Task<List<DepenseCaisseDeciderTableDTO>> GetDepenseCaissesForDeciderTable(int deciderUserId)
        {
            /* Get the records that needs to be decided upon now */
            List<DepenseCaisseDeciderTableDTO> depenseCaisses = await _context.DepenseCaisses
                .Include(dc => dc.LatestStatusNavigation)
                .Where(dc => dc.NextDeciderUserId == deciderUserId)
                .Select(dc => new DepenseCaisseDeciderTableDTO
                {
                    Id = dc.Id,
                    Total = dc.Total,
                    ReceiptsFileName= dc.ReceiptsFileName,
                    OnBehalf = dc.OnBehalf,
                    Description = dc.Description,
                    Currency = dc.Currency,
                    IsDecidable = dc.LatestStatusNavigation.StatusString != "Funds Collected" &&
                                  dc.LatestStatusNavigation.StatusString != "Finalized" &&
                                  dc.LatestStatusNavigation.StatusString != "Approved",
                    CreateDate = dc.CreateDate,
                })
                .ToListAsync();


            /* Get the records that have been already decided upon and add it to the previous list */
            if (_context.StatusHistories.Where(sh => sh.DepenseCaisseId != null && sh.DeciderUserId == deciderUserId).Any())
            {
                List<DepenseCaisseDeciderTableDTO> depenseCaisses2 = await _context.StatusHistories
                        .Where(sh => sh.DeciderUserId == deciderUserId && sh.DepenseCaisseId != null)
                        .Include(sh => sh.DepenseCaisse)
                        .Include(sh => sh.StatusNavigation)
                        .Select(sh => new DepenseCaisseDeciderTableDTO
                        {
                            Id = sh.DepenseCaisse.Id,
                            Total = sh.DepenseCaisse.Total,
                            IsDecidable = false,
                            OnBehalf = sh.DepenseCaisse.OnBehalf,
                            ReceiptsFileName = sh.DepenseCaisse.ReceiptsFileName,
                            Description = sh.DepenseCaisse.Description,
                            Currency = sh.DepenseCaisse.Currency,
                            CreateDate = sh.DepenseCaisse.CreateDate,
                        })
                        .ToListAsync();
                depenseCaisses.AddRange(depenseCaisses2);
            }


            return depenseCaisses.Distinct(new DepenseCaisseDeciderTableDTO()).ToList();
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

        public async Task<decimal[]> GetRequesterAllTimeRequestedAmountsByUserIdAsync(int userId)
        {
            decimal[] totalAmounts = new decimal[2];
            decimal AllTimeTotalMAD = 0.0m;
            decimal AllTimeTotalEUR = 0.0m;

            List<decimal> resultMAD = await _context.DepenseCaisses.Where(av => av.UserId == userId).Where(av => av.Currency == "MAD").Select(dc => dc.Total).ToListAsync();
            List<decimal> resultEUR = await _context.DepenseCaisses.Where(av => av.UserId == userId).Where(av => av.Currency == "EUR").Select(dc => dc.Total).ToListAsync();

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
                                        .Include(sh => sh.DepenseCaisse)
                                        .Where(sh => sh.DepenseCaisseId != null && sh.DeciderUserId == userId)
                                        .Where(sh => sh.DepenseCaisse.Currency == "MAD")
                                        .Select(sh => sh.DepenseCaisse.Total)
                                        .ToListAsync();
            List<decimal> resultEUR = await _context.StatusHistories
                                        .Include(sh => sh.DepenseCaisse)
                                        .Where(sh => sh.DepenseCaisseId != null && sh.DeciderUserId == userId)
                                        .Where(sh => sh.DepenseCaisse.Currency == "EUR")
                                        .Select(sh => sh.DepenseCaisse.Total)
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

        public async Task<List<DepenseCaisseDTO>> GetFinalizedDepenseCaissesForDeciderStats(int deciderUserId)
        {
            var temp = await _context.StatusHistories
                .Where(sh => sh.DeciderUserId == deciderUserId)
                .Where(sh => sh.DepenseCaisseId != null)
                .Where(sh => sh.Status == 16)
                .Include(sh => sh.DepenseCaisse)
                .Select(sh => new DepenseCaisseDTO
                {
                    Id = sh.DepenseCaisse.Id,
                    Currency = sh.DepenseCaisse.Currency,
                    Total = sh.DepenseCaisse.Total,
                }).ToListAsync();

            return temp.Distinct(new DepenseCaisseDTO()).ToList();
        }

        public async Task<List<DepenseCaisseDTO>> GetFinalizedDepenseCaissesForRequesterStats(int userId)
        {
            return await _context.DepenseCaisses
                .Where(sh => sh.UserId == userId)
                .Where(sh => sh.LatestStatus == 16)
                .Select(sh => new DepenseCaisseDTO
                {
                    Currency = sh.Currency,
                    Total = sh.Total,
                }).ToListAsync();
        }


        public async Task<DateTime?> GetTheLatestCreatedRequestByUserIdAsync(int userId)
        {
            DateTime? lastCreated = null;

            var latestDepenseVoyage = await _context.DepenseCaisses
                .Where(av => av.UserId == userId)
                .OrderByDescending(av => av.CreateDate)
                .FirstOrDefaultAsync();

            if (latestDepenseVoyage != null)
            {
                lastCreated = latestDepenseVoyage.CreateDate;
            }

            return lastCreated;
        }

        public async Task<DepenseCaisseDocumentDetailsDTO> GetDepenseCaisseDocumentDetailsByIdAsync(int depenseCaisseId)
        {
            int satatusHistoryBreakRowId = _context.StatusHistories
                                                .Where(lq => lq.Status == 1 && lq.DepenseCaisseId == depenseCaisseId)
                                                .OrderByDescending(lq => lq.Id)
                                                .Select(lq => lq.Id)
                                                .First();

            return await _context.DepenseCaisses.Where(ac => ac.Id == depenseCaisseId)
                        .Include(ac => ac.Expenses)
                        .Include(ac => ac.StatusHistories)
                        .Include(ac => ac.User)
                        .Select(ac => new DepenseCaisseDocumentDetailsDTO
                        {
                            Id = ac.Id,
                            FirstName = ac.User.FirstName,
                            LastName = ac.User.LastName,
                            Description = ac.Description,
                            Currency = ac.Currency,
                            SubmitDate = _context.StatusHistories
                                        .Where(lq => lq.DepenseCaisseId == depenseCaisseId && lq.Status == 1)
                                        .OrderByDescending(lq => lq.CreateDate)
                                        .Select(lq => lq.CreateDate)
                                        .First(),
                            Total = ac.Total,
                            Expenses = _mapper.Map<List<ExpenseDTO>>(ac.Expenses),
                            Signers = ac.StatusHistories
                                        .Where(lq => lq.DepenseCaisseId == depenseCaisseId)
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



        public async Task<bool> SaveAsync()
        {
            int result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}
