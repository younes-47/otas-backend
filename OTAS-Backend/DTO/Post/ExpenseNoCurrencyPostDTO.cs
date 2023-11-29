namespace OTAS.DTO.Post
{
    public class ExpenseNoCurrencyPostDTO
    {
        public int Id { get; set; } //Need Id when resubmitting in case of return

        public decimal EstimatedFee { get; set; }

        public string Description { get; set; } = null!;

        public DateTime ExpenseDate { get; set; }

    }
}
