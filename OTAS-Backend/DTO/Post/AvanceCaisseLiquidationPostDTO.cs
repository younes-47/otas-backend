namespace OTAS.DTO.Post
{
    public class AvanceCaisseLiquidationPostDTO
    {
        public int AvanceCaisseId { get; set; }
        public Byte[] ReceiptsFile { get; set; } = null!;
        public virtual List<ExpenseLiquidationPostDTO> ExpensesLiquidations { get; set; } = null!;
        public virtual List<ExpensePostDTO> NewExpenses { get; set; } = new List<ExpensePostDTO>();
    }
}
