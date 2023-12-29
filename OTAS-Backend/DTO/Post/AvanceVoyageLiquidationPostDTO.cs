namespace OTAS.DTO.Post
{
    public class AvanceVoyageLiquidationPostDTO
    {
        public int AvanceVoyageId { get; set; }
        public Byte[] ReceiptsFile { get; set; } = null!;
        public virtual List<TripLiquidationPostDTO> TripsLiquidations { get; set;} = null!;
        public virtual List<ExpenseLiquidationPostDTO> ExpensesLiquidations { get; set; } = new List<ExpenseLiquidationPostDTO>();
        public virtual List<ExpensePostDTO> NewExpenses { get; set;} = new List<ExpensePostDTO>();
        public virtual List<TripPostDTO> NewTrips { get; set; } = new List<TripPostDTO>();
    }
}
