namespace OTAS.DTO.Get
{
    public class LiquidationRequestDetailsDTO
    {
        public int Id { get; set; }

        public virtual ActualRequesterDTO? ActualRequester { get; set; }

        public bool OnBehalf { get; set; }

        public string Description { get; set; } = null!;

        public string Currency { get; set; } = null!;

        public decimal EstimatedTotal { get; set; }

        public DateTime CreateDate { get; set; }

        public virtual List<TripDTO> Trips { get; set; } = new List<TripDTO>();

        public virtual List<ExpenseDTO> Expenses { get; set; } = new List<ExpenseDTO>();
    }
}
