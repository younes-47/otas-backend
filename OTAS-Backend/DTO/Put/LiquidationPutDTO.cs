using Microsoft.AspNetCore.Mvc.ModelBinding;
using OTAS.DTO.Post;

namespace OTAS.DTO.Put
{
    public class LiquidationPutDTO
    {
        public int Id { get; set; }

        public string Action { get; set; } = null!;

        public int RequestId { get; set; }

        public string RequestType { get; set; } = null!;

        public Byte[]? ReceiptsFile { get; set; }

        public virtual List<TripLiquidationPutDTO> TripsLiquidations { get; set; } = new List<TripLiquidationPutDTO>();

        public virtual List<ExpenseLiquidationPutDTO> ExpensesLiquidations { get; set; } = new List<ExpenseLiquidationPutDTO>();

        public virtual List<ExpenseNoCurrencyPostDTO> NewExpenses { get; set; } = new List<ExpenseNoCurrencyPostDTO>();

        public virtual List<TripPostDTO> NewTrips { get; set; } = new List<TripPostDTO>();

    }
}
