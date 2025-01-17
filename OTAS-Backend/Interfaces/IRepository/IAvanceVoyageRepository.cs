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
        Task<AvanceVoyageViewDTO> GetAvancesVoyageViewDetailsByIdAsync(int id);
        Task<AvanceVoyageDocumentDetailsDTO> GetAvanceVoyageDocumentDetailsByIdAsync(int avanceVoyageId);
        Task<AvanceVoyageDeciderViewDTO> GetAvanceVoyageFullDetailsByIdForDecider(int avanceVoyageId);
        Task<List<AvanceVoyage>> GetAvancesVoyageByStatusAsync(int status);
        Task<List<AvanceVoyage>> GetAvancesVoyageByOrdreMissionIdAsync(int ordreMissionId);
        Task<int> GetAvanceVoyageNextDeciderUserId(string currentlevel, bool? isLongerThanOneDay = false);
        Task<List<AvanceVoyageDeciderTableDTO>> GetAvanceVoyagesForDeciderTable(int deciderUserId);
        Task<decimal[]> GetRequesterAllTimeRequestedAmountsByUserIdAsync(int userId);
        Task<decimal[]> GetDeciderAllTimeDecidedUponAmountsByUserIdAsync(int userId);
        Task<DateTime?> GetTheLatestCreatedRequestByUserIdAsync(int userId);
        Task<LiquidationRequestDetailsDTO> GetAvanceVoyageDetailsForLiquidationAsync(int requestId);
        Task<List<AvanceVoyageViewDTO>> GetFinalizedAvanceVoyagesForDeciderStats(int deciderUserId);
        Task<List<AvanceVoyageViewDTO>> GetFinalizedAvanceVoyagesForRequesterStats(int userId);

        //Post Methods
        Task<ServiceResult> AddAvanceVoyageAsync(AvanceVoyage voyage);
        

        //Put Methods
        Task<ServiceResult> UpdateAvanceVoyageStatusAsync(int avanceVoyageId, int status, int nextDeciderUserId);
        Task<ServiceResult> UpdateAvanceVoyageAsync(AvanceVoyage avanceVoyage);


        Task<ServiceResult> DeleteAvanceVoyagesRangeAsync(List<AvanceVoyage> avanceVoyages);
        Task<bool> SaveAsync();
    }
}
