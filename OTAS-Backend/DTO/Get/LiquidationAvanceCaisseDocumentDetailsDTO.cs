namespace OTAS.DTO.Get
{
    public class LiquidationAvanceCaisseDocumentDetailsDTO
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public decimal ActualTotal { get; set; }

        public decimal EstimatedTotal { get; set; }

        public decimal Result { get; set;}

        public DateTime SubmitDate { get; set; }

        public List<Signatory> Signers { get; set; } = new List<Signatory>();

        public virtual List<ExpenseDTO> Expenses { get; set; } = new List<ExpenseDTO>();

    }
}
