﻿using OTAS.Data;
using OTAS.Models;
using OTAS.Interfaces.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using OTAS.Services;
using OTAS.DTO.Get;
using System.Collections.Generic;
using Azure.Core;
using System.Reflection.Metadata.Ecma335;

namespace OTAS.Repository
{
    public class LiquidationRepository : ILiquidationRepository
    {
        private readonly OtasContext _context;
        public LiquidationRepository(OtasContext context)
        {
            _context = context;
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
                reqs.AddRange(await _context.AvanceVoyages.Where(av => av.UserId == userId).Where(av => av.LatestStatus == 10).Select(av => new RequestToLiquidate
                {
                    Id = av.Id,
                    Description = av.OrdreMission.Description,
                }).ToListAsync());
            }

            if (type == "AC")
            {
                reqs.AddRange(await _context.AvanceCaisses.Where(ac => ac.UserId == userId).Where(ac => ac.LatestStatus == 10).Select(ac => new RequestToLiquidate
                {
                    Id = ac.Id,
                    Description = ac.Description,
                }).ToListAsync());
            }

            return reqs;
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
