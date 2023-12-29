namespace OTAS.DTO.Post
{
    public class AvanceCaissePostDTO
    {
        public bool OnBehalf { get; set; }

        public string Currency { get; set; } = null!;

        public string Description { get; set; } = null!;

        public List<ExpenseNoCurrencyPostDTO> Expenses { get; set; } = null!;

        public ActualRequesterPostDTO? ActualRequester { get; set; }
    }
}
