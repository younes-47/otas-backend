using OTAS.DTO.Put;

namespace OTAS.DTO.Post
{
    public class LiquidationPostDTO
    {
        public int RequestId { get; set; }

        public string RequestType { get; set; } = null!;

        public Byte[] ReceiptsFile { get; set; } = null!;

        public virtual List<TripLiquidationPutDTO> TripsLiquidations { get; set; } = new List<TripLiquidationPutDTO>();

        public virtual List<ExpenseLiquidationPutDTO> ExpensesLiquidations { get; set; } = new List<ExpenseLiquidationPutDTO>();

        public virtual List<ExpenseNoCurrencyPostDTO> NewExpenses { get; set; } = new List<ExpenseNoCurrencyPostDTO>();

        public virtual List<TripPostDTO> NewTrips { get; set; } = new List<TripPostDTO>();
    }
}
