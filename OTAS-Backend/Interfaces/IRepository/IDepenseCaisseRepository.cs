﻿using OTAS.DTO.Get;
using OTAS.Models;
using OTAS.Services;

namespace OTAS.Interfaces.IRepository
{
    public interface IDepenseCaisseRepository
    {
        Task<DepenseCaisse?> FindDepenseCaisseAsync(int depenseCaisseId);
        Task<DepenseCaisse> GetDepenseCaisseByIdAsync(int id);
        Task<DepenseCaisseViewDTO> GetDepenseCaisseFullDetailsById(int depenseCaisseId);
        Task<ServiceResult> UpdateDepenseCaisseAsync(DepenseCaisse depenseCaisse);
        Task<ServiceResult> AddDepenseCaisseAsync(DepenseCaisse depenseCaisse);
        Task<ServiceResult> UpdateDepenseCaisseStatusAsync(int depesneCaisseId, int status);
        Task<ServiceResult> DeleteDepenseCaisseAync(DepenseCaisse depenseCaisse);
        Task<List<DepenseCaisse>> GetDepensesCaisseByStatus(int status);
        Task<List<DepenseCaisseDTO>> GetDepensesCaisseByUserIdAsync(int userId);
        Task<List<DepenseCaisseDTO>> GetDepenseCaissesForDeciderTable(int deciderUserId);
        Task<int> GetDepenseCaisseNextDeciderUserId(string currentlevel, bool? isReturnedToFMByTR = false, bool? isReturnedToTRbyFM = false);
        Task<bool> SaveAsync();
    }
}
