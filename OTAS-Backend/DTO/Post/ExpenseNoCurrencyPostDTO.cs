namespace OTAS.DTO.Post
{
    public class ExpenseNoCurrencyPostDTO
    {
        public decimal EstimatedFee { get; set; }

        public string Description { get; set; } = null!;

        public DateTime ExpenseDate { get; set; }

    }
}
