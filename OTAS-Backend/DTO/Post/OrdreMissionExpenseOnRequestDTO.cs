namespace OTAS.DTO.Post
{
    public class OrdreMissionExpenseOnRequestDTO
    {
        public decimal EstimatedFee { get; set; }

        public string Description { get; set; } = null!;

        public DateTime ExpenseDate { get; set; }

        public string Currency { get; set; } = null!;

    }
}
