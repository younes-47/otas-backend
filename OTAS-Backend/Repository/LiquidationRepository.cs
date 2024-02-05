using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using OTAS.Services;
using OTAS.DTO.Get;
using System.Collections.Generic;
using Azure.Core;

namespace OTAS.Repository
{
    public class LiquidationRepository : ILiquidationRepository
    {
        private readonly OtasContext _context;
        public LiquidationRepository(OtasContext context)
        {
            _context = context;
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
                .Where(lq => lq.AvanceCaisseId != null && lq.AvanceVoyageId != null)
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

        public async Task<List<LiquidationTableDTO>> GetLiquidationsTableForDecider(int deciderUserId)
        {
            /* Get the records that needs to be decided upon now */
            List<LiquidationTableDTO> liquidations = await _context.Liquidations
                            .Where(lq => lq.NextDeciderUserId == deciderUserId)
                            .Where(lq => lq.AvanceCaisseId != null && lq.AvanceVoyageId != null)
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

            /* Get the records that have been already decided upon and add it to the previous list */
            var notMappedliquidations = await _context.StatusHistories
                .Where(sh => sh.DeciderUserId == deciderUserId)
                .Select(sh => sh.Liquidation).ToListAsync();

            List<LiquidationTableDTO> mappedLiquidations = new();

            foreach (var liquidation in notMappedliquidations)
            {
                LiquidationTableDTO mappedLiquidation = new();
                mappedLiquidation.Id = liquidation.Id;
                mappedLiquidation.OnBehalf = liquidation.OnBehalf;
                mappedLiquidation.ActualTotal = liquidation.ActualTotal;
                mappedLiquidation.ReceiptsFileName = liquidation.ReceiptsFileName;
                mappedLiquidation.Result = liquidation.Result;
                mappedLiquidation.CreateDate = liquidation.CreateDate;
                mappedLiquidation.LatestStatus = liquidation.LatestStatusNavigation.StatusString;
                mappedLiquidation.RequestType = liquidation.AvanceCaisseId != null ? "AC" : "AV";
                mappedLiquidation.RequestId = liquidation.AvanceCaisseId != null ? (int)liquidation.AvanceCaisseId : (int)liquidation.AvanceVoyageId;
                mappedLiquidation.Description = liquidation.AvanceCaisseId != null ? liquidation.AvanceCaisse.Description : liquidation.AvanceVoyage.OrdreMission.Description;
                mappedLiquidation.Currency = liquidation.AvanceCaisseId != null ? liquidation.AvanceCaisse.Currency : liquidation.AvanceVoyage.Currency;

                mappedLiquidations.Add(mappedLiquidation);
            }

            liquidations.AddRange(mappedLiquidations);

            return liquidations;
        }

        // Save context state
        public async Task<bool> SaveAsync()
        {
            int saved = await _context.SaveChangesAsync();
            return saved > 0;
        }


    }
}
