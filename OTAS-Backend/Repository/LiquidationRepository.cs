using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using OTAS.Services;
using OTAS.DTO.Get;
using System.Collections.Generic;

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
                    AvanceCaisseId = lq.AvanceCaisse != null ? lq.AvanceCaisse.Id : null,
                    AvanceCaisseDescription = lq.AvanceCaisse != null ? lq.AvanceCaisse.Description : null,
                    AvanceVoyageId = lq.AvanceVoyage != null ? lq.AvanceVoyage.Id : null,
                    AvanceVoyageDescription = lq.AvanceVoyage != null ? lq.AvanceVoyage.OrdreMission.Description : null,
                }).ToListAsync();
              
            return liquidations;
        }

        public async Task<List<LiquidationTableDTO>> GetLiquidationsTableForDecider(int deciderUserId)
        {
            /* Get the records that needs to be decided upon now */
            List<LiquidationTableDTO> liquidations = await _context.Liquidations
                            .Where(lq => lq.NextDeciderUserId == deciderUserId)
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
                                AvanceCaisseId = lq.AvanceCaisse != null ? lq.AvanceCaisse.Id : null,
                                AvanceCaisseDescription = lq.AvanceCaisse != null ? lq.AvanceCaisse.Description : null,
                                AvanceVoyageId = lq.AvanceVoyage != null ? lq.AvanceVoyage.Id : null,
                                AvanceVoyageDescription = lq.AvanceVoyage != null ? lq.AvanceVoyage.OrdreMission.Description : null,
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
                mappedLiquidation.AvanceCaisseId = liquidation.AvanceCaisse?.Id;
                mappedLiquidation.AvanceCaisseDescription = liquidation.AvanceCaisse?.Description;
                mappedLiquidation.AvanceVoyageId = liquidation.AvanceVoyage?.Id;
                mappedLiquidation.AvanceVoyageDescription = liquidation.AvanceVoyage?.OrdreMission.Description;

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
