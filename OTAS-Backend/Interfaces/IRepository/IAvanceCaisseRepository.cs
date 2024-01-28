﻿using OTAS.DTO.Get;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface IAvanceCaisseRepository
    {
        Task<AvanceCaisse> GetAvanceCaisseByIdAsync(int idavanceCaisseId);
        Task<AvanceCaisse?> FindAvanceCaisseAsync(int avanceCaisseId);
        Task<List<AvanceCaisse>> GetAvancesCaisseByStatusAsync(int status);
        Task<int> GetAvanceCaisseNextDeciderUserId(string currentlevel, bool? isReturnedToFMByTR = false, bool? isReturnedToTRbyFM = false);
        Task<List<AvanceCaisseDTO>> GetAvancesCaisseByUserIdAsync(int userId);
        Task<AvanceCaisseViewDTO> GetAvanceCaisseFullDetailsById(int avanceCaisseId);
        Task<List<AvanceCaisseDTO>> GetAvanceCaissesForDeciderTable(int deciderUserId);
        Task<ServiceResult> AddAvanceCaisseAsync(AvanceCaisse avanceCaisse);
        Task<ServiceResult> UpdateAvanceCaisseStatusAsync(int avanceCaisseId, int status);
        Task<ServiceResult> UpdateAvanceCaisseAsync(AvanceCaisse avanceCaisse);
        Task<ServiceResult> DeleteAvanceCaisseAync(AvanceCaisse avanceCaisse);
        Task<string?> DecodeStatusAsync(int statusCode);
        Task<decimal[]> GetRequesterAllTimeRequestedAmountsByUserIdAsync(int userId);
        Task<DateTime?> GetTheLatestCreatedRequestByUserIdAsync(int userId);
        Task<bool> SaveAsync();

    }
}
