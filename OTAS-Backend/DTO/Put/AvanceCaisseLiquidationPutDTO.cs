using OTAS.DTO.Post;

namespace OTAS.DTO.Put
{
    public class AvanceCaisseLiquidationPutDTO
    {
        public Byte[]? ReceiptsFile { get; set; }
        public virtual List<ExpenseLiquidationPutDTO> ExpensesLiquidations { get; set; } = null!;
        public virtual List<ExpenseNoCurrencyPostDTO> NewExpenses { get; set; } = new List<ExpenseNoCurrencyPostDTO>();
    }
}
