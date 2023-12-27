namespace OTAS.DTO.Post
{
    public class AvanceCaissePostDTO
    {
        public int Id { get; set; } //Need Id when resubmitting in case of modification

        public bool OnBehalf { get; set; }

        public string Currency { get; set; } = null!;

        public string Description { get; set; } = null!;

        public List<ExpenseNoCurrencyPostDTO> Expenses { get; set; } = new List<ExpenseNoCurrencyPostDTO>();

        public ActualRequesterPostDTO? ActualRequester { get; set; }
    }
}
