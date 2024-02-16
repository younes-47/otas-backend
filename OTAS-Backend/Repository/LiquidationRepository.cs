using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using OTAS.Services;
using OTAS.DTO.Get;
using System.Collections.Generic;
using Azure.Core;
using System.Reflection.Metadata.Ecma335;
using OTAS.Interfaces.IService;
using Microsoft.Extensions.Logging.Console;

namespace OTAS.Repository
{
    public class LiquidationRepository : ILiquidationRepository
    {
        private readonly OtasContext _context;
        private readonly IDeciderRepository _deciderRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMiscService _miscService;

        public LiquidationRepository(OtasContext context, IDeciderRepository deciderRepository,
                                IUserRepository userRepository, IMiscService miscService)
        {
            _context = context;
            _deciderRepository = deciderRepository;
            _userRepository = userRepository;
            _miscService = miscService;
        }

        public async Task<Liquidation?> FindLiquidationAsync(int liquidationId)
        {
            return await _context.Liquidations.FindAsync(liquidationId);
        }

        public async Task<ServiceResult> DeleteLiquidationAync(Liquidation liquidation)
        {
            ServiceResult result = new();
            _context.Remove(liquidation);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "Liquidation has been deleted successfully" : "Something went wrong while deleting the Liquidation from DB.";
            return result;
        }

        public async Task<LiquidationViewDTO> GetLiquidationFullDetailsByIdForRequester(int liquidationId)
        {
            return await _context.Liquidations
                .Where(lq => lq.Id == liquidationId)
                .Include(lq => lq.AvanceCaisse)
                .Include(lq => lq.AvanceVoyage)
                .Include(lq => lq.LatestStatusNavigation)
                .Include(lq => lq.StatusHistories)
                .Select(lq => new LiquidationViewDTO
                {
                    Id = lq.Id,
                    RequestId = lq.AvanceVoyageId != null ? (int)lq.AvanceVoyageId : (int)lq.AvanceCaisseId,
                    RequestType = lq.AvanceVoyageId != null ? "AV" : "AC",
                    ActualTotal = (decimal)lq.ActualTotal,
                    Result = lq.Result,
                    OnBehalf = lq.OnBehalf,
                    DeciderComment = lq.DeciderComment,
                    CreateDate = lq.CreateDate,
                    LatestStatus = lq.LatestStatusNavigation.StatusString,
                    ReceiptsFileName = lq.ReceiptsFileName,
                    StatusHistory = lq.StatusHistories.Select(sh => new StatusHistoryDTO
                    {
                        Status = sh.StatusNavigation.StatusString,
                        DeciderFirstName = sh.Decider != null ? sh.Decider.FirstName : null,
                        DeciderLastName = sh.Decider != null ? sh.Decider.LastName : null,
                        DeciderComment = sh.DeciderComment,
                        Total = sh.Total,
                        CreateDate = sh.CreateDate
                    }).ToList(),
                }).FirstAsync();
        }


        public async Task<Liquidation> GetLiquidationByIdAsync(int id)
        {
            return await _context.Liquidations.Where(lq => lq.Id == id).FirstAsync();
        }

        public async Task<ServiceResult> AddLiquidationAsync(Liquidation liquidation)
        {
            ServiceResult result = new();
            await _context.AddAsync(liquidation);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "Liquidation has been added successfully" : "Something went wrong while adding the Liquidation";
            return result;
        }

        public async Task<List<LiquidationTableDTO>> GetLiquidationsTableForRequster(int userId)
        {
            List<LiquidationTableDTO> liquidations = await _context.Liquidations
                .Where(lq => lq.UserId == userId)
                .Include(lq => lq.LatestStatusNavigation)
                .Include(lq => lq.AvanceCaisse)
                .Include(lq => lq.AvanceVoyage)
                .Select(lq => new LiquidationTableDTO()
                {
                    Id = lq.Id,
                    OnBehalf = lq.OnBehalf,
                    ActualTotal = lq.ActualTotal,
                    ReceiptsFileName = lq.ReceiptsFileName,
                    Result = lq.Result,
                    CreateDate = lq.CreateDate,
                    LatestStatus = lq.LatestStatusNavigation.StatusString,
                    RequestType = lq.AvanceCaisseId != null ? "AC" : "AV",
                    RequestId = lq.AvanceCaisseId != null ? (int)lq.AvanceCaisseId : (int)lq.AvanceVoyageId,
                    Description = lq.AvanceCaisseId != null ? lq.AvanceCaisse.Description : lq.AvanceVoyage.OrdreMission.Description,
                    Currency = lq.AvanceCaisseId != null ? lq.AvanceCaisse.Currency : lq.AvanceVoyage.Currency,
                }).ToListAsync();
              
            return liquidations;
        }

        public async Task<List<RequestToLiquidate>> GetRequestsToLiquidatesByTypeAsync(string type, int userId)
        {
            List<RequestToLiquidate> reqs = new();
            if(type == "AV")
            {
                reqs.AddRange(await _context.AvanceVoyages
                    .Include(av => av.Liquidation)
                    .Where(av => av.UserId == userId)
                    .Where(av => av.LatestStatus == 10)
                    .Where(av => av.Liquidation == null && av.Liquidation.AvanceVoyage == null && av.Liquidation.AvanceVoyageId != av.Id)
                    .Select(av => new RequestToLiquidate
                {
                    Id = av.Id,
                    Description = av.OrdreMission.Description,
                }).ToListAsync());
            }

            if (type == "AC")
            {
                reqs.AddRange(await _context.AvanceCaisses
                    .Include(av => av.Liquidation)
                    .Where(ac => ac.UserId == userId)
                    .Where(ac => ac.LatestStatus == 10)
                    .Where(av => av.Liquidation == null && av.Liquidation.AvanceCaisse == null && av.Liquidation.AvanceCaisseId != av.Id)
                    .Select(ac => new RequestToLiquidate
                {
                    Id = ac.Id,
                    Description = ac.Description,
                }).ToListAsync());
            }

            return reqs;
        }

