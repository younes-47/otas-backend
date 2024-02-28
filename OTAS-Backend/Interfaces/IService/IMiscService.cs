using OTAS.DTO.Get;
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
        string GetDeciderLevelByStatus(int status, bool? isRequestOM = false);

        bool IsRequestDecidable(string deciderUsername, string nextDeciderUsername, string latestStatus);

        Xceed.Document.NET.Table GenerateExpesnesTableForDocuments(Xceed.Words.NET.DocX docx, List<ExpenseDTO> expenses);

        Xceed.Document.NET.Table GenerateExpesnesTableForLiquidationDocuments(Xceed.Words.NET.DocX docx, List<ExpenseDTO> expenses);

        Xceed.Document.NET.Table GenerateTripsTableForDocuments(Xceed.Words.NET.DocX docx, List<TripDTO> trips);

        Xceed.Document.NET.Table GenerateTripsTableForLiquidationDocuments(Xceed.Words.NET.DocX docx, List<TripDTO> trips);

        string GenerateEmailBodyEnglish(string requestType, int requestId, string deciderFullName);

        string GenerateEmailBodyFrench(string requestType, int requestId, string deciderFullName);

    }
}
