﻿using OTAS.DTO.Get;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface IAvanceVoyageRepository
    {
        //Get Methods
        Task<AvanceVoyage> GetAvanceVoyageByIdAsync(int id);
        Task<AvanceVoyage?> FindAvanceVoyageByIdAsync(int avanceVoyageId);
        Task<List<AvanceVoyageTableDTO>> GetAvancesVoyageByUserIdAsync(int userId);
        Task<List<AvanceVoyage>> GetAvancesVoyageByStatusAsync(int status);
        Task<List<AvanceVoyage>> GetAvancesVoyageByOrdreMissionIdAsync(int ordreMissionId);
        Task<int> GetAvanceVoyageNextDeciderUserId(string currentlevel, bool? isLongerThanOneDay = false, bool? isReturnedToFMByTR = false, bool? isReturnedToTRbyFM = false);
        Task<List<AvanceVoyageTableDTO>> GetAvanceVoyagesForDeciderTable(int deciderRole);

        //Post Methods
        Task<ServiceResult> AddAvanceVoyageAsync(AvanceVoyage voyage);


        //Put Methods
        Task<ServiceResult> UpdateAvanceVoyageStatusAsync(int avanceVoyageId, int status, int nextDeciderUserId);
        Task<ServiceResult> UpdateAvanceVoyageAsync(AvanceVoyage avanceVoyage);


        Task<ServiceResult> DeleteAvanceVoyagesRangeAsync(List<AvanceVoyage> avanceVoyages);
        Task<bool> SaveAsync();
    }
}
