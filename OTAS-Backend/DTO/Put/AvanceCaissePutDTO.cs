using OTAS.DTO.Post;

namespace OTAS.DTO.Put
{
    public class AvanceCaissePutDTO
    {
        public int Id { get; set; }

        public string Action { get; set; } = null!;

        public bool OnBehalf { get; set; }

        public string Currency { get; set; } = null!;

        public string Description { get; set; } = null!;

        public List<ExpenseNoCurrencyPostDTO> Expenses { get; set; } = new List<ExpenseNoCurrencyPostDTO>();

        public ActualRequesterPostDTO? ActualRequester { get; set; }
    }
}
