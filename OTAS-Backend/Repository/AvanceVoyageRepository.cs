﻿using OTAS.Data;
using OTAS.Models;
using Microsoft.EntityFrameworkCore;
using OTAS.Interfaces.IRepository;
using OTAS.Services;
using OTAS.DTO.Get;
using AutoMapper;

namespace OTAS.Repository
{
    public class AvanceVoyageRepository : IAvanceVoyageRepository
    {
        private readonly OtasContext _context;
        private readonly IMapper _mapper;

        public AvanceVoyageRepository(OtasContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // This method should probably be in a service rather than the repo (like AC) but whatever
        public async Task<List<AvanceVoyageTableDTO>> GetAvanceVoyagesForDeciderTable(int deciderRole)
        {
            List<AvanceVoyage> avanceVoyages = await GetAvancesVoyageByStatusAsync(deciderRole - 1); //See oneNote sketches to understand why it is role-1
            List<AvanceVoyageTableDTO> mappedAvanceVoyages = _mapper.Map<List<AvanceVoyageTableDTO>>(avanceVoyages);
            return mappedAvanceVoyages;
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
                .Select(av => new AvanceVoyageTableDTO
                {
                    Id = av.Id,
                    EstimatedTotal = av.EstimatedTotal,
                    ActualTotal = av.ActualTotal,
                    Currency = av.Currency,
                    ConfirmationNumber = av.ConfirmationNumber,
                    LatestStatus = av.LatestStatusNavigation.StatusString,
                    CreateDate = av.CreateDate,
                })
                .ToListAsync();
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

        public async Task <ServiceResult> UpdateAvanceVoyageStatusAsync(int avanceVoyageId, int status)
        {
            ServiceResult result = new();

            AvanceVoyage updatedAvanceVoyage  = await GetAvanceVoyageByIdAsync(avanceVoyageId);
            updatedAvanceVoyage.LatestStatus = status;
            _context.AvanceVoyages.Update(updatedAvanceVoyage);

            result.Success = await SaveAsync();
            result.Message = result.Success == true ? "AvanceVoyage Status Updated Successfully" : "Something went wrong while updating AvanceVoyage Status";

           return result;

        }

        public async Task<bool> SaveAsync()
        {
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }
    }
}
