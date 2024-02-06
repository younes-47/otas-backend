using OTAS.DTO.Put;

namespace OTAS.DTO.Post
{
    public class AvanceCaisseLiquidationPostDTO
    {
        public int AvanceCaisseId { get; set; }
        public Byte[] ReceiptsFile { get; set; } = null!;
        public virtual List<ExpenseLiquidationPutDTO> ExpensesLiquidations { get; set; } = null!;
        public virtual List<ExpenseNoCurrencyPostDTO> NewExpenses { get; set; } = new List<ExpenseNoCurrencyPostDTO>();
    }
}
