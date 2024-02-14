namespace OTAS.DTO.Get
{
    public class AvanceVoyageDocumentDetailsDTO
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public decimal EstimatedTotal { get; set; }

        public int? ConfirmationNumber { get; set; }

        public List<Signatory> Signers { get; set; } = new List<Signatory>();

        public virtual List<TripDTO> Trips { get; set; } = new List<TripDTO>();

        public virtual List<ExpenseDTO> Expenses { get; set; } = new List<ExpenseDTO>();

    }
}
