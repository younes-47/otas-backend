using OTAS.DTO.Post;

namespace OTAS.DTO.Put
{
    public class DepenseCaissePutDTO
    {
        public int Id { get; set; }

        public string Action { get; set; } = null!;

        public bool OnBehalf { get; set; }

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public Byte[]? ReceiptsFile { get; set; } // receipts file is not required during modification
       
        public virtual ActualRequesterPostDTO? ActualRequester { get; set; }

        public virtual List<ExpenseNoCurrencyPostDTO> Expenses { get; set; } = new List<ExpenseNoCurrencyPostDTO>();

    }
}
