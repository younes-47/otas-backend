﻿using OTAS.DTO.Get;
using OTAS.Models;

namespace OTAS.Interfaces.IService
{
    public interface IMiscService
    {
        decimal CalculateExpensesEstimatedTotal(List<Expense> expenses);
        decimal CalculateTripEstimatedFee(Trip trip);
        decimal CalculateTripsEstimatedTotal(List<Trip> trips);
        string GenerateRandomString(int length);
        int GenerateRandomNumber(int length);

        List<StatusHistoryDTO> IllustrateStatusHistory(List<StatusHistoryDTO> statusHistories);
        string GetDeciderLevelByStatus(int status, string requestType);
    }
}
