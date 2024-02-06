using OTAS.DTO.Put;

namespace OTAS.DTO.Post
{
    public class AvanceVoyageLiquidationPostDTO
    {
        public int AvanceVoyageId { get; set; }
        public Byte[] ReceiptsFile { get; set; } = null!;
        public virtual List<TripLiquidationPutDTO> TripsLiquidations { get; set;} = null!;
        public virtual List<ExpenseLiquidationPutDTO> ExpensesLiquidations { get; set; } = new List<ExpenseLiquidationPutDTO>();
        public virtual List<ExpenseNoCurrencyPostDTO> NewExpenses { get; set;} = new List<ExpenseNoCurrencyPostDTO>();
        public virtual List<TripPostDTO> NewTrips { get; set; } = new List<TripPostDTO>();
    }
}
