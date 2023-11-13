﻿using Microsoft.EntityFrameworkCore;
using OTAS.Data;
using OTAS.Interfaces.IRepository;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Repository
{
    public class OrdreMissionRepository : IOrdreMissionRepository
    {
        private readonly OtasContext _context;

        public OrdreMissionRepository(OtasContext context)
        {
            _context = context;
        }


        
        public async  Task<List<OrdreMission>> GetOrdresMissionByUserIdAsync(int userid)
        {
            return await _context.OrdreMissions.Where(om => om.UserId == userid).ToListAsync();
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
            var statusCodeEntity =  await _context.StatusCodes.Where(sc => sc.StatusInt == statusCode).FirstOrDefaultAsync();

            return statusCodeEntity?.StatusString;
        }

        public async Task<bool> AddOrdreMissionAsync(OrdreMission ordreMission)
        {
            await _context.OrdreMissions.AddAsync(ordreMission);
            return await SaveAsync();
        }

        public async Task<ServiceResult> UpdateOrdreMissionStatusAsync(int ordreMissionId, int status)
        {
                ServiceResult result = new();
               
                OrdreMission updatedOrdreMission = await GetOrdreMissionByIdAsync(ordreMissionId);
                updatedOrdreMission.LatestStatus = status;
                _context.OrdreMissions.Update(updatedOrdreMission);

                result.Success = await SaveAsync();
                if(result.Success)
                {
                    result.SuccessMessage = "Updated Successfully";
                }
                else
                {
                    result.ErrorMessage = "Something went wrong while updating";
                }
            

            return result;
        }

        public async Task<bool> SaveAsync()
        {
            var saved = await _context.SaveChangesAsync();
            return saved > 0;
        }
    }
}