        public async Task<List<LiquidationDeciderTableDTO>> GetLiquidationsTableForDecider(int deciderUserId)
        {
            /* Get the records that needs to be decided upon now */
            List<LiquidationDeciderTableDTO> liquidations = await _context.Liquidations
                .Include(lq => lq.LatestStatusNavigation)
                .Include(lq => lq.AvanceCaisse)
                .Include(lq => lq.AvanceVoyage)
                .Where(lq => lq.NextDeciderUserId == deciderUserId)
                .Select(lq => new LiquidationDeciderTableDTO
                {
                    Id = lq.Id,
                    RequestType = lq.AvanceCaisseId != null ? "AC" : "AV",
                    RequestId = lq.AvanceCaisseId != null ? (int)lq.AvanceCaisseId : (int)lq.AvanceVoyageId,
                    ActualTotal = lq.ActualTotal,
                    IsDecidable = lq.LatestStatusNavigation.StatusString != "Funds Collected" &&
                                  lq.LatestStatusNavigation.StatusString != "Finalized" &&
                                  lq.LatestStatusNavigation.StatusString != "Approved",
                    ReceiptsFileName = lq.ReceiptsFileName,
                    OnBehalf = lq.OnBehalf,
                    Description = lq.AvanceCaisseId != null ? lq.AvanceCaisse.Description : lq.AvanceVoyage.OrdreMission.Description,
                    Currency = lq.Currency,
                    CreateDate = lq.CreateDate,
                    Result = lq.Result,
                })
                .ToListAsync();


            /* Get the records that have been already decided upon and add it to the previous list */
            if (_context.StatusHistories.Where(sh => sh.LiquidationId != null && sh.DeciderUserId == deciderUserId).Any())
            {
                List<LiquidationDeciderTableDTO> liquidations2 = await _context.StatusHistories
                        .Where(sh => sh.DeciderUserId == deciderUserId && sh.LiquidationId != null)
                        .Include(sh => sh.Liquidation)
                        .Include(sh => sh.StatusNavigation)
                        .Select(sh => new LiquidationDeciderTableDTO
                        {
                            Id = sh.Liquidation.Id,
                            RequestType = sh.Liquidation.AvanceCaisseId != null ? "AC" : "AV",
                            RequestId = sh.Liquidation.AvanceCaisseId != null ? (int)sh.Liquidation.AvanceCaisseId : (int)sh.Liquidation.AvanceVoyageId,
                            ActualTotal = sh.Liquidation.ActualTotal,
                            ReceiptsFileName = sh.Liquidation.ReceiptsFileName,
                            OnBehalf = sh.Liquidation.OnBehalf,
                            Description = sh.Liquidation.AvanceCaisseId != null ? sh.Liquidation.AvanceCaisse.Description : sh.Liquidation.AvanceVoyage.OrdreMission.Description,
                            Currency = sh.Liquidation.Currency,
                            CreateDate = sh.Liquidation.CreateDate,
                            IsDecidable = false,
                            Result = sh.Liquidation.Result,
                        })
                        .ToListAsync();
                liquidations.AddRange(liquidations2);
            }


            return liquidations.Distinct(new LiquidationDeciderTableDTO()).ToList();
        }

        public async Task<ServiceResult> UpdateLiquidationAsync(Liquidation liquidation)
        {
            ServiceResult result = new();
            _context.Update(liquidation);
            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "Liquidation has been updated successfully" : "Something went wrong while updating the liquidation";
            return result;
        }

        public async Task<int> GetLiquidationNextDeciderUserId(string currentlevel, bool? isReturnedToFMByTR = false, bool? isReturnedToTRbyFM = false)
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

        public async Task<LiquidationDeciderViewDTO> GetLiquidationFullDetailsByIdForDecider(int liquidationId)
        {
            return await _context.Liquidations
                .Where(lq => lq.Id == liquidationId)
                .Include(lq => lq.LatestStatusNavigation)
                .Include(lq => lq.StatusHistories)
                .Include(lq => lq.AvanceCaisse)
                .Include(lq => lq.AvanceVoyage)
                .Select(lq => new LiquidationDeciderViewDTO
                {
                    Id = lq.Id,
                    UserId = lq.UserId,
                    RequestId = lq.AvanceVoyageId != null ? (int)lq.AvanceVoyageId : (int)lq.AvanceCaisseId,
                    RequestType = lq.AvanceVoyageId != null ? "AV" : "AC",
                    DeciderComment = lq.DeciderComment != null ? lq.DeciderComment : null,
                    ActualTotal = lq.ActualTotal,
                    Result = lq.Result,
                    OnBehalf = lq.OnBehalf,
                    LatestStatus = lq.LatestStatusNavigation.StatusString,
                    CreateDate = lq.CreateDate,
                    ReceiptsFileName = lq.ReceiptsFileName,
                    StatusHistory = lq.StatusHistories.Select(sh => new StatusHistoryDTO
                    {
                        Status = sh.StatusNavigation.StatusString,
                        DeciderFirstName = sh.Decider != null ? sh.Decider.FirstName : null,
                        DeciderLastName = sh.Decider != null ? sh.Decider.LastName : null,
                        DeciderComment = sh.DeciderComment,
                        Total = sh.Total,
                        CreateDate = sh.CreateDate
                    }).ToList(),
                })
                .FirstAsync();
        }

        // Save context state
        public async Task<bool> SaveAsync()
        {
            int saved = await _context.SaveChangesAsync();
            return saved > 0;
        }


    }
}
